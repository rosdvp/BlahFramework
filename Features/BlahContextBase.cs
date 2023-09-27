using System;
using System.Collections.Generic;
using Blah.Injection;
using Blah.Ordering;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features
{
public abstract class BlahContextBase
{
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private BlahPoolsContext _poolsContext = new();
	
	private BlahServicesContext _servicesContext;
	private BlahSystemsContext  _systemsContext;

	private bool _isRequestedSwitchSystemsGroupWithPoolsRemoveAll;

	public void Init(IBlahServicesInitData servicesInitData, IBlahSystemsInitData systemsInitData)
	{
		_servicesContext = new BlahServicesContext(servicesInitData);
		_systemsContext  = new BlahSystemsContext(systemsInitData);

		foreach ((int groupId, var features) in FeaturesBySystemsGroups)
		foreach (var feature in features)
		{
#if UNITY_EDITOR
			BlahFeaturesValidator.Validate(feature);
#endif
			if (feature.Services != null)
				foreach (var serviceType in feature.Services)
					_servicesContext.TryAdd(serviceType, (BlahServiceBase)Activator.CreateInstance(serviceType));
		}
		_servicesContext.FinalizeInit();

		var injector = BuildInjector();
		
		var tempSystemsTypes = new List<Type>();
		foreach ((int groupId, var features) in FeaturesBySystemsGroups)
		{
			tempSystemsTypes.Clear();

			foreach (var feature in features)
			foreach (var systemType in feature.Systems)
				tempSystemsTypes.Add(systemType);

			try
			{
				BlahOrderer.Order(ref tempSystemsTypes);
			}
			catch (BlahOrdererSortingException e)
			{
				throw new Exception(e.GetFullMsg());
			}

			var group = _systemsContext.AddGroup(groupId);
			foreach (var systemType in tempSystemsTypes)
			{
				var system = (IBlahSystem)Activator.CreateInstance(systemType);
				injector.InjectInto(system);
				group.AddSystem(system);
			}
		}
	}
	

	public BlahPoolsContext    Pools    => _poolsContext;
	public BlahServicesContext Services => _servicesContext;

	public void Run()
	{
		if (_systemsContext.IsSwitchGroupRequested &&
		    _isRequestedSwitchSystemsGroupWithPoolsRemoveAll)
		{
			_poolsContext.RemoveAll();
		}

		_systemsContext.Run();
		_poolsContext.ToNextFrame();
	}

	public void RequestSwitchSystemsGroup(int? groupId, bool withPoolsRemoveAll)
	{
		_systemsContext.RequestSwitchGroup(groupId);
		_isRequestedSwitchSystemsGroupWithPoolsRemoveAll = withPoolsRemoveAll;
	}

	
	protected abstract Dictionary<int, List<BlahFeatureBase>> FeaturesBySystemsGroups { get; }

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	private BlahInjector BuildInjector()
	{
		var injector = new BlahInjector();
		injector.AddSource(_servicesContext,
		                   typeof(BlahServiceBase),
		                   nameof(BlahServicesContext.Get),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahSignalConsumer<>),
		                   nameof(BlahPoolsContext.GetSignalConsumer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahSignalProducer<>),
		                   nameof(BlahPoolsContext.GetSignalProducer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahDataConsumer<>),
		                   nameof(BlahPoolsContext.GetDataConsumer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahDataProducer<>),
		                   nameof(BlahPoolsContext.GetDataProducer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		return injector;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if UNITY_EDITOR
	public string DebugGetSystemsOrderMsg() => _systemsContext.DebugGetSystemsOrderMsg();
#endif
}
}
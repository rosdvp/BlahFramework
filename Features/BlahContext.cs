using System;
using System.Collections.Generic;
using Blah.Injection;
using Blah.Ordering;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features
{
public abstract class BlahContext
{
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private BlahServicesContext _servicesContext;
	private BlahPoolsContext    _poolsContext;
	private BlahSystemsContext  _systemsContext;

	private bool _isRequestedSwitchSystemsGroupWithPoolsRemoveAll;

	public void Init(IBlahServicesInitData servicesInitData, IBlahSystemsInitData systemsInitData)
	{
		_servicesContext = new BlahServicesContext(servicesInitData);
		_poolsContext    = new BlahPoolsContext();
		_systemsContext  = new BlahSystemsContext(systemsInitData);

		foreach (var feature in Features)
		foreach (var serviceType in feature.Services)
			_servicesContext.TryAdd(serviceType, (BlahServiceBase)Activator.CreateInstance(serviceType));
		_servicesContext.FinalizeInit();


		var groupIdToSystemsTypes = new Dictionary<int, List<Type>>();
		foreach (var feature in Features)
		{
			BlahFeaturesValidator.Validate(feature);

			if (groupIdToSystemsTypes.TryGetValue(feature.SystemsGroupId, out var systems))
				systems.AddRange(feature.Systems);
			else
				groupIdToSystemsTypes[feature.SystemsGroupId] = new List<Type>(feature.Systems);
		}
		
		var injector = new BlahInjector();
		injector.AddSource(_servicesContext, typeof(BlahServiceBase), nameof(BlahServicesContext.Get));
		injector.AddSource(_poolsContext, typeof(IBlahSignalConsumer<>), nameof(BlahPoolsContext.GetSignalConsumer));
		injector.AddSource(_poolsContext, typeof(IBlahSignalProducer<>), nameof(BlahPoolsContext.GetSignalProducer));
		injector.AddSource(_poolsContext, typeof(IBlahDataConsumer<>), nameof(BlahPoolsContext.GetDataConsumer));
		injector.AddSource(_poolsContext, typeof(IBlahDataProducer<>), nameof(BlahPoolsContext.GetDataProducer));
		
		foreach (var pair in groupIdToSystemsTypes)
		{
			int groupId      = pair.Key;
			var systemsTypes = pair.Value;
			BlahOrderer.Order(ref systemsTypes);

			var group = _systemsContext.AddGroup(groupId);
			foreach (var systemType in systemsTypes)
			{
				var system = (IBlahSystem)Activator.CreateInstance(systemType);
				injector.InjectInto(system);
				group.AddSystem(system);
			}
		}
	}
	
	public BlahPoolsContext Pools => _poolsContext;

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


	protected abstract IReadOnlyList<BlahFeatureBase> Features { get; }
}
}
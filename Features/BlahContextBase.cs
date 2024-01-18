using System;
using System.Collections.Generic;
using Blah.Ecs;
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
	private BlahPoolsContext    _poolsContext = new();
	private BlahServicesContext _servicesContext;
	private BlahSystemsContext  _systemsContext;
	private BlahEcs        _ecs = new();

	private bool _isRequestedSwitchWithPoolsClear;

	public void Init(IBlahServicesInitData servicesInitData, IBlahSystemsInitData systemsInitData)
	{
		_servicesContext = new BlahServicesContext(servicesInitData);
		_systemsContext  = new BlahSystemsContext(systemsInitData, OnSwitchGroupBetweenPauseAndResume);

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
		_systemsContext.Run();
		_poolsContext.OnNextFrame();
	}

	
	/// <summary>
	/// Sets the <paramref name="groupId"/> active, and the current one - inactive<br/>
	/// On next Run:
	/// For current group, <see cref="IBlahResumePauseSystem.Pause"/> will be called.<br/>
	/// For new group <see cref="IBlahInitSystem.Init"/> and <see cref="IBlahResumePauseSystem.Resume"/>
	/// will be called.
	/// </summary>
	/// <param name="withPoolsClear">If true, clear all pools after Pause.</param>
	public void RequestSwitchSystemsGroup(int? groupId, bool withPoolsClear)
	{
		_systemsContext.RequestSwitchGroup(groupId);
		_isRequestedSwitchWithPoolsClear = withPoolsClear;
	}

	private void OnSwitchGroupBetweenPauseAndResume()
	{
		if (!_isRequestedSwitchWithPoolsClear)
			return;
		_isRequestedSwitchWithPoolsClear = false;
		
		_poolsContext.Clear();
		_ecs.Clear();
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
		                   typeof(IBlahNfSignalConsumer<>),
		                   nameof(BlahPoolsContext.GetNfSignalConsumer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahNfSignalProducer<>),
		                   nameof(BlahPoolsContext.GetNfSignalProducer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahSoloSignalConsumer<>),
		                   nameof(BlahPoolsContext.GetSoloSignalConsumer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument);
		injector.AddSource(_poolsContext,
		                   typeof(IBlahSoloSignalProducer<>),
		                   nameof(BlahPoolsContext.GetSoloSignalProducer),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument);
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

		var ecsSource = new BlahEcsInjectSource(_ecs);
		injector.AddSource(ecsSource,
		                   typeof(BlahEcs),
		                   nameof(BlahEcsInjectSource.GetEcs),
		                   BlahInjector.EMethodType.Simple
		);
		injector.AddSource(ecsSource,
		                   typeof(IBlahEcsCompWrite<>),
		                   nameof(BlahEcsInjectSource.GetWrite),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(ecsSource,
		                   typeof(IBlahEcsCompRead<>),
		                   nameof(BlahEcsInjectSource.GetRead),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		injector.AddSource(ecsSource,
		                   typeof(BlahEcsFilter),
		                   nameof(BlahEcsInjectSource.GetFilter),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
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
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
	private BlahSystemsContext _systemsContext;

	private bool _isRequestedSwitchWithPoolsClear;


	public BlahPoolsContext    Pools    { get; } = new();
	public BlahEcs             Ecs      { get; } = new();
	public BlahServicesContext Services { get; private set; }
	public BlahInjector        Injector { get; } = new();

	public void Init(IBlahServicesInitData servicesInitData, IBlahSystemsInitData systemsInitData)
	{
		Services        = new BlahServicesContext(servicesInitData);
		_systemsContext = new BlahSystemsContext(systemsInitData, OnSwitchGroupBetweenPauseAndResume);

		var typeToSystem = new Dictionary<Type, IBlahSystem>();
		var allSystems   = new List<IBlahSystem>();

		var bgSystemsTypes = new List<Type>();
		if (BackgroundFeatures != null)
			foreach (var feature in BackgroundFeatures)
			foreach (var system in feature.Systems)
			{
				var type = system.GetType();
				bgSystemsTypes.Add(type);
				typeToSystem[type] = system;
				allSystems.Add(system);
			}

		var groupSystemsTypes = new List<Type>();

		foreach ((int groupId, var features) in FeaturesGroups)
		{
			groupSystemsTypes.Clear();
			groupSystemsTypes.AddRange(bgSystemsTypes);

			foreach (var feature in features)
			foreach (var system in feature.Systems)
			{
				var type = system.GetType();
				groupSystemsTypes.Add(type);
				typeToSystem[type] = system;
				allSystems.Add(system);
			}

			BlahOrderer.Order(ref groupSystemsTypes);

			var group = _systemsContext.AddGroup(groupId);
			foreach (var type in groupSystemsTypes)
				group.AddSystem(typeToSystem[type]);
		}

		AddSourcesToInjector();
		foreach (var system in allSystems)
			Injector.InjectInto(system);
	}

	public void Run()
	{
		_systemsContext.Run();
		Pools.OnNextFrame();
	}


	public int? ActiveFeaturesGroupId => _systemsContext.ActiveGroupId;

	/// <summary>
	/// Sets the <paramref name="groupId"/> active, and the current one - inactive<br/>
	/// On next Run:
	/// For current group, <see cref="IBlahResumePauseSystem.Pause"/> will be called.<br/>
	/// For new group <see cref="IBlahInitSystem.Init"/> and <see cref="IBlahResumePauseSystem.Resume"/>
	/// will be called.
	/// </summary>
	/// <param name="withPoolsClear">If true, clear all pools after Pause.</param>
	public void RequestSwitchFeaturesGroup(int? groupId, bool withPoolsClear)
	{
		_systemsContext.RequestSwitchGroup(groupId);
		_isRequestedSwitchWithPoolsClear = withPoolsClear;
	}

	private void OnSwitchGroupBetweenPauseAndResume()
	{
		if (!_isRequestedSwitchWithPoolsClear)
			return;
		_isRequestedSwitchWithPoolsClear = false;

		Pools.Clear();
		Ecs.Clear();
	}


	public abstract Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; }

	public virtual List<BlahFeatureBase> BackgroundFeatures { get; }

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private void AddSourcesToInjector()
	{
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetSignalRead),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahSignalRead<>)
		);
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetSignalWrite),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahSignalWrite<>)
		);
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetNfSignalRead),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahNfSignalRead<>)
		);
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetNfSignalWrite),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahNfSignalWrite<>)
		);
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetDataGetter),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahDataGet<>)
		);
		Injector.AddSource(Pools,
		                   nameof(BlahPoolsContext.GetDataAdder),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahDataFull<>)
		);
		Injector.AddSource(Services,
		                   nameof(BlahServicesContext.Get),
		                   BlahInjector.EMethodType.TakeGenericReturnSimple,
		                   typeof(BlahServiceBase)
		);
		Injector.AddSimpleInjectable(Ecs);
		Injector.AddSource(Ecs,
		                   nameof(BlahEcs.GetCompGetter),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahEcsGet<>)
		);
		Injector.AddSource(Ecs,
		                   nameof(BlahEcs.GetCompFull),
		                   BlahInjector.EMethodType.TakeGenericReturnGenericInSimple,
		                   typeof(IBlahEcsFull<>)
		);
		Injector.AddSource(Ecs,
		                   nameof(BlahEcs.CreateFilter),
		                   BlahInjector.EMethodType.TakeGenericReturnSimple,
		                   typeof(BlahEcsFilter)
		);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public IReadOnlyList<IBlahSystem> TestsGetAllSystems(int groupId)
	{
		return _systemsContext.GetAllSystems(groupId);
	}

	public void TestsAddSourcesToInjector() => AddSourcesToInjector();
#endif
}
}
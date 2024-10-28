using System;
using System.Collections.Generic;
using Blah.Ecs;
using Blah.Injection;
using Blah.Ordering;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Context
{
public abstract class BlahContextBase
{
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public  BlahInjector        Injector { get; } = new();
	public  BlahServicesContext Services { get; private set; }
	public  BlahPoolsContext    Pools    { get; } = new();
	public  BlahEcs             Ecs      { get; } = new();
	private BlahSystemsContext  Systems  { get; set; }

	public void Init(IBlahServicesInitData servicesInitData, IBlahSystemsInitData systemsInitData)
	{
		Services = new BlahServicesContext(servicesInitData);
		Systems  = new BlahSystemsContext(systemsInitData, OnFeaturesGroupSwitch);

		AddInjectorSources();
		
		var bgSystemsTypes = new List<Type>();
		var systemsTypes   = new List<Type>();
		var typeToSystem   = new Dictionary<Type, IBlahSystem>();
		
		if (BackgroundFeatures != null)
			foreach (var bgFeature in BackgroundFeatures)
			foreach (var bgSystem in bgFeature.Systems)
			{
				typeToSystem.Add(bgSystem.GetType(), bgSystem);
				bgSystemsTypes.Add(bgSystem.GetType());
				
				Injector.InjectInto(bgSystem);
			}
		
		foreach ((int groupId, var features) in FeaturesGroups)
		{
			systemsTypes.Clear();
			systemsTypes.AddRange(bgSystemsTypes);
			foreach (var feature in features)
			foreach (var system in feature.Systems)
			{
				typeToSystem.Add(system.GetType(), system);
				systemsTypes.Add(system.GetType());
				
				Injector.InjectInto(system);
			}

			try
			{
				BlahOrderer.Order(ref systemsTypes);
			}
			catch (BlahOrdererSortingException e)
			{
				throw new Exception(e.GetFullMsg());
			}

			var group = Systems.AddGroup(groupId);
			foreach (var systemType in systemsTypes)
				group.AddSystem(typeToSystem[systemType]);
		}
		
		Services.FinalizeInit();
	}

	
	public void Run()
	{
		Systems.Run();
		Pools.OnNextFrame();
	}


	public int? ActiveFeaturesGroupId => Systems.ActiveGroupId;

	/// <summary>
	/// On next Run:
	/// For current group, <see cref="IBlahResumePauseSystem.Pause"/> will be called.<br/>
	/// For new group <see cref="IBlahInitSystem.Init"/> and <see cref="IBlahResumePauseSystem.Resume"/>
	/// will be called.
	/// </summary>
	/// <remarks>Pools and Ecs will be cleared.</remarks>
	public void RequestFeaturesGroupSwitchOnNextRun(int? groupId)
		=> Systems.RequestSwitchOnNextRun(groupId);

	private void OnFeaturesGroupSwitch()
	{
		Pools.Clear();
		Ecs.Clear();
	}


	public abstract Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; }

	public virtual List<BlahFeatureBase> BackgroundFeatures { get; }

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	/// <summary>
	/// Is a separate method for injection purpose.
	/// </summary>
	public BlahEcs GetEcs() => Ecs;
	
	private void AddInjectorSources()
	{
		Injector.AddSource(Services,
		                   typeof(BlahServiceBase),
		                   nameof(BlahServicesContext.Get),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahSignalRead<>),
		                   nameof(BlahPoolsContext.GetSignalRead),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahSignalWrite<>),
		                   nameof(BlahPoolsContext.GetSignalWrite),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahNfSignalRead<>),
		                   nameof(BlahPoolsContext.GetNfSignalRead),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahNfSignalWrite<>),
		                   nameof(BlahPoolsContext.GetNfSignalWrite),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahDataGet<>),
		                   nameof(BlahPoolsContext.GetDataGetter),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Pools,
		                   typeof(IBlahDataFull<>),
		                   nameof(BlahPoolsContext.GetDataFull),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(this,
		                   typeof(BlahEcs),
		                   nameof(GetEcs),
		                   BlahInjector.EMethodType.Simple
		);
		Injector.AddSource(Ecs,
		                   typeof(IBlahCompGet<>),
		                   nameof(BlahEcs.GetCompGetter),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Ecs,
		                   typeof(IBlahCompFull<>),
		                   nameof(BlahEcs.GetCompFull),
		                   BlahInjector.EMethodType.GenericAcceptGenericArgument
		);
		Injector.AddSource(Ecs,
		                   typeof(BlahFilter),
		                   nameof(BlahEcs.GetFilter),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public IReadOnlyList<IBlahSystem> GetAllSystems(int groupId)
	{
		return Systems.GetAllSystems(groupId);
	}
#endif

#if UNITY_EDITOR
	public string DebugGetSystemsOrderMsg() => Systems.DebugGetSystemsOrderMsg();
#endif
}
}
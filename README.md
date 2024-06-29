# About
_TODO: list key features._

# Quick Start
1) Install package via Unity Package Manager.
2) _TODO: provide demo files that can be copied into empty project._

# Usage
## Architecture
### Systems
A System is a C# pure class that implements one or more interfaces:
```c#
public class TestSystem : 
    IBlahInitSystem, 
    IBlahResumeSystem, IBlahPauseSystem, 
    IBlahRunSystem
{
    // Invocation order inside one frame:
    // 1) Init
    // 2) Resume
    // 3) Run
    // 4) Pause

    // Invoked when System becomes active first time.
    public void Init(IBlahSystemsInitData initData)
    {
        var myContext = (MyContext)initData;
        myContext.UI.DoSomething();
    }

    // Invoked each time System becomes active.
    public void Resume(IBlahSystemsInitData initData)
    {
        var myContext = (MyContext)initData;
    }

    // Invoked each time System becomes inactive.
    public void Pause() {}

    // Invoked each frame while System is active.
    public void Run() {}
}
```
### Features
Systems belong to Features. A System becomes active/inactive when a Feature becomes active/inactive.

> [!NOTE]
> Each Feature contains three code-gen properties that theoretically should make it easier for you to understand how each Feature interacts with others.

```c#
public class FeatureUnits : BlahFeatureBase
{
    // All Signals the Feature consumes but does not produce.
    // Must NOT be filled manually (code-gen is used).
    public override HashSet<Type> ConsumingFromOutside{ get; } = new()
    {
        typeof(UnitSpawnCmd),
    };
    // All Signals the Feature produces.
    // Must NOT be filled manually (code-gen is used).
    public override HashSet<Type> Producing { get; } = new()
    {
        typeof(UnitSpawnedEv),
    };
    // All Services the Feature uses.
    // Must NOT be filled manually (code-gen is used).
    public override HashSet<Type> Services { get; } = new()
    {
        typeof(UnitsService)
    };
    // All Systems in Feature.
    // MUST be filled manually.
    public override IReadOnlyList<Type> Systems { get; } = new[]
    {
        typeof(UnitSpawnSystem),
    };
}
```
### Context and Groups
Features belong to Groups. You cannot activate/deactivate a specific Feature, however you can activate/deactivate the Group that contains the Feature. At one time only one Group can be active, however Background Features are always active and can be used for peripherals.

```c#
public class BlahContext : BlahContextBase
{
    protected override Dictionary<int, List<BlahFeatureBase>> FeaturesBySystemsGroups { get; } = new()
    {
        {
            // The Systems of these Features run only if the current Group is "Menu".
            (int)EFeaturesGroup.Menu, new List<BlahFeatureBase>
            {
                new FeatureMenuBotPanel(),
                new FeatureMenuTopPanel(),
            }
        },
        {
            // The Systems of these Features run only if the current Group is "Battle".
            (int)EFeaturesGroup.Battle, new List<BlahFeatureBase>
            {
                new FeatureUnits(),
                new FeatureMoving(),
                new FeatureAttacking(),
            }
        },
    };
    
    // Background Features always run.
	protected override List<BlahFeatureBase> BackgroundFeatures { get; } = new()
    {
        new FeautresAds(),
    }
}
public enum EFeaturesGroup
{
    Menu,
    Battle,
}
```
### Startup Example
```c#
[Serializable]
public class MyContext : IBlahSystemsInitData, IBlahServicesInitData
{
    public GameObject UI;
}

public class GameStartup : MonoBehaviour
{
    [SerializedField]
    private MyContext _myContext;

    private BlahContext _blahContext;

    private void Start()
    {
        _blahContext = new BlahContext();

        // Instantiates Features, Systems, Services, and performs ordering & injection.
        // Services' Init methods are invoked here.
        // No Systems' methods are invoked here.
        _blahContext.Init(
            _myContext, // Will be passed to Systems' Init methods.
            _myContext  // Will be passed to Services' Init methods.
        );

        // Requests "Menu" Group to become active.
        // Switching of active Group always happens during next Run call even if it is requested during current Run call (i.e. inside a System).
        _blahContext.RequestSwitchSystemsGroup(
            (int)EFeaturesGroup.Menu, 
            true // Will automatically clear Signals/Datas/ECS when switch is performed.
        );
    }

    private void Update()
    {
        // During one invocation the following happens in order:
        // 1) If Group switch is requested and another Group is active, invoke Pause.
        // 2) If Group switch is requested and it is a first time a requested Group runs, invoke Init.
        // 3) If Group switch is requested, invoke Resume.
        // 4) Invoke Run.
        _blahContext.Run();
    }
}
```

## Pools
### Signals
Signals are pooled data-structures which life time is one Run. Usually, Signals should be used to communicate from one System (or MonoBehaviour) to another System(s) that something must be done (command) or something happened (event) within one Run.

> [!IMPORTANT]
> Signals are destroyed when all Systems completed Run.

> [!WARNING]
> Signals define [Systems Execution Order](#systems-execution-order), so any System producing Signal always Runs before any System consuming Signal.

```c#
public struct PurchaseCmd : IBlahEntrySignal 
{
    // You can use pure fields or properties + methods to pass the data.
    public string Id { get; private set; }

    public void Set(string id)
    {
        Id = id;
    }
}
public struct PurchasedEv : IBlahEntrySignal 
{
    public string Id { get; private set; }

    public void Set(string id)
    {
        Id = id;
    }
}

public class PurchaseSystem : IBlahRunSystem
{
    // Signals Consumers/Producers are injected into Systems fields.
    private IBlahSignalConsumer<PurchaseCmd> _purchaseCmd;

    private IBlahSignalProducer<PurchasedEv> _purchasedEv;

    public void Run()
    {
        // Iterate all Signals of type at this Run.
        foreach (ref var cmd in _purchaseCmd)
        {
            // Some logic...

            // Adds a new event.
            _purchasedEv.Add().Set(cmd.Id);

            // Another option:
            // ref var ev = ref _purchasedEv.Add();
            // ev.Id = cmd.Id;
        }
    }
}

// UiSystem always Run after PurchaseSystem 
// since UiSystem consumes PurchasedEv and PurchaseSystem produces it.
public class UiSystem : IBlahRunSystem
{
    private IBlahSignalConsumer<PurchasedEv> _purchasedEv;

    public void Run()
    {
        // If you do not need all Signals of type at this Run, just check if there is at least one..
        if (!_purchasedEv.IsEmpty)
        {
            // ..and take any of them.
            ref var ev = ref _purchasedEv.GetAny();
            // Some logic..
        }
    }
}
```

### Next Frame Signals
Since Signals live during one Run, the cyclic dependency might take place: SystemA tells something to SystemB and SystemB should tell something back to SystemA, but SystemA has already done its Run.

The one walkaround might be sending Signals for next Run.

> [!IMPORTANT]
> NF Signals become consumable only at the beginning of the next Run and are destroyed once all Systems completed the Run.

> [!WARNING]
> NF Signals do **NOT** define [Systems Execution Order](#systems-execution-order).

> [!CAUTION]
> In general, NF Signals make code harder to maintain, so you should use them as a last resort.

```c#
public struct ScreenShowCmd : IBlahEntryNextFrameSignal {}
public struct ScreenShownEv : IBlahEntrySignal {}

// ScreensSystem will go before TriggerSystem, since TriggerSystem consumes ScreenShownEv and NF Signals do not define Order.
public class TriggerSystem : IBlahRunSystem
{
    private IBlahSignalConsumer<ScreenShownEv> _screenShownEv;

    // NF Signals Consumers/Producers are injected into Systems fields.
    private IBlahNfSignalProducer<ScreenShowCmd> _screenShowCmd;

    public void Run()
    {
        if (/* screen should be shown */)
        {
            // Send NF Signal for next Run.
            _screenShowCmd.AddNf();
        }

        // Check if screen has been shown at this Run.
        if (!_screenShowEv.IsEmpty)
        {
            // Some logic..
        }    
    }
}

// ScreensSystem always Run before TriggerSystem since
// ScreensSystem produces ScreenShownEv and TriggerSystem consumes it.
// ScreenShownCmd does not affect the order because it is a Next Frame Signal.
public class ScreensSystem : IBlahRunSystem
{
    private IBlahNfSignalConsumer<ScreenShowCmd> _screenShowCmd;

    private IBlahSignalProducer<ScreenShownEv> _screenShownEv;

    public void Run()
    {
        // Check if there is any NF Signal from previous Run.
        if (!_screenShowCmd.IsEmpty)
        {
            // Some logic..

            // Send Signal for this Run.
            _screenShowEv.Add();
        }
    }
}

```

### Datas
On the contrary to Signals, Datas live until being explicitly destroyed. Usually, Datas should be used for queries/singletons/animations.

> [!WARNING]
> Datas do **NOT** define [Systems Execution Order](#systems-execution-order).

```c#
public struct MoneyAnimData : IBlahEntryData
{
    public Vector2 StartPos;
    public Vector2 TargetPos;
    public float StartTime;
    public float Duration;

    public Transform Tf;
}

public class MoneyAnimSystem : IBlahRunSystem
{
    // Datas Consumers/Producers are injected into Systems fields.
    public IBlahDataConsumer<MoneyAnimData> _moneyAnimData;

    public void Run()
    {
        foreach (ref var anim in _moneyAnimData)
        {
            float progress = (Time.time - anim.StartTime) / anim.Duration;
            if (progress < 1.0f)
            {
                var step = (anim.TargetPos - anim.StartPos) / anim.Duration; 
                anim.Tf.position = anim.StartPos + step * progress;
            }
            else
            {
                anim.Tf.position = anim.TargetPos;

                // Destroyes only Data which is current in foreach iteration.
                _moneyAnimData.Remove();

                // If you perform nested foreach, the Data of inner foreach will be destroyed. Also, you can specify from each foreach iteration you want to destroy the Data.
                // foreach (ref var a in _data)
                // foreach (ref var b in _data)
                // {
                //      _data.Remove();      'b' will be destroyed
                //      _data.Remove(0);     'a' will be destroyed
                //      _data.Remove(1);     'b' will be destroyed
                // }
            }
        }
    }
}
```

### Data Pointers
The purpose of Data Pointer is similar to purpose of Entity in ECS: fast access to specific Data in pool. However, it is still not an Entity since Data Pointer is valid only in scope of one Data type.
Usually, Data Pointers are used to avoid extra Ids and extra foreach iteration to find a specific Data.

```c#
public struct PlayerData : IBlahEntryData {}

public struct MoneyChangedEv : IBlahEntrySignal
{
    public BlahDataPtr PlayerPtr;
}

public class PlayerSystem : IBlahInitSystem, IBlahRunSystem
{
    private IBlahSignalConsumer<MoneyChangedEv> _moneyChangedEv;

    private IBlahDataConsumer<PlayerData> _playerDataCons;

    private IBlahDataProducer<PlayerData> _playerDataProd;

    public void Init(IBlahInitData initData)
    {
        // Creates a new Data and gets Pointer to it,
        // so you can pass Pointer whenever you want 
        // to access exactly this Data instance later.
        _playerDataProd.Add(out var ptr);

        foreach (ref var player in _playerDataCons)
        {
            // Gets Pointer to current Data in foreach iteration.
            var ptr = _playerDataCons.GetPtr();
        }
    }

    public void Run()
    {
        foreach (ref var ev in _moneyChangedEv)
        {
            // Checks if there is a Data with the Pointer.
            if (_playerDataCons.IsPtrValid(ev.PlayerPtr))
            {
                // Gets the specific Data the Pointer is assigned to.
                ref var player = ref _playerDataCons.Get(ev.PlayerPtr);
            }
        }

        // Without Pointers the code would be:
        // foreach (ref var ev in _moneyChangedEv)
        // {
        //     foreach (ref var player in _playerDataCons)
        //     {
        //         if (player.Id == ev.PlayerId)
        //         {
        //             ...
        //         }
        //     }
        // }   
    }
}
```

### ECS
The ECS API is a mix of LeoEcs & LeoEcsLite.

> [!WARNING]
> ECS does **NOT** define [Systems Execution Order](#systems-execution-order).

```c#
public struct UnitComp : IBlahEntryEcs {}

public class UnitSystem : IBlahInitSystem, IBlahRunSystem
{
    // Components Read/Write handlers are injected into System's fields.
    private IBlahEcsRead<UnitComp> _units;

    private IBlahEcsWrite<UnitComp> _unitsWriter;

    // Filters are injected into System's fields.
    private BlahEcsFilter<UnitComp> _unitsFilter;

    // BlahEcs is injected into System's field.
    private BlahEcs _ecs;

    public void Init(IBlahSystemsInitData initData)
    {
        // Creates a new Entity.
        var ent = _ecs.CreateEntity();

        // Adds a Component to Entity.
        ref var unit = ref _unitsWriter.Add(ent);
    }

    public void Run()
    {
        // Enumerates all Entities which meet Filter's mask.
        foreach (var ent in _unitsFilter)
        {
            // Gets Component of Entity.
            ref var unit = ref _units.Get(ent);

            // Removes Component from Entity.
            _units.Remove(ent);

            // Destroyes Entity.
            _ecs.DestroyEntity(ent);
        }
    }
}

```

## Systems Execution Order
The order in wich Systems' methods are invoked is maintained by the framework. When BlahContext.Init is invoked, the framework uses topological sorting algorithm that takes into account:
1. System's Signals Consumers/Producers.
2. BlahAfter/BlahAfterAll/BlahBefore/BlahBeforeAll attributes.

The Features does not define the order of Systems, so it is not guaranteed that Systems from the same Feature comes together or in order they are listed in the Feature.

> [!TIP]
> Most time you should not worry about Systems order. 
Just keep in mind that the framework guarantees that all producers are invoked before consumers.

> [!CAUTION]
> If there is a cyclic dependency, the framework will throw an exception with detailed info about wrong dependencies.

```c#
// In the following example the Systems order is:
// 1. SystemA: it produces Ev1 which is consumed by SystemB.
// 2. SystemB: it has BlahBeforeAll attribute with highest priority among others.
// 3. SystemC: it has BlahBeforeAll attribute.
// 4. _SystemX might be there since producers of Ev1 and Ev2 already past._
// 5. SystemD: it consumes Ev2 produced by SystemB.
// 4. _SystemX might be there since producers of Ev1 and Ev2 already past._
// 7. SystemE: it has BlahAfterAll attribute.
// 8. SystemF: it has BlahAfter attribute with parameter pointing to SystemE.

public class SystemA 
{
    private IBlahSignalProducer<Ev1> _ev1;
}

[BlahBeforeAll(99)]
public class SystemB 
{
    private IBlahSignalConsumer<Ev1> _ev1;

    private IBlahSignalProducer<Ev2> _ev2;
}

[BlahBeforeAll]
public class SystemC {}

public class SystemD 
{
    private IBlahSignalConsumer<Ev2> _ev2;
}

[BlahAfterAll]
public class SystemE {}

[BlahAfter(typeof(SystemD))]
public class SystemF {}

public class SystemX 
{
    private IBlahSignalConsumer<Ev1> _ev1;
    private IBlahSignalConsumer<Ev2> _ev2;
}
```

## Services
Services are pure C# classes-singletons that live during application lifetime.
On the contrary to Datas or Components, the Services are not the part of ECS or pools, so they stay alive on Features group switch.

The common usages of Services are OOP parts of the code (Path Finding, Hierarchies..), peripheral functions (Ads, IAP..), or interaction with Model.

The communication between Services is performed via Lazy references. However, it is guaranteed that InitImpl is invoked for all Services during BlahContext.Init call.

```c#
public class MoneyService : BlahServiceBase
{
    private Model _model;

    protected override void InitImpl(IBlahServiceInitData initData, IBlahServicesContainerLazy services)
    {
        // Cast to your class.
        var myContext = (MyContext)initData;

        _model = myContext.Model;
    }

    public int Money => _model.Money;

    // Tip: place Signal Producer in public API parameter, 
    // so any System that wants to change money balance will be obligated to have Producer field.
    public bool TryChange(int delta, IBlahSignalProducer<MoneyChangedEv> ev)
    {
        if (_model.Money + delta <> 0)
            return false;
        _model.Money += delta;
        ev.Add();
        return true;
    }
}

public class StuffsService : BlahServiceBase
{
    private BlahServiceLazy<MoneyService> _moneyService;

    protected override void InitImpl(IBlahServiceInitData initData, IBlahServicesContainerLazy services)
    {
        // Get Lazy reference to another service to avoid cyclic dependency.
        _moneyService = services.GetLazy<MoneyService>();
    }

    public bool TryPurchase(EStuffId id, int price, IBlahSignalProducer<MoneyChangedEv> ev)
    {
        if (_moneyService.Get.TryChange(price, ev))
            ...
    }
}

public class StuffsSystem : IBlahRunSystem
{
    private IBlahSignalProducer<MoneyChangedEv> _moneyChangedEv;

    // Services are injected into System's fields.
    private StuffsService _stuffsService;

    public void Run()
    {
        _stuffsService.TryPurchase(EStuffId.House, 10, _moneyChangedEv);
    }
}
```

## Injection

_TODO: describe custom injection._

## Unity Interoption

_TODO: describe top menu "Blah" in Unity._

_TODO: describe AOT._

## Best Practices

_TODO_

# Other Packages
[BlahEditor](https://github.com/rosdvp/BlahEditor) - a set of attributes/drawers/utilities to prettify Unity Editor + SubAssets mechanism for game configs. 

[BlahSaves](https://github.com/rosdvp/BlahSaves) - a tool to simplify saving/loading game progress.

[BlahDebugConsole](https://github.com/rosdvp/BlahDebugConsole) - a logger + cheats console for game QA.
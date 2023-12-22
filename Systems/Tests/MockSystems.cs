using System;

namespace Blah.Systems.Tests
{
internal class MockSystemsInitData : IBlahSystemsInitData
{
	public BlahSystemsContext Context;
}

internal enum EMockGroupId
{
	GroupA,
	GroupB,
	GroupC
}

internal abstract class MockBaseSystem : IBlahSystem
{
	protected static int NextInitOrder;
	protected static int NextRunOrder;
	protected static int NextResumeOrder;
	protected static int NextPauseOrder;


	public int InitOrder   { get; protected set; } = -1;
	public int InitCount   { get; protected set; }
	public int RunOrder    { get; protected set; } = -1;
	public int RunCount    { get; protected set; }
	public int ResumeOrder { get; protected set; } = -1;
	public int ResumeCount { get; protected set; }
	public int PauseOrder  { get; protected set; } = -1;
	public int PauseCount  { get; protected set; }

	protected MockSystemsInitData InitData;

	public void Init(IBlahSystemsInitData initData)
	{
		InitData = (MockSystemsInitData)initData;
		
		if (InitOrder == -1)
			InitOrder = NextInitOrder++;
		InitCount += 1;
	}

	public virtual void Run()
	{
		if (RunOrder == -1)
			RunOrder = NextRunOrder++;
		RunCount += 1;
	}

	public void Resume(IBlahSystemsInitData initData)
	{
		if (ResumeOrder == -1)
			ResumeOrder = NextResumeOrder++;
		ResumeCount += 1;
	}

	public void Pause()
	{
		if (PauseOrder == -1)
			PauseOrder = NextPauseOrder++;
		PauseCount += 1;
	}
	
	
	public void ResetOrders()
	{
		NextInitOrder   = 0;
		NextRunOrder    = 0;
		NextResumeOrder = 0;
		NextPauseOrder  = 0;
		
		InitOrder   = -1;
		RunOrder    = -1;
		ResumeOrder = -1;
		PauseOrder  = -1;
	}
}

internal class MockInitSystem : MockBaseSystem, IBlahInitSystem { }

internal class MockFullSystem : 
	MockBaseSystem, IBlahInitSystem, IBlahRunSystem, IBlahResumeSystem, IBlahPauseSystem { }


internal class MockFullWithSwitchToBSystem : 
	MockBaseSystem, IBlahInitSystem, IBlahRunSystem, IBlahResumeSystem, IBlahPauseSystem
{
	public override void Run()
	{
		base.Run();
		InitData.Context.RequestSwitchGroup((int)EMockGroupId.GroupB);
	}
}

internal class MockFullWithSwitchToCSystem 
	: MockBaseSystem, IBlahInitSystem, IBlahRunSystem, IBlahResumeSystem, IBlahPauseSystem
{
	public override void Run()
	{
		base.Run();
		InitData.Context.RequestSwitchGroup((int)EMockGroupId.GroupC);
	}
}
}
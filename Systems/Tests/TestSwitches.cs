using NUnit.Framework;

namespace Blah.Systems.Tests
{
internal class TestSwitches
{
	[Test]
	public void Test_Create_NoMethodsCalled()
	{
		var (context, systems) = CreateFullSystems();

		AssertHelper.AssertSystems(
			systems,
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 }
		);
	}

	[Test]
	public void Test_SwitchToAAndRun_AInitAndRun()
	{
		var (context, systems) = CreateFullSystems();

		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupA);
		context.Run();
		
		AssertHelper.AssertSystems(
			systems,
			new[] { 0, -1, -1 },
			new[] { 1, 0, 0 },
			new[] { 0, -1, -1 },
			new[] { 1, 0, 0 },
			new[] { 0, -1, -1 },
			new[] { 1, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 }
		);
		
		context.Run();
		
		AssertHelper.AssertSystems(
			systems,
			new[] { -1, -1, -1 },
			new[] { 1, 0, 0 },
			new[] { 0, -1, -1 },
			new[] { 2, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 1, 0, 0 },
			new[] { -1, -1, -1 },
			new[] { 0, 0, 0 }
		);
	}

	[Test]
	public void Test_SwitchToBThenA_BAInitAndRun()
	{
		var (context, systems) = CreateFullSystems();

		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupB);
		context.Run();
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupA);
		context.Run();
		
		AssertHelper.AssertSystems(
			systems,
			new[] { 1, 0, -1 },
			new[] { 1, 1, 0 },
			new[] { 1, 0, -1 },
			new[] { 1, 1, 0 },
			new[] { 1, 0, -1 },
			new[] { 1, 1, 0 },
			new[] { -1, 0, -1 },
			new[] { 0, 1, 0 }
		);
	}

	[Test]
	public void Test_RunCRunB2RunA3_MethodsCalled()
	{
		var (context, systems) = CreateFullSystems();

		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupC);
		context.Run();
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupB);
		context.Run();
		context.Run();
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupA);
		context.Run();
		context.Run();
		context.Run();
		
		AssertHelper.AssertSystems(
			systems,
			new[] { 2, 1, 0 },
			new[] { 1, 1, 1 },
			new[] { 2, 1, 0 },
			new[] { 3, 2, 1 },
			new[] { 2, 1, 0 },
			new[] { 1, 1, 1 },
			new[] { -1, 1, 0 },
			new[] { 0, 1, 1 }
		);
	}

	[Test]
	public void Test_SwitchBRunSwitchARunSwitchBRun_BOnceInit()
	{
		var (context, systems) = CreateFullSystems();
		
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupB);
		context.Run();
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupA);
		context.Run();
		context.RequestSwitchOnNextRun((int)EMockGroupId.GroupB);
		context.Run();
		
		AssertHelper.AssertSystems(
			systems,
			new[] { 1, 0, -1 },
			new[] { 1, 1, 0 },
			new[] { 1, 0, -1 },
			new[] { 1, 2, 0 },
			new[] { 1, 0, -1 },
			new[] { 1, 2, 0 },
			new[] { 1, 0, -1 },
			new[] { 1, 1, 0 }
		);
	}


	private (BlahSystemsContext context, MockBaseSystem[] systems)
		CreateFullSystems()
	{
		var systems = new MockBaseSystem[]
		{
			new MockFullSystem(),
			new MockFullSystem(),
			new MockFullSystem(),
		};
		var initData = new MockSystemsInitData();
		var context  = new BlahSystemsContext(initData, null);
		initData.Context = context;
		context.AddGroup((int)EMockGroupId.GroupA).AddSystem(systems[0]);
		context.AddGroup((int)EMockGroupId.GroupB).AddSystem(systems[1]);
		context.AddGroup((int)EMockGroupId.GroupC).AddSystem(systems[2]);

		return (context, systems);
	}
}
}
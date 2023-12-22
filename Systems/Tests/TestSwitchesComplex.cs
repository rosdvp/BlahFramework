using NUnit.Framework;

namespace Blah.Systems.Tests
{
internal class TestSwitchesComplex
{
	[Test]
	public void Test_DuringRunBSwitchToC_BRunsTillEnd()
	{
		var systems = new MockBaseSystem[]
		{
			//A
			new MockFullSystem(),
			//B
			new MockFullWithSwitchToCSystem(),
			new MockFullSystem(),
			//C
			new MockFullSystem(),
		};
		var initData = new MockSystemsInitData();
		var context  = new BlahSystemsContext(initData, null);
		initData.Context = context;
		
		context.AddGroup((int)EMockGroupId.GroupA).AddSystem(systems[0]);
		context.AddGroup((int)EMockGroupId.GroupB)
		       .AddSystem(systems[1])
		       .AddSystem(systems[2]);
		context.AddGroup((int)EMockGroupId.GroupC).AddSystem(systems[3]);

		context.RequestSwitchGroup((int)EMockGroupId.GroupB);
		context.Run();
		context.Run();

		AssertHelper.AssertSystems(
			systems,
			new[] { -1, 0, 1, 2 },
			new[] { 0, 1, 1, 1, },
			new[] { -1, 0, 1, 2 },
			new[] { 0, 1, 1, 1, },
			new[] { -1, 0, 1, 2 },
			new[] { 0, 1, 1, 1, },
			new[] { -1, 0, 1, -1 },
			new[] { 0, 1, 1, 0, }
		);
	}
	
	[Test]
	public void Test_DuringRunASwitchToBAndToC_CIsActive()
	{
		var systems = new MockBaseSystem[]
		{
			//A
			new MockFullWithSwitchToBSystem(),
			new MockFullWithSwitchToCSystem(),
			//B
			new MockFullSystem(),
			//C
			new MockFullSystem(),
		};
		var initData = new MockSystemsInitData();
		var context  = new BlahSystemsContext(initData, null);
		initData.Context = context;

		context.AddGroup((int)EMockGroupId.GroupA)
		       .AddSystem(systems[0])
		       .AddSystem(systems[1]);
		context.AddGroup((int)EMockGroupId.GroupB).AddSystem(systems[2]);
		context.AddGroup((int)EMockGroupId.GroupC).AddSystem(systems[3]);

		context.RequestSwitchGroup((int)EMockGroupId.GroupA);
		context.Run();
		context.Run();

		AssertHelper.AssertSystems(
			systems,
			new[] { 0, 1, -1, 2 },
			new[] { 1, 1, 0, 1, },
			new[] { 0, 1, -1, 2 },
			new[] { 1, 1, 0, 1, },
			new[] { 0, 1, -1, 2 },
			new[] { 1, 1, 0, 1, },
			new[] { 0, 1, -1, -1 },
			new[] { 1, 1, 0, 0, }
		);
	}
}
}
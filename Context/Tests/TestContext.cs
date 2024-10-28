using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;
using NUnit.Framework;

namespace Blah.Context.Tests
{
internal class TestContext
{
	[Test]
	public void Test_Context()
	{
		var context = new MockContext();
		context.Init(null, null);

		context.RequestFeaturesGroupSwitchOnNextRun(0);
		context.Run();

		var mockCmdA = context.Pools.GetSignalWrite<MockCmdA>();
		mockCmdA.Add().Val = 25;

		context.Run();

		var dataB = context.Pools.GetDataGetter<MockDataB>();
		Assert.AreEqual(1, dataB.Count);
		foreach (ref var data in dataB)
			Assert.AreEqual(25, data.Val);
	}
	
	private class MockContext : BlahContextBase
	{
		public override Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; } = new()
		{
			{
				0, new List<BlahFeatureBase>
				{
					new MockFeatureA(),
					new MockFeatureB()
				}
			}
		};
	}
	
	
	private class MockFeatureA : BlahFeatureBase
	{
		public override IBlahSystem[] Systems { get; } =
		{
			new MockSystemA()
		};

		private class MockSystemA : IBlahRunSystem
		{
			private IBlahSignalRead<MockCmdA> _cmd;
	
			private IBlahSignalWrite<MockEventA> _event;

			private MockServiceA _serviceA;

			public void Run()
			{
				foreach (ref var cmd in _cmd)
					_event.Add().Val = cmd.Val;
			}
		}
	}
	
	private class MockFeatureB : BlahFeatureBase
	{
		public override IBlahSystem[] Systems { get; } =
		{
			new MockSystemB()
		};

		private class MockSystemB : IBlahRunSystem
		{
			private IBlahSignalRead<MockEventA> _event;

			private IBlahDataFull<MockDataB> _data;

			private MockServiceA _serviceA;
			private MockServiceB _serviceB;

			public void Run()
			{
				foreach (ref var ev in _event)
					_data.Add().Val = ev.Val;
			}
		}
	}
	
	
	private struct MockCmdA : IBlahEntrySignal
	{
		public int Val;
	}

	private struct MockEventA : IBlahEntrySignal
	{
		public int Val;
	}

	private struct MockDataB : IBlahEntryData
	{
		public int Val;
	}


	private class MockServiceA : BlahServiceBase
	{
		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services) { }
	}

	private class MockServiceB : BlahServiceBase
	{
		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services) { }
	}
}
}
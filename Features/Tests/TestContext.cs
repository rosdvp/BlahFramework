using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Systems;
using NUnit.Framework;

namespace Blah.Features.Tests
{
internal class TestContext
{
	[Test]
	public void Test_Context()
	{
		var context = new MockContext();
		context.Init(null, null);

		context.RequestSwitchFeaturesGroup(0, false);

		var producer = context.Pools.GetSignalWrite<MockCmdA>();
		producer.Add().Val = 25;

		context.Run();

		var consumer = context.Pools.GetDataGetter<MockDataB>();
		Assert.AreEqual(1, consumer.Count);
		foreach (var data in consumer)
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
		public override IReadOnlyList<IBlahSystem> Systems { get; } = new[]
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
		public override IReadOnlyList<IBlahSystem> Systems => new[]
		{
			new MockSystemB()
		};

		private class MockSystemB : IBlahRunSystem
		{
			private IBlahSignalRead<MockEventA> _event;

			private IBlahDataAdd<MockDataB> _data;

			private MockServiceA _serviceA;
			private MockServiceB _serviceB;

			public void Run()
			{
				foreach (ref var ev in _event)
					_data.Add().Val = ev.Val;
			}
		}
	}
}
}
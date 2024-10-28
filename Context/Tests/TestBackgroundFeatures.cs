using System.Collections.Generic;
using Blah.Context;
using Blah.Pools;
using Blah.Systems;
using NUnit.Framework;

// use custom namespace to simulate game types (since Blah types are ignored)
// ReSharper disable once CheckNamespace
namespace TestsBlah.Context.Tests
{
internal class TestBackgroundFeatures
{
	[Test]
	public void Test()
	{
		var context = new MockContext();
		context.Init(null, null);


		var systemsA = context.GetAllSystems(0);
		var systemsB = context.GetAllSystems(1);
		
		Assert.AreEqual(typeof(MockSystemA1), systemsA[0].GetType());
		Assert.AreEqual(typeof(MockSystemBg), systemsA[1].GetType());
		Assert.AreEqual(typeof(MockSystemA2), systemsA[2].GetType());
		
		Assert.AreEqual(typeof(MockSystemBg), systemsB[0].GetType());
		Assert.AreEqual(typeof(MockSystemB1), systemsB[1].GetType());
		Assert.AreEqual(typeof(MockSystemB2), systemsB[2].GetType());
		
		Assert.AreSame(systemsA[1], systemsB[0]);
	}
	
	
	
	private class MockContext : BlahContextBase
	{
		public override Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; } = new()
		{
			{ 0, new List<BlahFeatureBase> { new MockFeatureA() } },
			{ 1, new List<BlahFeatureBase> { new MockFeatureB() } }
		};

		public override List<BlahFeatureBase> BackgroundFeatures { get; } = new()
		{
			new MockFeatureBg()
		};
	}

	private class MockFeatureA : BlahFeatureBase
	{
		public override IBlahSystem[] Systems { get; } =
		{
			new MockSystemA1(),
			new MockSystemA2()
		};
	}

	private class MockFeatureB : BlahFeatureBase
	{
		public override IBlahSystem[] Systems { get; } =
		{
			new MockSystemB1(),
			new MockSystemB2()
		};
	}

	private class MockFeatureBg : BlahFeatureBase
	{
		public override IBlahSystem[] Systems { get; } = 
		{
			new MockSystemBg()
		};
	}

	private class MockSystemA1 : MockSystemBase
	{
		private IBlahSignalWrite<MockEv1> _ev1;
	}

	private class MockSystemA2 : MockSystemBase
	{
		private IBlahSignalRead<MockEv1> _ev1;
		private IBlahSignalRead<MockEv3> _ev3;
	}

	private class MockSystemB1 : MockSystemBase
	{
		private IBlahSignalRead<MockEv3> _ev3;
		
		private IBlahSignalWrite<MockEv2> _ev2;
	}

	private class MockSystemB2 : MockSystemBase
	{
		private IBlahSignalRead<MockEv2> _ev2;
	}

	private class MockSystemBg : MockSystemBase
	{
		private IBlahSignalRead<MockEv1> _ev1;
		
		private IBlahSignalWrite<MockEv3> _ev3;
	}

	private class MockSystemBase : IBlahInitSystem, IBlahResumeSystem, IBlahPauseSystem, IBlahRunSystem
	{
		public void Init(IBlahSystemsInitData initData) { }
		public void Resume(IBlahSystemsInitData initData) { }
		public void Pause() { }
		public void Run() { }
	}

	private struct MockEv1 : IBlahEntrySignal { }
	private struct MockEv2 : IBlahEntrySignal { }
	private struct MockEv3 : IBlahEntrySignal { }
}
}
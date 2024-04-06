using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Systems;
using NUnit.Framework;

namespace Blah.Features.Tests
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
		protected override Dictionary<int, List<BlahFeatureBase>> FeaturesBySystemsGroups { get; } = new()
		{
			{ 0, new List<BlahFeatureBase> { new MockFeatureA() } },
			{ 1, new List<BlahFeatureBase> { new MockFeatureB() } }
		};

		protected override List<BlahFeatureBase> BackgroundFeatures { get; } = new()
		{
			new MockFeatureBg()
		};
	}

	private class MockFeatureA : BlahFeatureBase
	{
		public override HashSet<Type> ConsumingFromOutside { get; } = new()
		{
			typeof(MockEv3)
		};
		public override HashSet<Type> Producing { get; } = new()
		{
			typeof(MockEv1)
		};
		public override HashSet<Type> Services             { get; }
		public override IReadOnlyList<Type> Systems { get; } = new[]
		{
			typeof(MockSystemA1),
			typeof(MockSystemA2)
		};
	}

	private class MockFeatureB : BlahFeatureBase
	{
		public override HashSet<Type> ConsumingFromOutside { get; } = new()
		{
			typeof(MockEv3)
		};
		public override HashSet<Type> Producing { get; } = new()
		{
			typeof(MockEv2)
		};
		public override HashSet<Type>       Services             { get; }
		public override IReadOnlyList<Type> Systems { get; } = new[]
		{
			typeof(MockSystemB1),
			typeof(MockSystemB2)
		};
	}

	private class MockFeatureBg : BlahFeatureBase
	{
		public override HashSet<Type>       ConsumingFromOutside { get; }
		public override HashSet<Type>       Producing            { get; }
		public override HashSet<Type>       Services             { get; }
		public override IReadOnlyList<Type> Systems { get; } = new[]
		{
			typeof(MockSystemBg)
		};
	}

	private class MockSystemA1 : MockSystemBase
	{
		private IBlahSignalProducer<MockEv1> _ev1;
	}

	private class MockSystemA2 : MockSystemBase
	{
		private IBlahSignalConsumer<MockEv1> _ev1;
		private IBlahSignalConsumer<MockEv3> _ev3;
	}

	private class MockSystemB1 : MockSystemBase
	{
		private IBlahSignalConsumer<MockEv3> _ev3;
		
		private IBlahSignalProducer<MockEv2> _ev2;
	}

	private class MockSystemB2 : MockSystemBase
	{
		private IBlahSignalConsumer<MockEv2> _ev2;
	}

	private class MockSystemBg : MockSystemBase
	{
		private IBlahSignalConsumer<MockEv1> _ev1;
		
		private IBlahSignalProducer<MockEv3> _ev3;
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
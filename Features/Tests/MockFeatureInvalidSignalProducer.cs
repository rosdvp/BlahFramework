using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureInvalidSignalProducer : BlahFeatureBase
{
	public override HashSet<Type> Consumers { get; } = new()
	{
		typeof(MockCmdA),
	};

	public override HashSet<Type> Producers { get; } = new()
	{
		typeof(MockEventA)
	};

	public override IReadOnlyList<Type> Systems { get; } = new[]
	{
		typeof(MockInvalidSystem)
	};

	public override HashSet<Type> Services => null;

	
	public struct MockInvalidSignal : IBlahEntrySignal { }

	public class MockInvalidSystem : IBlahRunSystem
	{
		private IBlahSignalProducer<MockInvalidSignal> _pool;

		public void Run() { }
	}
}
}
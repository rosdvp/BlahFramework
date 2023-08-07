using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureInvalidSignalConsumer : BlahFeatureBase
{
	public override HashSet<Type> ConsumingFromOutside { get; }

	public override HashSet<Type> Producing { get; }

	public override IReadOnlyList<Type> Systems { get; } = new[]
	{
		typeof(MockInvalidSystem)
	};

	public override HashSet<Type> Services => null;

	
	public struct MockInvalidSignal : IBlahEntrySignal { }

	public class MockInvalidSystem : IBlahRunSystem
	{
		private IBlahSignalConsumer<MockInvalidSignal> _pool;

		public void Run() { }
	}
}
}
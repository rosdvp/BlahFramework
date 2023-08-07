using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureExcessiveSignalConsumer : BlahFeatureBase
{
	public override HashSet<Type> ConsumingFromOutside { get; } = new()
	{
		typeof(MockInvalidSignal),
	};

	public override HashSet<Type> Producing { get; }

	public override IReadOnlyList<Type> Systems { get; } = new[]
	{
		typeof(MockInvalidSystem)
	};

	public override HashSet<Type> Services => null;

	
	public struct MockInvalidSignal : IBlahEntrySignal { }

	public class MockInvalidSystem : IBlahRunSystem
	{
		public void Run() { }
	}
}
}
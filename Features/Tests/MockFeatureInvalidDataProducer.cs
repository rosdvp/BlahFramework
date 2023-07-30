using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureInvalidDataProducer : BlahFeatureBase
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

	
	public struct MockInvalidData : IBlahEntryData { }

	public class MockInvalidSystem : IBlahRunSystem
	{
		private IBlahDataProducer<MockInvalidData> _pool;

		public void Run() { }
	}
}
}
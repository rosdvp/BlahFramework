using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureA : BlahFeatureBase
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
		typeof(MockSystemA)
	};

	public override HashSet<Type> Services { get; } = new()
	{
		typeof(MockServiceA)
	};

	private class MockSystemA : IBlahRunSystem
	{
		private IBlahSignalConsumer<MockCmdA> _cmd;
	
		private IBlahSignalProducer<MockEventA> _event;

		public void Run()
		{
			foreach (ref var cmd in _cmd)
				_event.Add().Val = cmd.Val;
		}
	}
}
}
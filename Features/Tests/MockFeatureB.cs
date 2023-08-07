using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureB : BlahFeatureBase
{
	public override HashSet<Type> ConsumingFromOutside { get; } = new()
	{
		typeof(MockEventA),
	};

	public override HashSet<Type> Producing { get; } = new()
	{
		typeof(MockDataB)
	};

	public override IReadOnlyList<Type> Systems => new[]
	{
		typeof(MockSystemB)
	};

	public override HashSet<Type> Services { get; } = new()
	{
		typeof(MockServiceA),
		typeof(MockServiceB)
	};

	private class MockSystemB : IBlahRunSystem
	{
		private IBlahSignalConsumer<MockEventA> _event;

		private IBlahDataProducer<MockDataB> _data;

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
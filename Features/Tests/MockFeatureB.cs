using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureB : BlahFeatureBase
{
	public override bool IsEnabled => true;

	public override int SystemsGroupId => 0;

	public override HashSet<Type> Consumers { get; } = new()
	{
		typeof(MockEventA),
	};

	public override HashSet<Type> Producers { get; } = new()
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

		public void Run()
		{
			foreach (ref var ev in _event)
				_data.Add().Val = ev.Val;
		}
	}
}
}
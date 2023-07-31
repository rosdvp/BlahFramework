using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureInvalidService : BlahFeatureBase
{
	public override HashSet<Type> Consumers { get; }

	public override HashSet<Type> Producers { get; }

	public override IReadOnlyList<Type> Systems { get; } = new[]
	{
		typeof(MockInvalidSystem)
	};

	public override HashSet<Type> Services { get; } = new()
	{
		typeof(MockServiceA)
	};


	public class MockInvalidService : BlahServiceBase
	{
		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services) { }
	}
	
	public class MockInvalidSystem : IBlahRunSystem
	{
		private MockInvalidService _service;

		public void Run() { }
	}
}
}
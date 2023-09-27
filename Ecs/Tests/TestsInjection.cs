using System;
using Blah.Injection;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsInjection
{
	[Test]
	public void TestLight()
	{
		var ecs = new BlahEcs();

		object rawFilter = ecs.GetFilter<BlahEcsFilter<MockCompA>>(
			new[] { typeof(MockCompA) },
			null
		);
		var filter = (BlahEcsFilter<MockCompA>)rawFilter;
		
		Assert.NotNull(filter);
	}

	[Test]
	public void TestFull()
	{
		var ecs = new BlahEcs();

		var system1 = new MockSystem();
		var system2 = new MockSystem();

		var source = new BlahEcsInjectSource(ecs);

		var injector = new BlahInjector();
		injector.AddSource(source,
		                   typeof(BlahEcs),
		                   nameof(BlahEcsInjectSource.GetWorld),
		                   BlahInjector.EMethodType.Simple
		);
		injector.AddSource(source,
		                   typeof(BlahEcsFilterProxy),
		                   nameof(source.GetFilter),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);

		injector.InjectInto(system1);
		injector.InjectInto(system2);


		Assert.NotNull(system1.ecs);
		Assert.NotNull(system1.Filter1);
		Assert.NotNull(system1.Filter2A);
		Assert.NotNull(system1.Filter2B);
		Assert.NotNull(system1.Filter3);

		Assert.IsFalse(system1.Filter1.IsSame(system1.Filter2A));

		Assert.IsTrue(system1.Filter2A.IsSame(system1.Filter2B));
		Assert.IsTrue(system1.Filter2A.IsSame(system1.Filter3));

		Assert.AreSame(system1.ecs, system2.ecs);
		Assert.IsTrue(system1.Filter1.IsSame(system2.Filter1));
		Assert.IsTrue(system1.Filter2A.IsSame(system2.Filter2A));
		Assert.IsTrue(system1.Filter2B.IsSame(system2.Filter2B));
		Assert.IsTrue(system1.Filter3.IsSame(system2.Filter3));
	}



	private class MockSystem
	{
		private BlahEcs _ecs;

		private BlahEcsFilter<MockCompA>            _filter1;
		private BlahEcsFilter<MockCompA, MockCompB> _filter2A;
		private BlahEcsFilter<MockCompA, MockCompB> _filter2B;
		private BlahEcsFilter<MockCompB, MockCompA> _filter3;


		public BlahEcs                        ecs    => _ecs;
		public BlahEcsFilter<MockCompA>            Filter1  => _filter1;
		public BlahEcsFilter<MockCompA, MockCompB> Filter2A => _filter2A;
		public BlahEcsFilter<MockCompA, MockCompB> Filter2B => _filter2B;
		public BlahEcsFilter<MockCompB, MockCompA> Filter3  => _filter3;
	}

	private struct MockCompA : IBlahEntryEcs { }
	private struct MockCompB : IBlahEntryEcs { }
}
}
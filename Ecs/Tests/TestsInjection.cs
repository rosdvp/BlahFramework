using System;
using System.Collections.Generic;
using Blah.Injection;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsInjection
{
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
		                   nameof(BlahEcsInjectSource.GetEcs),
		                   BlahInjector.EMethodType.Simple
		);
		injector.AddSource(source,
		                   typeof(BlahEcsFilter),
		                   nameof(BlahEcsInjectSource.GetFilter),
		                   BlahInjector.EMethodType.GenericAcceptFieldType
		);

		injector.InjectInto(system1);
		injector.InjectInto(system2);


		Assert.NotNull(system1.Ecs);
		Assert.NotNull(system1.Filter1);
		Assert.NotNull(system1.Filter2A);
		Assert.NotNull(system1.Filter2B);
		Assert.NotNull(system1.Filter3);
		Assert.NotNull(system1.Filter4);
		Assert.NotNull(system1.Filter5);
		Assert.NotNull(system1.Filter6);
		Assert.NotNull(system1.Filter7);

		Assert.AreNotEqual(system1.Filter1, system1.Filter2A);
		Assert.AreEqual(system1.Filter2A, system1.Filter2B);
		Assert.AreEqual(system1.Filter2A, system1.Filter3);
		
		Assert.AreNotEqual(system1.Filter4, system1.Filter5);
		Assert.AreEqual(system1.Filter6, system1.Filter7);

		Assert.AreEqual(system1.Ecs, system2.Ecs);
		Assert.AreEqual(system1.Filter1, system2.Filter1);
		Assert.AreEqual(system1.Filter2A, system2.Filter2A);
		Assert.AreEqual(system1.Filter2B, system2.Filter2B);
		Assert.AreEqual(system1.Filter3, system2.Filter3);
		Assert.AreEqual(system1.Filter4, system2.Filter4);
		Assert.AreEqual(system1.Filter5, system2.Filter5);
		Assert.AreEqual(system1.Filter6, system2.Filter6);
		Assert.AreEqual(system1.Filter7, system2.Filter7);
	}



	private class MockSystem
	{
		private BlahEcs _ecs;

		private BlahEcsFilter<MockCompA> _filter1;
		private BlahEcsFilter<MockCompA, MockCompB> _filter2A;
		private BlahEcsFilter<MockCompA, MockCompB> _filter2B;
		private BlahEcsFilter<MockCompB, MockCompA> _filter3;

		private BlahEcsFilter<MockCompA>.Exc<MockCompB>            _filter4;
		private BlahEcsFilter<MockCompB>.Exc<MockCompA>            _filter5;
		private BlahEcsFilter<MockCompA, MockCompB>.Exc<MockCompC> _filter6;
		private BlahEcsFilter<MockCompB, MockCompA>.Exc<MockCompC> _filter7;


		public BlahEcs                             Ecs      => _ecs;
		public BlahEcsFilter<MockCompA>            Filter1  => _filter1;
		public BlahEcsFilter<MockCompA, MockCompB> Filter2A => _filter2A;
		public BlahEcsFilter<MockCompA, MockCompB> Filter2B => _filter2B;
		public BlahEcsFilter<MockCompB, MockCompA> Filter3  => _filter3;

		public BlahEcsFilter<MockCompA>.Exc<MockCompB>            Filter4  => _filter4;
		public BlahEcsFilter<MockCompB>.Exc<MockCompA>            Filter5  => _filter5;
		public BlahEcsFilter<MockCompA, MockCompB>.Exc<MockCompC> Filter6 => _filter6;
		public BlahEcsFilter<MockCompB, MockCompA>.Exc<MockCompC> Filter7 => _filter7;
	}

	private struct MockCompA : IBlahEntryEcs { }
	private struct MockCompB : IBlahEntryEcs { }
	private struct MockCompC : IBlahEntryEcs { }
}
}
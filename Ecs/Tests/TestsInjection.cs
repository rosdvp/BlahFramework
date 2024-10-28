using System;
using System.Collections.Generic;
using System.Reflection;
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

		var context = new TestContext();
		context.Ecs = ecs;
		
		var system1 = new MockSystem();
		var system2 = new MockSystem();

		var injector = new BlahInjector();
		injector.AddSource(context,
		                   typeof(BlahEcs),
		                   nameof(TestContext.GetEcs),
		                   BlahInjector.EMethodType.Simple
		);
		injector.AddSource(ecs,
		                   typeof(BlahFilter),
		                   nameof(BlahEcs.GetFilter),
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

		AssertFilter(system1.Filter1, new[] { typeof(MockCompA) }, null);
		AssertFilter(system1.Filter2A, new[] { typeof(MockCompA), typeof(MockCompB) }, null);
		AssertFilter(system1.Filter2B, new[] { typeof(MockCompA), typeof(MockCompB) }, null);
		AssertFilter(system1.Filter3, new[] { typeof(MockCompA), typeof(MockCompB) }, null);
		AssertFilter(system1.Filter4, new[] { typeof(MockCompA) }, new[] { typeof(MockCompB) });
		AssertFilter(system1.Filter5, new[] { typeof(MockCompB) }, new[] { typeof(MockCompA) });
		AssertFilter(system1.Filter6, new[] { typeof(MockCompA), typeof(MockCompB) }, new[] { typeof(MockCompC) });
		AssertFilter(system1.Filter7, new[] { typeof(MockCompA), typeof(MockCompB) }, new[] { typeof(MockCompC) });
	}


	private void AssertFilter(BlahFilter filter, Type[] incs, Type[] excs)
	{
		var incsSet = new HashSet<Type>(incs);
		var excsSet = excs == null ? new HashSet<Type>() : new HashSet<Type>(excs);

		var core = filter.TestsCore;
		
		foreach (var incPool in core.TestsIncCompsPools)
		{
			var poolType = incPool.GetType();
			var compType = poolType.GenericTypeArguments[0];
			Assert.IsTrue(incsSet.Remove(compType), $"inc {compType.Name}");
		}
		Assert.IsTrue(incsSet.Count == 0);

		foreach (var excPool in core.TestsExcCompsPools)
		{
			var poolType = excPool.GetType();
			var compType = poolType.GenericTypeArguments[0];
			Assert.IsTrue(excsSet.Remove(compType), $"exc {compType.Name}");
		}
		Assert.IsTrue(excsSet.Count == 0);
	}

	private class TestContext
	{
		public BlahEcs Ecs;

		public BlahEcs GetEcs() => Ecs;
	}

	private class MockSystem
	{
		private BlahEcs _ecs;

		private BlahFilter<MockCompA> _filter1;
		private BlahFilter<MockCompA, MockCompB> _filter2A;
		private BlahFilter<MockCompA, MockCompB> _filter2B;
		private BlahFilter<MockCompB, MockCompA> _filter3;

		private BlahFilter<MockCompA>.Exc<MockCompB>            _filter4;
		private BlahFilter<MockCompB>.Exc<MockCompA>            _filter5;
		private BlahFilter<MockCompA, MockCompB>.Exc<MockCompC> _filter6;
		private BlahFilter<MockCompB, MockCompA>.Exc<MockCompC> _filter7;


		public BlahEcs                             Ecs      => _ecs;
		public BlahFilter<MockCompA>            Filter1  => _filter1;
		public BlahFilter<MockCompA, MockCompB> Filter2A => _filter2A;
		public BlahFilter<MockCompA, MockCompB> Filter2B => _filter2B;
		public BlahFilter<MockCompB, MockCompA> Filter3  => _filter3;

		public BlahFilter<MockCompA>.Exc<MockCompB>            Filter4  => _filter4;
		public BlahFilter<MockCompB>.Exc<MockCompA>            Filter5  => _filter5;
		public BlahFilter<MockCompA, MockCompB>.Exc<MockCompC> Filter6 => _filter6;
		public BlahFilter<MockCompB, MockCompA>.Exc<MockCompC> Filter7 => _filter7;
	}

	private struct MockCompA : IBlahEntryComp { }
	private struct MockCompB : IBlahEntryComp { }
	private struct MockCompC : IBlahEntryComp { }
}
}
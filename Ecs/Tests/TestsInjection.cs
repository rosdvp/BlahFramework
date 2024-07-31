using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Features;
using Blah.Injection;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsInjection
{
	[Test]
	public void TestFull()
	{
		var context = new TestBlahContext();
		context.TestsAddSourcesToInjector();
		
		var system1 = new MockSystem();
		var system2 = new MockSystem();

		context.Injector.InjectInto(system1);
		context.Injector.InjectInto(system2);


		Assert.NotNull(system1.Ecs);
		Assert.NotNull(system1.Filter1?.TestsCore);
		Assert.NotNull(system1.Filter2A?.TestsCore);
		Assert.NotNull(system1.Filter2B?.TestsCore);
		Assert.NotNull(system1.Filter3?.TestsCore);
		Assert.NotNull(system1.Filter4?.TestsCore);
		Assert.NotNull(system1.Filter5?.TestsCore);
		Assert.NotNull(system1.Filter6?.TestsCore);
		Assert.NotNull(system1.Filter7?.TestsCore);

		Assert.AreNotEqual(system1.Filter1.TestsCore, system1.Filter2A.TestsCore);
		Assert.AreEqual(system1.Filter2A.TestsCore, system1.Filter2B.TestsCore);
		Assert.AreEqual(system1.Filter2A.TestsCore, system1.Filter3.TestsCore);

		Assert.AreNotEqual(system1.Filter4.TestsCore, system1.Filter5.TestsCore);
		Assert.AreEqual(system1.Filter6.TestsCore, system1.Filter7.TestsCore);

		Assert.AreEqual(system1.Ecs, system2.Ecs);
		Assert.AreEqual(system1.Filter1.TestsCore, system2.Filter1.TestsCore);
		Assert.AreEqual(system1.Filter2A.TestsCore, system2.Filter2A.TestsCore);
		Assert.AreEqual(system1.Filter2B.TestsCore, system2.Filter2B.TestsCore);
		Assert.AreEqual(system1.Filter3.TestsCore, system2.Filter3.TestsCore);
		Assert.AreEqual(system1.Filter4.TestsCore, system2.Filter4.TestsCore);
		Assert.AreEqual(system1.Filter5.TestsCore, system2.Filter5.TestsCore);
		Assert.AreEqual(system1.Filter6.TestsCore, system2.Filter6.TestsCore);
		Assert.AreEqual(system1.Filter7.TestsCore, system2.Filter7.TestsCore);

		AssertFilter(system1.Filter1, new[] { typeof(CompA) }, null);
		AssertFilter(system1.Filter2A, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter2B, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter3, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter4, new[] { typeof(CompA) }, new[] { typeof(CompB) });
		AssertFilter(system1.Filter5, new[] { typeof(CompB) }, new[] { typeof(CompA) });
		AssertFilter(system1.Filter6, new[] { typeof(CompA), typeof(CompB) }, new[] { typeof(CompC) });
		AssertFilter(system1.Filter7, new[] { typeof(CompA), typeof(CompB) }, new[] { typeof(CompC) });

		Assert.AreEqual(context.Ecs.GetCompGetter<CompA>(), system1.Filter1.A);
		Assert.AreEqual(system1.Filter1.A, system2.Filter2A.A);
		Assert.AreEqual(system1.Filter1.A, system2.Filter2B.A);
		Assert.AreEqual(system1.Filter1.A, system2.Filter3.A);
		Assert.AreEqual(system1.Filter1.A, system2.Filter4.A);
		Assert.AreEqual(system1.Filter2A.B, system2.Filter5.B);
		Assert.AreEqual(system1.Filter1.A, system2.Filter6.A);
		Assert.AreEqual(system1.Filter1.A, system2.Filter7.A);
	}


	private void AssertFilter(BlahEcsFilter filter, Type[] incs, Type[] excs)
	{
		var incsSet = new HashSet<Type>(incs);
		var excsSet = excs == null ? new HashSet<Type>() : new HashSet<Type>(excs);
		
		foreach (object incPool in filter.TestsCore.TestsIncPools)
		{
			var poolType = incPool.GetType();
			var compType = poolType.GenericTypeArguments[0];
			Assert.IsTrue(incsSet.Remove(compType), $"inc {compType.Name}");
		}
		Assert.IsTrue(incsSet.Count == 0);

		if (filter.TestsCore.TestsExcPools != null)
			foreach (object excPool in filter.TestsCore.TestsExcPools)
			{
				var poolType = excPool.GetType();
				var compType = poolType.GenericTypeArguments[0];
				Assert.IsTrue(excsSet.Remove(compType), $"exc {compType.Name}");
			}
		Assert.IsTrue(excsSet.Count == 0);
	}


	private class TestBlahContext : BlahContextBase
	{
		public override Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; }
	}

	private class MockSystem
	{
		public class FilterA : BlahEcsFilter
		{
			public BlahEcsGet<CompA> A = Inc;
		}

		public class FilterAB : BlahEcsFilter
		{
			public BlahEcsGet<CompA> A = Inc;
			public BlahEcsGet<CompB> B = Inc;
		}

		public class FilterBA : BlahEcsFilter
		{
			public BlahEcsGet<CompB> B = Inc;
			public BlahEcsGet<CompA> A = Inc;
		}

		public class FilterAexcB : BlahEcsFilter
		{
			public BlahEcsGet<CompA> A = Inc;

			private BlahEcsGet<CompB> _b = Exc;
		}

		public class FilterBexcA : BlahEcsFilter
		{
			public BlahEcsGet<CompB> B = Inc;

			private BlahEcsGet<CompA> _a = Exc;
		}

		public class FilterABexcC : BlahEcsFilter
		{
			public BlahEcsGet<CompA> A = Inc;
			public BlahEcsGet<CompB> B = Inc;

			private BlahEcsGet<CompC> _c = Exc;
		}

		public class FilterBAexcC : BlahEcsFilter
		{
			public BlahEcsGet<CompB> B = Inc;
			public BlahEcsGet<CompA> A = Inc;

			private BlahEcsGet<CompC> _c = Exc;
		}


		private BlahEcs _ecs;

		private FilterA  _filter1;
		private FilterAB _filter2A;
		private FilterAB _filter2B;
		private FilterBA _filter3;

		private FilterAexcB  _filter4;
		private FilterBexcA  _filter5;
		private FilterABexcC _filter6;
		private FilterBAexcC _filter7;


		public BlahEcs  Ecs      => _ecs;
		public FilterA  Filter1  => _filter1;
		public FilterAB Filter2A => _filter2A;
		public FilterAB Filter2B => _filter2B;
		public FilterBA Filter3  => _filter3;

		public FilterAexcB  Filter4 => _filter4;
		public FilterBexcA  Filter5 => _filter5;
		public FilterABexcC Filter6 => _filter6;
		public FilterBAexcC Filter7 => _filter7;
	}

	private struct CompA : IBlahEntryEcs { }

	private struct CompB : IBlahEntryEcs { }

	private struct CompC : IBlahEntryEcs { }
}
}
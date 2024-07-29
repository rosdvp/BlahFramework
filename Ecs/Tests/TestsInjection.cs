using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Injection;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsInjection
{
	/*
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

		AssertFilter(system1.Filter1, new[] { typeof(CompA) }, null);
		AssertFilter(system1.Filter2A, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter2B, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter3, new[] { typeof(CompA), typeof(CompB) }, null);
		AssertFilter(system1.Filter4, new[] { typeof(CompA) }, new[] { typeof(CompB) });
		AssertFilter(system1.Filter5, new[] { typeof(CompB) }, new[] { typeof(CompA) });
		AssertFilter(system1.Filter6, new[] { typeof(CompA), typeof(CompB) }, new[] { typeof(CompC) });
		AssertFilter(system1.Filter7, new[] { typeof(CompA), typeof(CompB) }, new[] { typeof(CompC) });

		Assert.AreEqual(ecs.GetRead<CompA>(), system1.Filter1.A);
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

		var fieldCore = typeof(BlahEcsFilter).GetField(
			"_filter",
			BindingFlags.Instance
			| BindingFlags.NonPublic
		);
		object objCore = fieldCore.GetValue(filter);

		var fieldIncsPools = typeof(BlahEcsFilterCore).GetField(
			"_incCompsPools",
			BindingFlags.Instance
			| BindingFlags.NonPublic
		);
		object objIncsPools = fieldIncsPools.GetValue(objCore);

		var arrIncsPools = (Array)objIncsPools;
		foreach (object objIncPool in arrIncsPools)
		{
			var poolType = objIncPool.GetType();
			var compType = poolType.GenericTypeArguments[0];
			Assert.IsTrue(incsSet.Remove(compType), $"inc {compType.Name}");
		}
		Assert.IsTrue(incsSet.Count == 0);

		var fieldExcsPools = typeof(BlahEcsFilterCore).GetField(
			"_excCompsPools",
			BindingFlags.Instance
			| BindingFlags.NonPublic
		);
		object objExcsPools = fieldExcsPools.GetValue(objCore);

		var arrExcsPools = (Array)objExcsPools;
		if (arrExcsPools != null)
			foreach (object objExcPool in arrExcsPools)
			{
				var poolType = objExcPool.GetType();
				var compType = poolType.GenericTypeArguments[0];
				Assert.IsTrue(excsSet.Remove(compType), $"exc {compType.Name}");
			}
		Assert.IsTrue(excsSet.Count == 0);
	}


	private class MockSystem
	{
		public class FilterA : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompA> A;
		}

		public class FilterAB : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompA> A;
			public IBlahEcsCompRead<CompB> B;
		}

		public class FilterBA : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompB> B;
			public IBlahEcsCompRead<CompA> A;
		}

		public class FilterAexcB : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompA> A;

			public override Type[] Exc { get; } = { typeof(CompB) };
		}

		public class FilterBexcA : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompB> B;

			public override Type[] Exc { get; } = { typeof(CompA) };
		}

		public class FilterABexcC : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompA> A;
			public IBlahEcsCompRead<CompB> B;

			public override Type[] Exc { get; } = { typeof(CompC) };
		}

		public class FilterBAexcC : BlahEcsFilter
		{
			public IBlahEcsCompRead<CompB> B;
			public IBlahEcsCompRead<CompA> A;

			public override Type[] Exc { get; } = { typeof(CompC) };
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
	*/
}
}
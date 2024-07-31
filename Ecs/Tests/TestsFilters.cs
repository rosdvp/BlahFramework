using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Blah.Ecs.Tests
{
internal class TestsFilters
{
	[Test]
	public void Test_SameCompsInFilters_FiltersCoresSame()
	{
		var ecs = new BlahEcs();

		var filter1 = ecs.CreateFilter<FilterAB>();
		var filter2 = ecs.CreateFilter<FilterAB>();
		var filter3 = ecs.CreateFilter<FilterBA>();
		
		Assert.AreEqual(filter1.TestsCore, filter2.TestsCore);
		Assert.AreEqual(filter1.TestsCore, filter3.TestsCore);
		Assert.AreEqual(filter2.TestsCore, filter3.TestsCore);

		
		var filter4 = ecs.CreateFilter<FilterABC>();
		var filter5 = ecs.CreateFilter<FilterAexcB>();
		Assert.AreNotEqual(filter1.TestsCore, filter4.TestsCore);
		Assert.AreNotEqual(filter1.TestsCore, filter5.TestsCore);
		
		var filter6 = ecs.CreateFilter<FilterAexcBC>();
		var filter7 = ecs.CreateFilter<FilterAexcCB>();
		Assert.AreEqual(filter6.TestsCore, filter7.TestsCore);

		var filter8 = ecs.CreateFilter<FilterBexcAC>();
		Assert.AreNotEqual(filter7.TestsCore, filter8.TestsCore);
	}
	
	
	[Test]
	public void Test_EmptyFilters()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();
		
		Assert.IsTrue(filter.IsEmpty);
		
		foreach (var e in filter)
			Assert.Fail();

		var ent = ecs.CreateEntity();
		pool.Add(ent);
        
		Assert.IsFalse(filter.IsEmpty);
		
		filter.A.Remove(ent);
		
		Assert.IsTrue(filter.IsEmpty);
	}
	

	[Test]
	public void Test_AddComp_FilterUpdated([NUnit.Framework.Range(1, 10)] int count)
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		for (var i = 0; i < count; i++)
		{
			var ent = ecs.CreateEntity();
			pool.Add(ent).Val = i + 1;

			var expected = Enumerable.Range(1, i + 1).ToList();
			foreach (var e in filter)
				Assert.IsTrue(expected.Remove(filter.A.Get(e).Val));
			
			foreach (int val in expected)
				Assert.Fail($"{val} left");
		}
	}

	[Test]
	public void Test_MultipleEntitiesRemoveCompOfOne_FilterUpdated()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		var ent1 = ecs.CreateEntity();
		var ent2 = ecs.CreateEntity();
		var ent3 = ecs.CreateEntity();

		pool.Add(ent1).Val = 1;
		pool.Add(ent2).Val = 2;
		pool.Add(ent3).Val = 3;

		var expected = new List<int> { 1, 2, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(filter.A.Get(e).Val));
        Assert.Zero(expected.Count);

        filter.A.Remove(ent2);
        
        expected = new List<int> { 1, 3 };
        foreach (var e in filter)
	        Assert.IsTrue(expected.Remove(filter.A.Get(e).Val));
        Assert.Zero(expected.Count);
	}
	

	[Test]
	public void Test_SwitchCompAToB_FiltersUpdated()
	{
		var ecs     = new BlahEcs();
		var poolA   = ecs.GetCompFull<CompA>();
		var poolB   = ecs.GetCompFull<CompB>();
		var filterA = ecs.CreateFilter<FilterA>();
		var filterB = ecs.CreateFilter<FilterB>();

		foreach (var e in filterA)
			Assert.Fail();
		foreach (var e in filterB)
			Assert.Fail();

		var ent = ecs.CreateEntity();
		poolA.Add(ent);

		var iterationsCount = 0;
		foreach (var e in filterA)
			iterationsCount += 1;
		Assert.AreEqual(1, iterationsCount);
		foreach (var e in filterB)
			Assert.Fail();
        
		filterA.A.Remove(ent);
		poolB.Add(ent);

		foreach (var e in filterA)
			Assert.Fail();
		iterationsCount = 0;
		foreach (var e in filterB)
			iterationsCount += 1;
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_AddExtraComp_StillInFilter()
	{
		var ecs     = new BlahEcs();
		var poolA   = ecs.GetCompFull<CompA>();
		var poolB   = ecs.GetCompFull<CompB>();
		var filterA = ecs.CreateFilter<FilterA>();
		var filterB = ecs.CreateFilter<FilterB>();

		var ent = ecs.CreateEntity();
		poolA.Add(ent).Val = 2;
		poolB.Add(ent).Val = 3;

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, filterA.A.Get(e).Val);
			Assert.AreEqual(3, filterB.B.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		iterationsCount = 0;
		foreach (var e in filterB)
		{
			Assert.AreEqual(2, filterA.A.Get(e).Val);
			Assert.AreEqual(3, filterB.B.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_AddCompThenRemove_FilterUpdated()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		var ent = ecs.CreateEntity();
		pool.Add(ent).Val = 2;

		foreach (var e in filter)
			Assert.AreEqual(2, filter.A.Get(e).Val);
        
		filter.A.Remove(ent);
		foreach (var e in filter)
			Assert.Fail();

		pool.Add(ent).Val = 1;
		var iterationsCount = 0;
		foreach (var e in filter)
		{
			Assert.AreEqual(1, filter.A.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_DelayedCreateFilter_FilterUpdated()
	{
		var ecs  = new BlahEcs();
		var pool = ecs.GetCompFull<CompA>();

		var ent = ecs.CreateEntity();
		pool.Add(ent).Val = 2;
        
		var filterA = ecs.CreateFilter<FilterA>();
		var filterB = ecs.CreateFilter<FilterB>();

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, filterA.A.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		foreach (var e in filterB)
			Assert.Fail();
	}

	[Test]
	public void Test_RemoveCompDuringIteration_FilterUpdated()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		pool.Add(ecs.CreateEntity()).Val = 1;
		pool.Add(ecs.CreateEntity()).Val = 2;
		pool.Add(ecs.CreateEntity()).Val = 3;

		int iterationsCount = 0;
		foreach (var e in filter)
		{
			if (filter.A.Get(e).Val == 2)
			{
				filter.A.Remove(e);
				Assert.IsFalse(filter.A.Has(e));
			}
			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		var expected = new List<int> { 1, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(filter.A.Get(e).Val));
		Assert.IsTrue(expected.Count == 0);
	}
	
	[Test]
	public void Test_DestroyEntityDuringIteration_SameFilterUpdated()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		for (var i = 0; i < 20; i++)
		{
			Debug.Log($"iter {i}");

			foreach (var ent in filter)
				filter.A.Get(ent).Val += 1;
			
			foreach (var ent in filter)
				if (filter.A.Get(ent).Val == 5)
					ecs.DestroyEntity(ent);
			
			pool.Add(ecs.CreateEntity()).Val = 1;
		}

		var expected = new List<int> { 1, 2, 3, 4 };
		foreach (var ent in filter)
			Assert.IsTrue(expected.Remove(filter.A.Get(ent).Val), $"val {filter.A.Get(ent).Val}");
		Assert.IsTrue(expected.Count == 0);
	}

	[Test]
	public void Test_DestroyEntityDuringIteration_AnotherFilterUpdated()
	{
		var ecs      = new BlahEcs();
		var poolA    = ecs.GetCompFull<CompA>();
		var poolB    = ecs.GetCompFull<CompB>();
		var filterA  = ecs.CreateFilter<FilterA>();
		var filterAb = ecs.CreateFilter<FilterAB>();

		var ent1 = ecs.CreateEntity();
		poolA.Add(ent1).Val = 1;
		poolB.Add(ent1).Val = 11;
		var ent2 = ecs.CreateEntity();
		poolA.Add(ent2).Val = 2;
		poolB.Add(ent2).Val = 22;
		var ent3 = ecs.CreateEntity();
		poolA.Add(ent3).Val = 3;
		poolB.Add(ent3).Val = 33;

		var expected = new List<int> { 11, 22, 33};

		var iterationsCount = 0;
		foreach (var e1 in filterA)
		{
			if (filterA.A.Get(e1).Val == 2)
			{
				expected.Remove(22);
				ecs.DestroyEntity(e1);
			}

			var expectedInIter = new List<int>(expected);
			foreach (var e2 in filterAb)
				Assert.IsTrue(expectedInIter.Remove(filterAb.B.Get(e2).Val));
			if (expectedInIter.Count > 0)
				Assert.Fail();

			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		expected = new List<int>() { 1, 3 };
		foreach (var e1 in filterA)
			Assert.IsTrue(expected.Remove(filterA.A.Get(e1).Val));
		Assert.IsTrue(expected.Count == 0);
	}
	
	[Test]
	public void Test_DestroyAllEntitiesThenCreate_SameFilterUpdated()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		for (var i = 0; i < 10; i++)
			pool.Add(ecs.CreateEntity()).Val = i;

		foreach (var ent in filter)
			ecs.DestroyEntity(ent);

		for (var i = 10; i < 20; i++)
			pool.Add(ecs.CreateEntity()).Val = i;

		int iterationsCount = 0;
		var expected        = Enumerable.Range(10, 10).ToList();
		foreach (var ent in filter)
		{
			Assert.IsTrue(expected.Remove(filter.A.Get(ent).Val));
			iterationsCount += 1;
		}
		Assert.AreEqual(10, iterationsCount);
		
		foreach (int exp in expected)
			Assert.Fail($"{exp} left");
	}
	
	[Test]
	public void Test_EntHasExcComp_NotInFilter()
	{
		var ecs    = new BlahEcs();
		var poolA  = ecs.GetCompFull<CompA>();
		var poolB  = ecs.GetCompFull<CompB>();
		var filter = ecs.CreateFilter<FilterAexcB>();

		var ents = new List<BlahEcsEntity>();
		ents.Add(ecs.CreateEntity());
		ents.Add(ecs.CreateEntity());
		ents.Add(ecs.CreateEntity());
		var temp = new List<BlahEcsEntity>();

		foreach (var ent in ents)
			poolA.Add(ent);

		temp.AddRange(ents);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);

		poolB.Add(ents[1]);

		temp.Add(ents[0]);
		temp.Add(ents[2]);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);
		
		poolB.Remove(ents[1]);
		
		temp.AddRange(ents);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);
	}
	

	[Test]
	public void Test_GetAny()
	{
		var ecs    = new BlahEcs();
		var pool   = ecs.GetCompFull<CompA>();
		var filter = ecs.CreateFilter<FilterA>();

		if (filter.TryGetAny(out var ent))
			Assert.Fail();

		var ent1 = ecs.CreateEntity();
		pool.Add(ent1).Val = 3;
        
        Assert.AreEqual(3, filter.A.Get(filter.GetAny()).Val);
        Assert.IsTrue(filter.TryGetAny(out var ent2));
        Assert.AreEqual(3, filter.A.Get(ent2).Val);
        
        filter.A.Remove(ent1);
        Assert.Throws<Exception>(() => filter.GetAny());
	}
	

	private class FilterA : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;
	}
	
	private class FilterB : BlahEcsFilter
	{
		public BlahEcsGet<CompB> B = Inc;
	}

	private class FilterAB : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;
		public BlahEcsGet<CompB> B = Inc;
	}

	private class FilterAexcB : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;

		public BlahEcsGet<CompB> B = Exc;
	}

	private class FilterBA : BlahEcsFilter
	{
		public BlahEcsGet<CompB> B = Inc;
		public BlahEcsGet<CompA> A = Inc;
	}

	private class FilterABC : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;
		public BlahEcsGet<CompB> B = Inc;
		public BlahEcsGet<CompC> C = Inc;
	}

	private class FilterAexcBC : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;
		
		public BlahEcsGet<CompB> B = Exc;
		public BlahEcsGet<CompC> C = Exc;
	}
	
	private class FilterAexcCB : BlahEcsFilter
	{
		public BlahEcsGet<CompA> A = Inc;
		
		public BlahEcsGet<CompC> C = Exc;
		public BlahEcsGet<CompB> B = Exc;
	}

	private class FilterBexcAC : BlahEcsFilter
	{
		public BlahEcsGet<CompB> B = Inc;
		
		public BlahEcsGet<CompA> A = Exc;
		public BlahEcsGet<CompC> C = Exc;
	}

	private struct CompA : IBlahEntryEcs
	{
		public int Val;
	}

	private struct CompB : IBlahEntryEcs
	{
		public int Val;
	}

	private struct CompC : IBlahEntryEcs
	{
		public int Val;
	}
}
}
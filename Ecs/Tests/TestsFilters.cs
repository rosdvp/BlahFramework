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
	public void Test_SameCompsInFilters_FiltersSame()
	{
		var ecs     = new BlahEcs();
		var filter1 = ecs.GetFilter<BlahFilter<CompA, CompB>>();
		var filter2 = ecs.GetFilter<BlahFilter<CompA, CompB>>();
		var filter3 = ecs.GetFilter<BlahFilter<CompB, CompA>>();
		
		Assert.AreEqual(filter1, filter2);
		Assert.AreEqual(filter1, filter3);
		Assert.AreEqual(filter2, filter3);

		var filter4 = ecs.GetFilter<BlahFilter<CompA, CompB, CompC>>();
		var filter5 = ecs.GetFilter<BlahFilter<CompA>.Exc<CompB>>();
		Assert.AreNotEqual(filter1, filter4);
		Assert.AreNotEqual(filter1, filter5);

		var filter6 = ecs.GetFilter<BlahFilter<CompA>.Exc<CompB, CompC>>();
		var filter7 = ecs.GetFilter<BlahFilter<CompA>.Exc<CompC, CompB>>();
		Assert.AreEqual(filter6, filter7);

		var filter8 = ecs.GetFilter<BlahFilter<CompB>.Exc<CompA, CompC>>();
		Assert.AreNotEqual(filter7, filter8);
	}
	
	
	[Test]
	public void Test_EmptyFilters()
	{
		var ecs    = new BlahEcs();
		var write  = ecs.GetCompFull<CompA>();
		var filter = ecs.GetFilter<BlahFilter<CompA>>();
		
		Assert.IsTrue(filter.IsEmpty);
		
		foreach (var e in filter)
			Assert.Fail();

		var ent = ecs.CreateEnt();
		write.Add(ent);
        
		Assert.IsFalse(filter.IsEmpty);
		
		write.Remove(ent);
		
		Assert.IsTrue(filter.IsEmpty);
	}
	

	[Test]
	public void Test_AddComp_FilterUpdated([NUnit.Framework.Range(1, 10)] int count)
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();
		
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		for (var i = 0; i < count; i++)
		{
			var ent = ecs.CreateEnt();
			write.Add(ent).Val = i + 1;

			var expected = Enumerable.Range(1, i + 1).ToList();
			foreach (var e in filter)
				Assert.IsTrue(expected.Remove(read.Get(e).Val));
			
			foreach (int val in expected)
				Assert.Fail($"{val} left");
		}
	}

	[Test]
	public void Test_MultipleEntitiesRemoveCompOfOne_FilterUpdated()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();
		
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		var ent1 = ecs.CreateEnt();
		var ent2 = ecs.CreateEnt();
		var ent3 = ecs.CreateEnt();

		write.Add(ent1).Val   = 1;
		write.Add(ent2).Val   = 2;
		write.Add(ent3).Val   = 3;

		var expected = new List<int> { 1, 2, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(read.Get(e).Val));
        Assert.Zero(expected.Count);

        read.Remove(ent2);
        
        expected = new List<int> { 1, 3 };
        foreach (var e in filter)
	        Assert.IsTrue(expected.Remove(read.Get(e).Val));
        Assert.Zero(expected.Count);
	}
	

	[Test]
	public void Test_SwitchCompAToB_FiltersUpdated()
	{
		var ecs     = new BlahEcs();
		var writeA  = ecs.GetCompFull<CompA>();
		var readA   = ecs.GetCompGetter<CompA>();
		var writeB  = ecs.GetCompFull<CompB>();
		var filterA = ecs.GetFilter<BlahFilter<CompA>>();
		var filterB = ecs.GetFilter<BlahFilter<CompB>>();

		foreach (var e in filterA)
			Assert.Fail();
		foreach (var e in filterB)
			Assert.Fail();

		var ent = ecs.CreateEnt();
		writeA.Add(ent);

		var iterationsCount = 0;
		foreach (var e in filterA)
			iterationsCount += 1;
		Assert.AreEqual(1, iterationsCount);
		foreach (var e in filterB)
			Assert.Fail();
        
		readA.Remove(ent);
		writeB.Add(ent);

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
		var ecs    = new BlahEcs();
		var writeA = ecs.GetCompFull<CompA>();
		var readA  = ecs.GetCompGetter<CompA>();
		var writeB = ecs.GetCompFull<CompB>();
		var readB  = ecs.GetCompGetter<CompB>();
		
		var filterA = ecs.GetFilter<BlahFilter<CompA>>();
		var filterB = ecs.GetFilter<BlahFilter<CompB>>();

		var ent = ecs.CreateEnt();
		writeA.Add(ent).Val = 2;
		writeB.Add(ent).Val = 3;

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, readA.Get(e).Val);
			Assert.AreEqual(3, readB.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		iterationsCount = 0;
		foreach (var e in filterB)
		{
			Assert.AreEqual(2, readA.Get(e).Val);
			Assert.AreEqual(3, readB.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_AddCompThenRemove_FilterUpdated()
	{
		var ecs    = new BlahEcs();
		var write  = ecs.GetCompFull<CompA>();
		var read   = ecs.GetCompGetter<CompA>();
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		var ent = ecs.CreateEnt();
		write.Add(ent).Val = 2;

		foreach (var e in filter)
			Assert.AreEqual(2, read.Get(e).Val);
        
		read.Remove(ent);
		foreach (var e in filter)
			Assert.Fail();

		write.Add(ent).Val = 1;
		var iterationsCount = 0;
		foreach (var e in filter)
		{
			Assert.AreEqual(1, read.Get(e).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_DelayedCreateFilter_FilterUpdated()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();

		var ent = ecs.CreateEnt();
		write.Add(ent).Val   = 2;
        
		var filterA = ecs.GetFilter<BlahFilter<CompA>>();
		var filterB = ecs.GetFilter<BlahFilter<CompB>>();

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, read.Get(e).Val);
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
		var write  = ecs.GetCompFull<CompA>();
		var read   = ecs.GetCompGetter<CompA>();
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		write.Add(ecs.CreateEnt()).Val = 1;
		write.Add(ecs.CreateEnt()).Val = 2;
		write.Add(ecs.CreateEnt()).Val = 3;

		int iterationsCount = 0;
		foreach (var e in filter)
		{
			if (read.Get(e).Val == 2)
			{
				read.Remove(e);
				Assert.IsFalse(read.Has(e));
			}
			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		var expected = new List<int> { 1, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(read.Get(e).Val));
	}

	[Test]
	public void Test_DestroyEntity_FiltersUpdated()
	{
		var ecs     = new BlahEcs();
		var writeA  = ecs.GetCompFull<CompA>();
		var readA   = ecs.GetCompGetter<CompA>();
		var writeB  = ecs.GetCompFull<CompB>();
		var readB   = ecs.GetCompGetter<CompB>();
		var filter1 = ecs.GetFilter<BlahFilter<CompA>>();
		var filter2 = ecs.GetFilter<BlahFilter<CompA, CompB>>();

		var ent1 = ecs.CreateEnt();
		writeA.Add(ent1).Val  = 1;
		writeB.Add(ent1).Val  = 11;
		var ent2 = ecs.CreateEnt();
		writeA.Add(ent2).Val = 2;
		writeB.Add(ent2).Val = 22;
		var ent3 = ecs.CreateEnt();
		writeA.Add(ent3).Val  = 3;
		writeB.Add(ent3).Val  = 33;

		var expected = new List<int> { 11, 22, 33};

		var iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			if (readA.Get(e1).Val == 2)
			{
				expected.Remove(22);
				ecs.DestroyEnt(e1);
			}

			var expectedInIter = new List<int>(expected);
			foreach (var e2 in filter2)
				Assert.IsTrue(expectedInIter.Remove(readB.Get(e2).Val));
			if (expectedInIter.Count > 0)
				Assert.Fail();

			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		expected = new List<int>() { 1, 3 };
		foreach (var e1 in filter1)
			Assert.IsTrue(expected.Remove(readA.Get(e1).Val));
	}

	[Test]
	public void Test_DestroyEntity_SameFilterUpdated()
	{
		var ecs    = new BlahEcs();
		var write  = ecs.GetCompFull<CompA>();
		var read   = ecs.GetCompGetter<CompA>();
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		for (var i = 0; i < 20; i++)
		{
			Debug.Log($"iter {i}");

			write.Add(ecs.CreateEnt()).Val = 1;
            
			foreach (var ent in filter)
				if (read.Get(ent).Val == 5)
					ecs.DestroyEnt(ent);

			foreach (var ent in filter)
				read.Get(ent).Val += 1;
		}
	}

	[Test]
	public void Test_DestroyAllEntitiesThenCreate_SameFilterUpdated()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();
		
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		for (var i = 0; i < 10; i++)
			write.Add(ecs.CreateEnt()).Val = i;

		foreach (var ent in filter)
			ecs.DestroyEnt(ent);

		for (var i = 10; i < 20; i++)
			write.Add(ecs.CreateEnt()).Val = i;

		int iterationsCount = 0;
		var expected        = Enumerable.Range(10, 10).ToList();
		foreach (var ent in filter)
		{
			Assert.IsTrue(expected.Remove(read.Get(ent).Val));
			iterationsCount += 1;
		}
		Assert.AreEqual(10, iterationsCount);
		
		foreach (int exp in expected)
			Assert.Fail($"{exp} left");
	}


	[Test]
	public void Test_GetAny()
	{
		var ecs    = new BlahEcs();
		var read   = ecs.GetCompGetter<CompA>();
		var write  = ecs.GetCompFull<CompA>();
		var filter = ecs.GetFilter<BlahFilter<CompA>>();

		if (filter.TryGetAny(out var ent))
			Assert.Fail();

		var ent1 = ecs.CreateEnt();
        write.Add(ent1).Val = 3;
        
        Assert.AreEqual(3, read.Get(filter.GetAny()).Val);
        Assert.IsTrue(filter.TryGetAny(out var ent2));
        Assert.AreEqual(3, read.Get(ent2).Val);
        
        read.Remove(ent1);
        Assert.Throws<Exception>(() => filter.GetAny());
	}


	private struct CompA : IBlahEntryComp
	{
		public int Val;
	}

	private struct CompB : IBlahEntryComp
	{
		public int Val;
	}

	private struct CompC : IBlahEntryComp
	{
		public int Val;
	}
}
}
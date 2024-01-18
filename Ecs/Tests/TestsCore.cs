using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Blah.Ecs.Tests
{
internal class TestsCore
{
	[Test]
	public void Test_AddComp_ValueSame([NUnit.Framework.Range(1, 10)] int count)
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetWrite<CompA>();
		var read  = ecs.GetRead<CompA>();

		var entities = new BlahEcsEntity[count];
		for (var i = 0; i < count; i++)
		{
			entities[i] = ecs.CreateEntity();

			write.Add(entities[i]).Val = i + 1;
			
			for (var j = 0; j <= i; j++)
				Assert.AreEqual(j + 1, read.Get(entities[j]).Val);
		}
	}

	[Test]
	public void Test_ReAddComp_NoThrow()
	{
		var ecs    = new BlahEcs();
		var write  = ecs.GetWrite<CompA>();
		var filter = GetFilter(ecs, new[] { typeof(CompA) }, null);

		var ent = ecs.CreateEntity();

		write.Add(ent);
		write.Remove(ent);
		write.Add(ent);
		write.Remove(ent);
	}

	
	
	[Test]
	public void Test_SameCompsInFilters_FiltersSame()
	{
		var ecs   = new BlahEcs();
		var filter1 = GetFilter(ecs, new[] { typeof(CompA), typeof(CompB) }, null);
		var filter2 = GetFilter(ecs, new[] { typeof(CompA), typeof(CompB) }, null);
		var filter3 = GetFilter(ecs, new[] { typeof(CompB), typeof(CompA) }, null);
		
		Assert.AreEqual(filter1, filter2);
		Assert.AreEqual(filter1, filter3);
		Assert.AreEqual(filter2, filter3);

		var filter4 = GetFilter(ecs, new[] { typeof(CompA), typeof(CompB), typeof(CompC) }, null);
		var filter5 = GetFilter(ecs, new[] { typeof(CompA) }, new[] { typeof(CompB) });
		Assert.AreNotEqual(filter1, filter4);
		Assert.AreNotEqual(filter1, filter5);
		
		var filter6 = GetFilter(ecs, new[] { typeof(CompA) }, new[] { typeof(CompB), typeof(CompC) });
		var filter7 = GetFilter(ecs, new[] { typeof(CompA) }, new[] { typeof(CompC), typeof(CompB) });
		Assert.AreEqual(filter6, filter7);

		var filter8 = GetFilter(ecs, new[] { typeof(CompB) }, new[] { typeof(CompA), typeof(CompC) });
		Assert.AreNotEqual(filter7, filter8);
	}
	
	
	[Test]
	public void Test_EmptyFilters()
	{
		var ecs  = new BlahEcs();
		var filter = GetFilter(ecs, new[] { typeof(CompA) }, null);
		
		foreach (var e in filter)
			Assert.Fail();
	}
	

	[Test]
	public void Test_AddComp_FilterUpdated([NUnit.Framework.Range(1, 10)] int count)
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetWrite<CompA>();
		var read  = ecs.GetRead<CompA>();
		
		var filter = GetFilter(ecs, new[] { typeof(CompA) }, null);

		for (var i = 0; i < count; i++)
		{
			var ent = ecs.CreateEntity();
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
		var write = ecs.GetWrite<CompA>();
		var read  = ecs.GetRead<CompA>();
		
		var filter = GetFilter(ecs, new[] { typeof(CompA) }, null);

		var ent1 = ecs.CreateEntity();
		var ent2 = ecs.CreateEntity();
		var ent3 = ecs.CreateEntity();

		write.Add(ent1).Val   = 1;
		write.Add(ent2).Val   = 2;
		write.Add(ent3).Val   = 3;

		var expected = new List<int> { 1, 2, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(read.Get(e).Val));
        Assert.Zero(expected.Count);

        write.Remove(ent2);
        
        expected = new List<int> { 1, 3 };
        foreach (var e in filter)
	        Assert.IsTrue(expected.Remove(read.Get(e).Val));
        Assert.Zero(expected.Count);
	}
	

	[Test]
	public void Test_SwitchCompAToB_FiltersUpdated()
	{
		var ecs     = new BlahEcs();
		var writeA  = ecs.GetWrite<CompA>();
		var writeB  = ecs.GetWrite<CompB>();
		var filterA = GetFilter(ecs, new[] {typeof(CompA) }, null);
		var filterB = GetFilter(ecs, new[] {typeof(CompB) }, null);

		foreach (var e in filterA)
			Assert.Fail();
		foreach (var e in filterB)
			Assert.Fail();

		var ent = ecs.CreateEntity();
		writeA.Add(ent);

		var iterationsCount = 0;
		foreach (var e in filterA)
			iterationsCount += 1;
		Assert.AreEqual(1, iterationsCount);
		foreach (var e in filterB)
			Assert.Fail();
        
		writeA.Remove(ent);
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
		var writeA = ecs.GetWrite<CompA>();
		var readA  = ecs.GetRead<CompA>();
		var writeB = ecs.GetWrite<CompB>();
		var readB  = ecs.GetRead<CompB>();
		
		var filterA = GetFilter(ecs, new[] {typeof(CompA) }, null);
		var filterB = GetFilter(ecs, new[] {typeof(CompB) }, null);

		var ent = ecs.CreateEntity();
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
		var write  = ecs.GetWrite<CompA>();
		var read   = ecs.GetRead<CompA>();
		var filter = GetFilter(ecs, new[] {typeof(CompA) }, null);

		var ent = ecs.CreateEntity();
		write.Add(ent).Val = 2;

		foreach (var e in filter)
			Assert.AreEqual(2, read.Get(e).Val);
        
		write.Remove(ent);
		foreach (var e in filter)
			Assert.Fail();

		write.Add(ent);
		var iterationsCount = 0;
		foreach (var e in filter)
		{
			Assert.AreEqual(2, read.Get(ent).Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_DelayedCreateFilter_FilterUpdated()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetWrite<CompA>();
		var read  = ecs.GetRead<CompA>();

		var ent = ecs.CreateEntity();
		write.Add(ent).Val   = 2;
        
		var filterA = GetFilter(ecs, new[] {typeof(CompA) }, null);
		var filterB = GetFilter(ecs, new[] {typeof(CompB) }, null);

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
		var write  = ecs.GetWrite<CompA>();
		var read   = ecs.GetRead<CompA>();
		var filter = GetFilter(ecs, new[] {typeof(CompA) }, null);

		write.Add(ecs.CreateEntity()).Val = 1;
		write.Add(ecs.CreateEntity()).Val = 2;
		write.Add(ecs.CreateEntity()).Val = 3;

		int iterationsCount = 0;
		foreach (var e in filter)
		{
			if (read.Get(e).Val == 2)
			{
				write.Remove(e);
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
		var writeA  = ecs.GetWrite<CompA>();
		var readA   = ecs.GetRead<CompA>();
		var writeB  = ecs.GetWrite<CompB>();
		var readB   = ecs.GetRead<CompB>();
		var filter1 = GetFilter(ecs, new[] {typeof(CompA) }, null);
		var filter2 = GetFilter(ecs, new[] {typeof(CompA), typeof(CompB) }, null);

		var ent1 = ecs.CreateEntity();
		writeA.Add(ent1).Val  = 1;
		writeB.Add(ent1).Val  = 11;
		var ent2 = ecs.CreateEntity();
		writeA.Add(ent2).Val = 2;
		writeB.Add(ent2).Val = 22;
		var ent3 = ecs.CreateEntity();
		writeA.Add(ent3).Val  = 3;
		writeB.Add(ent3).Val  = 33;

		var expected = new List<int> { 11, 22, 33};

		var iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			if (readA.Get(e1).Val == 2)
			{
				expected.Remove(22);
				ecs.DestroyEntity(e1);
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
		var write  = ecs.GetWrite<CompA>();
		var read   = ecs.GetRead<CompA>();
		var filter = GetFilter(ecs, new[] {typeof(CompA) }, null);

		for (var i = 0; i < 20; i++)
		{
			Debug.Log($"iter {i}");

			write.Add(ecs.CreateEntity()).Val = 1;
            
			foreach (var ent in filter)
				if (read.Get(ent).Val == 5)
					ecs.DestroyEntity(ent);

			foreach (var ent in filter)
				read.Get(ent).Val += 1;
		}
	}

	[Test]
	public void Test_DestroyAllEntitiesThenCreate_SameFilterUpdated()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetWrite<CompA>();
		var read  = ecs.GetRead<CompA>();
		
		var filter = GetFilter(ecs, new[] {typeof(CompA) }, null);

		for (var i = 0; i < 10; i++)
			write.Add(ecs.CreateEntity()).Val = i;

		foreach (var ent in filter)
			ecs.DestroyEntity(ent);

		for (var i = 10; i < 20; i++)
			write.Add(ecs.CreateEntity()).Val = i;

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



	private BlahEcsFilter GetFilter(BlahEcs ecs, Type[] inc, Type[] exc)
	{
		var core   = ecs.GetFilterCore(inc, exc);
		var filter = new BlahEcsFilter();
		filter.Set(core);
		return filter;
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
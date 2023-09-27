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
		var ecs = new BlahEcs();

		var entities = new BlahEcsEntity[count];
		for (var i = 0; i < count; i++)
		{
			entities[i] = ecs.CreateEntity();
			
			entities[i].Add<CompA>().Val = i + 1;
			
			for (var j = 0; j <= i; j++)
				Assert.AreEqual(j + 1, entities[j].Get<CompA>().Val);
		}
	}

	[Test]
	public void Test_ReAddComp_NoThrow()
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent = ecs.CreateEntity();

		ent.Add<CompA>();
		ent.Remove<CompA>();
		ent.Add<CompA>();
		ent.Remove<CompA>();
	}

	
	
	[Test]
	public void Test_SameCompsInFilters_FiltersSame()
	{
		var ecs   = new BlahEcs();
		var filter1 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);
		var filter2 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);
		var filter3 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB), typeof(CompA) }, null);
		
		Assert.IsTrue(filter1.IsSame(filter2));
		Assert.IsTrue(filter1.IsSame(filter3));
		Assert.IsTrue(filter2.IsSame(filter3));

		var filter4 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB), typeof(CompC) }, null);
		var filter5 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompB) });
		Assert.IsFalse(filter1.IsSame(filter4));
		Assert.IsFalse(filter1.IsSame(filter5));
		
		var filter6 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompB), typeof(CompC) });
		var filter7 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompC), typeof(CompB) });
		Assert.IsTrue(filter6.IsSame(filter7));

		var filter8 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, new[] { typeof(CompA), typeof(CompC) });
		Assert.IsFalse(filter7.IsSame(filter8));
	}
	
	
	[Test]
	public void Test_EmptyFilters()
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		
		foreach (var e in filter)
			Assert.Fail();
	}
	

	[Test]
	public void Test_AddComp_FilterUpdated([NUnit.Framework.Range(1, 10)] int count)
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		for (var i = 0; i < count; i++)
		{
			var ent = ecs.CreateEntity();
			ent.Add<CompA>().Val = i + 1;

			var expected = Enumerable.Range(1, i + 1).ToList();
			foreach (var e in filter)
				Assert.IsTrue(expected.Remove(e.Get<CompA>().Val));
			
			foreach (int val in expected)
				Assert.Fail($"{val} left");
		}
	}

	[Test]
	public void Test_MultipleEntitiesRemoveCompOfOne_FilterUpdated()
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent1 = ecs.CreateEntity();
		var ent2 = ecs.CreateEntity();
		var ent3 = ecs.CreateEntity();
		
		ent1.Add<CompA>().Val = 1;
		ent2.Add<CompA>().Val = 2;
		ent3.Add<CompA>().Val = 3;

		var expected = new List<int> { 1, 2, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(e.Get<CompA>().Val));
        Assert.Zero(expected.Count);
        
        ent2.Remove<CompA>();
        
        expected = new List<int> { 1, 3 };
        foreach (var e in filter)
	        Assert.IsTrue(expected.Remove(e.Get<CompA>().Val));
        Assert.Zero(expected.Count);
	}
	

	[Test]
	public void Test_SwitchCompAToB_FiltersUpdated()
	{
		var ecs   = new BlahEcs();
		var filterA = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

		foreach (var e in filterA)
			Assert.Fail();
		foreach (var e in filterB)
			Assert.Fail();

		var ent = ecs.CreateEntity();
		ent.Add<CompA>();

		var iterationsCount = 0;
		foreach (var e in filterA)
			iterationsCount += 1;
		Assert.AreEqual(1, iterationsCount);
		foreach (var e in filterB)
			Assert.Fail();

		ent.Remove<CompA>();
		ent.Add<CompB>();

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
		var ecs   = new BlahEcs();
		var filterA = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

		var ent = ecs.CreateEntity();
		ent.Add<CompA>().Val = 2;
		ent.Add<CompB>().Val = 3;

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, e.Get<CompA>().Val);
			Assert.AreEqual(3, e.Get<CompB>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		iterationsCount = 0;
		foreach (var e in filterB)
		{
			Assert.AreEqual(2, e.Get<CompA>().Val);
			Assert.AreEqual(3, e.Get<CompB>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_AddCompThenRemove_FilterUpdated()
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent = ecs.CreateEntity();
		ent.Add<CompA>().Val = 2;

		foreach (var e in filter)
			Assert.AreEqual(2, ent.Get<CompA>().Val);
		
		ent.Remove<CompA>();
		foreach (var e in filter)
			Assert.Fail();
        
		ent.Add<CompA>().Val = 2;
		var iterationsCount = 0;
		foreach (var e in filter)
		{
			Assert.AreEqual(2, ent.Get<CompA>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
	}

	[Test]
	public void Test_DelayedCreateFilter_FilterUpdated()
	{
		var ecs  = new BlahEcs();

		var ent = ecs.CreateEntity();
		ent.Add<CompA>().Val = 2;
        
		var filterA = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

		var iterationsCount = 0;
		foreach (var e in filterA)
		{
			Assert.AreEqual(2, e.Get<CompA>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		foreach (var e in filterB)
			Assert.Fail();
	}

	[Test]
	public void Test_RemoveCompDuringIteration_FilterUpdated()
	{
		var ecs  = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		ecs.CreateEntity().Add<CompA>().Val = 1;
		ecs.CreateEntity().Add<CompA>().Val = 2;
		ecs.CreateEntity().Add<CompA>().Val = 3;

		int iterationsCount = 0;
		foreach (var e in filter)
		{
			if (e.Get<CompA>().Val == 2)
			{
				e.Remove<CompA>();
				Assert.IsFalse(e.Has<CompA>());
			}
			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		var expected = new List<int> { 1, 3 };
		foreach (var e in filter)
			Assert.IsTrue(expected.Remove(e.Get<CompA>().Val));
	}

	[Test]
	public void Test_DestroyEntity_FiltersUpdated()
	{
		var ecs   = new BlahEcs();
		var filter1 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filter2 = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);

		var ent1 = ecs.CreateEntity();
		ent1.Add<CompA>().Val = 1;
		ent1.Add<CompB>().Val = 11;
		var ent2 = ecs.CreateEntity();
		ent2.Add<CompA>().Val = 2;
		ent2.Add<CompB>().Val = 22;
		var ent3 = ecs.CreateEntity();
		ent3.Add<CompA>().Val = 3;
		ent3.Add<CompB>().Val = 33;

		var expected = new List<int> { 11, 22, 33};

		var iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			if (e1.Get<CompA>().Val == 2)
			{
				expected.Remove(22);
				e1.Destroy();
			}

			var expectedInIter = new List<int>(expected);
			foreach (var e2 in filter2)
				Assert.IsTrue(expectedInIter.Remove(e2.Get<CompB>().Val));
			if (expectedInIter.Count > 0)
				Assert.Fail();

			iterationsCount += 1;
		}
		Assert.AreEqual(3, iterationsCount);

		expected = new List<int>() { 1, 3 };
		foreach (var e1 in filter1)
			Assert.IsTrue(expected.Remove(e1.Get<CompA>().Val));
	}

	[Test]
	public void Test_DestroyEntity_SameFilterUpdated()
	{
		var ecs    = new BlahEcs();
		var filter = ecs.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		for (var i = 0; i < 20; i++)
		{
			Debug.Log($"iter {i}");
			
			ecs.CreateEntity().Add<CompA>().Val = 1;
            
			foreach (var ent in filter)
				if (ent.Get<CompA>().Val == 5)
					ent.Destroy();

			foreach (var ent in filter)
				ent.Get<CompA>().Val += 1;
		}
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
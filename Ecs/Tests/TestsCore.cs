using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsCore
{
	[Test]
	public void Test_AddComp_ValueSame([Range(1, 10)] int count)
	{
		var world = new BlahEcsWorld();

		var entities = new BlahEcsEntity[count];
		for (var i = 0; i < count; i++)
		{
			entities[i] = world.CreateEntity();
			
			entities[i].Add<CompA>().Val = i + 1;
			
			for (var j = 0; j <= i; j++)
				Assert.AreEqual(j + 1, entities[j].Get<CompA>().Val);
		}
	}

	[Test]
	public void Test_ReAddComp_NoThrow()
	{
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent = world.CreateEntity();

		ent.Add<CompA>();
		ent.Remove<CompA>();
		ent.Add<CompA>();
		ent.Remove<CompA>();
	}

	
	
	[Test]
	public void Test_SameCompsInFilters_FiltersSame()
	{
		var world   = new BlahEcsWorld();
		var filter1 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);
		var filter2 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);
		var filter3 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB), typeof(CompA) }, null);
		
		Assert.IsTrue(filter1.IsSame(filter2));
		Assert.IsTrue(filter1.IsSame(filter3));
		Assert.IsTrue(filter2.IsSame(filter3));

		var filter4 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB), typeof(CompC) }, null);
		var filter5 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompB) });
		Assert.IsFalse(filter1.IsSame(filter4));
		Assert.IsFalse(filter1.IsSame(filter5));
		
		var filter6 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompB), typeof(CompC) });
		var filter7 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, new[] { typeof(CompC), typeof(CompB) });
		Assert.IsTrue(filter6.IsSame(filter7));

		var filter8 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, new[] { typeof(CompA), typeof(CompC) });
		Assert.IsFalse(filter7.IsSame(filter8));
	}
	
	
	[Test]
	public void Test_EmptyFilters()
	{
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		
		foreach (var e in filter)
			Assert.Fail();
	}
	

	[Test]
	public void Test_AddComp_FilterUpdated([Range(1, 10)] int count)
	{
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		for (var i = 0; i < count; i++)
		{
			var ent = world.CreateEntity();
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
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent1 = world.CreateEntity();
		var ent2 = world.CreateEntity();
		var ent3 = world.CreateEntity();
		
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
		var world   = new BlahEcsWorld();
		var filterA = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

		foreach (var e in filterA)
			Assert.Fail();
		foreach (var e in filterB)
			Assert.Fail();

		var ent = world.CreateEntity();
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
		var world   = new BlahEcsWorld();
		var filterA = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

		var ent = world.CreateEntity();
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
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		var ent = world.CreateEntity();
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
		var world  = new BlahEcsWorld();

		var ent = world.CreateEntity();
		ent.Add<CompA>().Val = 2;
        
		var filterA = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filterB = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompB) }, null);

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
		var world  = new BlahEcsWorld();
		var filter = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);

		world.CreateEntity().Add<CompA>().Val = 1;
		world.CreateEntity().Add<CompA>().Val = 2;
		world.CreateEntity().Add<CompA>().Val = 3;

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
		var world   = new BlahEcsWorld();
		var filter1 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA) }, null);
		var filter2 = world.GetFilter<BlahEcsFilterProxy>(new[] { typeof(CompA), typeof(CompB) }, null);

		var ent = world.CreateEntity();
		ent.Add<CompA>().Val = 1;
		ent.Add<CompB>().Val = 2;

		var iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			e1.Destroy();

			foreach (var e2 in filter2)
				Assert.Fail();

			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		foreach (var e1 in filter1)
			Assert.Fail();

		ent = world.CreateEntity();
		ent.Add<CompA>().Val = 3;
		ent.Add<CompB>().Val = 4;

		iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			Assert.AreEqual(3, e1.Get<CompA>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
		
		iterationsCount = 0;
		foreach (var e1 in filter1)
		{
			Assert.AreEqual(3, e1.Get<CompA>().Val);
			Assert.AreEqual(4, e1.Get<CompB>().Val);
			iterationsCount += 1;
		}
		Assert.AreEqual(1, iterationsCount);
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
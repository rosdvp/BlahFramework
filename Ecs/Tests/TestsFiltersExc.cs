using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Blah.Ecs.Tests
{
internal class TestsFiltersExc
{
	[Test]
	public void Test_EntHasExcComp_NotInFilter()
	{
		var ecs    = new BlahEcs();
		var writeA = ecs.GetWrite<CompA>();
		var writeB = ecs.GetWrite<CompB>();

		var filter = GetFilter(ecs, new[] { typeof(CompA) }, new[] { typeof(CompB) });

		var ents = new List<BlahEcsEntity>();
		ents.Add(ecs.CreateEntity());
		ents.Add(ecs.CreateEntity());
		ents.Add(ecs.CreateEntity());
		var temp = new List<BlahEcsEntity>();

		foreach (var ent in ents)
			writeA.Add(ent);

		temp.AddRange(ents);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);

		writeB.Add(ents[1]);

		temp.Add(ents[0]);
		temp.Add(ents[2]);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);
		
		writeB.Remove(ents[1]);
		
		temp.AddRange(ents);
		foreach (var ent in filter)
			Assert.IsTrue(temp.Remove(ent));
		Assert.IsTrue(temp.Count == 0);
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
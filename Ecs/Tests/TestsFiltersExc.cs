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
		var writeA = ecs.GetCompFull<CompA>();
		var writeB = ecs.GetCompFull<CompB>();

		var filter = ecs.GetFilter<BlahFilter<CompA>.Exc<CompB>>();

		var ents = new List<BlahEnt>();
		ents.Add(ecs.CreateEnt());
		ents.Add(ecs.CreateEnt());
		ents.Add(ecs.CreateEnt());
		var temp = new List<BlahEnt>();

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
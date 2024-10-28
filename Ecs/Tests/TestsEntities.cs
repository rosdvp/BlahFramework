using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsEntities
{
	[Test]
	public void Test_CreateEntities_EntitiesAlive()
	{
		var entsCount = 10;
		
		var ecs = new BlahEcs();
		
		var aliveEnts = new List<BlahEnt>();
		var deadEnts  = new List<BlahEnt>();

		for (var iter = 0; iter < entsCount; iter++)
		{
			for (var i = 0; i < entsCount; i++)
			{
				aliveEnts.Add(ecs.CreateEnt());
				Assert.IsTrue(ecs.IsEntAlive(aliveEnts[i]), $"iter {iter}, i {i}");
			}

			foreach (var ent in aliveEnts)
				Assert.IsTrue(ecs.IsEntAlive(ent), $"iter {iter}");

			for (var i = 0; i < entsCount; i++)
			{
				var ent = aliveEnts[^1];
				ecs.DestroyEnt(ent);
				aliveEnts.RemoveAt(aliveEnts.Count-1);
				deadEnts.Add(ent);
				
				foreach (var aliveEnt in aliveEnts)
					Assert.IsTrue(ecs.IsEntAlive(aliveEnt), $"iter {iter}, {i}");
				foreach (var deadEnt in deadEnts)
					Assert.IsFalse(ecs.IsEntAlive(deadEnt), $"iter {iter}, {i}");
			}
		}
	}
}
}
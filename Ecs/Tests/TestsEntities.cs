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
		
		var aliveEnts = new List<BlahEcsEntity>();
		var deadEnts  = new List<BlahEcsEntity>();

		for (var iter = 0; iter < entsCount; iter++)
		{
			for (var i = 0; i < entsCount; i++)
			{
				aliveEnts.Add(ecs.CreateEntity());
				Assert.IsTrue(ecs.IsEntityAlive(aliveEnts[i]), $"iter {iter}, i {i}");
			}

			foreach (var ent in aliveEnts)
				Assert.IsTrue(ecs.IsEntityAlive(ent), $"iter {iter}");

			for (var i = 0; i < entsCount; i++)
			{
				var ent = aliveEnts[^1];
				ecs.DestroyEntity(ent);
				aliveEnts.RemoveAt(aliveEnts.Count-1);
				deadEnts.Add(ent);
				
				foreach (var aliveEnt in aliveEnts)
					Assert.IsTrue(ecs.IsEntityAlive(aliveEnt), $"iter {iter}, {i}");
				foreach (var deadEnt in deadEnts)
					Assert.IsFalse(ecs.IsEntityAlive(deadEnt), $"iter {iter}, {i}");
			}
		}
	}
}
}
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsComps
{
	[Test]
	public void Test_AddComp_ValueSame([Range(1, 10)] int count)
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
		var read   = ecs.GetRead<CompA>();

		var ent = ecs.CreateEntity();

		write.Add(ent);
		read.Remove(ent);
		write.Add(ent);
		read.Remove(ent);
	}
	
	
	private struct CompA : IBlahEntryEcs
	{
		public int Val;
	}
}
}
using NUnit.Framework;

namespace Blah.Ecs.Tests
{
internal class TestsComps
{
	[Test]
	public void Test_AddComp_ValueSame([Range(1, 10)] int count)
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();

		var entities = new BlahEnt[count];
		for (var i = 0; i < count; i++)
		{
			entities[i] = ecs.CreateEnt();

			write.Add(entities[i]).Val = i + 1;
			
			for (var j = 0; j <= i; j++)
				Assert.AreEqual(j + 1, read.Get(entities[j]).Val);
		}
	}

	[Test]
	public void Test_ReAddComp_NoThrow()
	{
		var ecs    = new BlahEcs();
		var write  = ecs.GetCompFull<CompA>();
		var read   = ecs.GetCompGetter<CompA>();

		var ent = ecs.CreateEnt();

		write.Add(ent);
		read.Remove(ent);
		write.Add(ent);
		read.Remove(ent);
	}

	[Test]
	public void Test_DestroyEnt1AddCompEnt2_Ent1HasNoComp()
	{
		var ecs   = new BlahEcs();
		var write = ecs.GetCompFull<CompA>();
		var read  = ecs.GetCompGetter<CompA>();

		var ent1 = ecs.CreateEnt();
        
		Assert.IsFalse(read.Has(ent1));
		
		write.Add(ent1).Val = 3;

		Assert.IsTrue(read.Has(ent1));
		
		ecs.DestroyEnt(ent1);
		var ent2 = ecs.CreateEnt();
		
		Assert.IsFalse(read.Has(ent1));
		Assert.IsFalse(read.Has(ent2));

		write.Add(ent2).Val = 4;
		Assert.IsFalse(read.Has(ent1));
		Assert.IsTrue(read.Has(ent2));
	}
	
	
	private struct CompA : IBlahEntryComp
	{
		public int Val;
	}
}
}
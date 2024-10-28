using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemoveThenAddDuringIteration
{
	[Test]
	public void Test()
	{
		var context = new BlahPoolsContext();
		var read    = context.GetSignalRead<MockSignalEntry>();
		var write   = context.GetSignalWrite<MockSignalEntry>();

		for (var i = 0; i < 10; i++)
		{
			for (var j = 0; j < 5; j++)
			{
				if (i != 0) 
					context.OnNextFrame();
				AssertHelper.CheckContent(read);
			}

			write.Add().Val = 1;
			write.Add().Val = 2;
			AssertHelper.CheckContent(read, 1, 2);

			var iterCount = 0;
			foreach (ref var ev in read)
			{
				if (ev.Val != 1 && ev.Val != 2)
					Assert.Fail();
				if (ev.Val == 1)
				{
					read.Remove();
					write.Add().Val = 3;
				}
				iterCount++;
			}
			Assert.AreEqual(2, iterCount);
            
			AssertHelper.CheckContent(read, 2, 3);
		}
		AssertHelper.CheckPoolLength(read, 4); //1 2 4
	}
}
}
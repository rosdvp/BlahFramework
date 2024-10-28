using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemoveThenAdd
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
			write.Add().Val = 3;

			foreach (ref var ev in read)
			{
				if (ev.Val == 2)
					read.Remove();
			}

			write.Add().Val = 4;
			write.Add().Val = 5;

			AssertHelper.CheckContent(read, 1, 3, 4, 5);
		}
		AssertHelper.CheckPoolLength(read, 4); //1 2 4
	}
}
}
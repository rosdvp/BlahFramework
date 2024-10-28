using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalSort
{
	[Test]
	public void Test()
	{
		var context  = new BlahPoolsContext();
		var consumer = context.GetSignalRead<MockSignalEntry>();
		var producer = context.GetSignalWrite<MockSignalEntry>();

		int[] values     = { 3, 1, 2, 3, 2 };
		int[] ascValues  = { 1, 2, 2, 3, 3 };
		int[] descValues = { 3, 3, 2, 2, 1 };

		producer.Add().Val = values[0];
		producer.Add().Val = values[1];
		producer.Add().Val = values[2];
		producer.Add().Val = values[3];
		producer.Add().Val = values[4];

		var i = 0;
		foreach (ref var ev in consumer)
		{
			Assert.AreEqual(values[i], ev.Val);
			i++;
		}

		consumer.Sort((a, b) => a.Val.CompareTo(b.Val));
		i = 0;
		foreach (ref var ev in consumer)
		{
			Assert.AreEqual(ascValues[i], ev.Val);
			i++;
		}

		consumer.Sort((a, b) => b.Val.CompareTo(a.Val));
		i = 0;
		foreach (ref var ev in consumer)
		{
			Assert.AreEqual(descValues[i], ev.Val);
			i++;
		}
	}
}
}
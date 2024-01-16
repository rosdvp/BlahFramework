using NUnit.Framework;

namespace Blah.Pools.Tests.SoloSignal
{
internal class TestSoloSignal
{
	[Test]
	public void Test()
	{
		var context  = new BlahPoolsContext();
		var consumer = context.GetSoloSignalConsumer<SoloSignal>();
		var producer = context.GetSoloSignalProducer<SoloSignal>();

		for (var i = 0; i < 2; i++)
		{
			Assert.IsTrue(consumer.IsEmpty);
			Assert.Catch(() => consumer.Get());

			producer.Add().Val = 5;
			Assert.IsTrue(consumer.IsExists);
			Assert.AreEqual(5, consumer.Get().Val);
			Assert.AreEqual(5, consumer.Get().Val);
			Assert.Catch(() => producer.Add());
			
			consumer.Remove();
			Assert.IsTrue(consumer.IsEmpty);
			Assert.Catch(() => consumer.Get());

			producer.Add().Val = 10;
			Assert.IsTrue(consumer.IsExists);
			Assert.AreEqual(10, consumer.Get().Val);
			Assert.Catch(() => producer.Add());
			
			context.OnNextFrame();
		}
	}


	private struct SoloSignal : IBlahEntrySoloSignal
	{
		public int Val;
	}
}
}
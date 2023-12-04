using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemoveThenAddDuringIteration
{
    [Test]
    public void Test()
    {
        var context  = new BlahPoolsContext();
        var consumer = context.GetSignalConsumer<MockSignalEntry>();
        var producer = context.GetSignalProducer<MockSignalEntry>();

        for (var i = 0; i < 10; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                if (i != 0) 
                    context.OnNextFrame();
                AssertHelper.CheckContent(consumer);
            }

            producer.Add().Val = 1;
            producer.Add().Val = 2;
            AssertHelper.CheckContent(consumer, 1, 2);

            var iterCount = 0;
            foreach (ref var ev in consumer)
            {
                if (ev.Val != 1 && ev.Val != 2)
                    Assert.Fail();
                if (ev.Val == 1)
                {
                    consumer.Remove();
                    producer.Add().Val = 3;
                }
                iterCount++;
            }
            Assert.AreEqual(2, iterCount);
            
            AssertHelper.CheckContent(consumer, 2, 3);
        }
        AssertHelper.CheckPoolLength(consumer, 4); //1 2 4
    }
}
}

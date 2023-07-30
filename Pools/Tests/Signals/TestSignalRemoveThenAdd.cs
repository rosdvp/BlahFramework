using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemoveThenAdd
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
                    context.ToNextFrame();
                AssertHelper.CheckContent(consumer);
            }

            producer.Add().Val = 1;
            producer.Add().Val = 2;
            producer.Add().Val = 3;

            foreach (ref var ev in consumer)
            {
                if (ev.Val == 2)
                    consumer.Remove();
            }

            producer.Add().Val   = 4;
            producer.Add().Val   = 5;

            AssertHelper.CheckContent(consumer, 1, 3, 4, 5);
        }
        AssertHelper.CheckPoolLength(consumer, 4); //1 2 4
    }
}
}

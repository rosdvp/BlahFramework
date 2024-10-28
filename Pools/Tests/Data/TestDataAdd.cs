using NUnit.Framework;

namespace Blah.Pools.Tests.Data
{
internal class TestDataAdd
{
    [Test]
    public void Test()
    {
        var context  = new BlahPoolsContext();
        var producer = context.GetDataFull<MockDataEntry>();
        var consumer = context.GetDataGetter<MockDataEntry>();
        
        for (var i = 0; i < 10; i++)
        {
            producer.Add().Val = 1;
            producer.Add().Val = 2;

            for (var j = 0; j < 5; j++)
            {
                AssertHelper.CheckContent(consumer, 1, 2);
                context.OnNextFrame();
            }
            foreach (ref var entry in consumer)
                consumer.Remove();
            AssertHelper.CheckContent(consumer);
        }
        AssertHelper.CheckPoolLength(consumer, 2);
    }
}
}

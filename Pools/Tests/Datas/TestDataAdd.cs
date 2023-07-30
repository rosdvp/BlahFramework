using NUnit.Framework;

namespace Blah.Pools.Tests.Datas
{
internal class TestDataAdd
{
    [Test]
    public void Test()
    {
        var context  = new BlahPoolsContext();
        var producer = context.GetDataProducer<MockDataEntry>();
        var consumer = context.GetDataConsumer<MockDataEntry>();
        
        for (var i = 0; i < 10; i++)
        {
            producer.Add().Val = 1;
            producer.Add().Val = 2;

            for (var j = 0; j < 5; j++)
            {
                AssertHelper.CheckContent(consumer, 1, 2);
                context.ToNextFrame();
            }
            consumer.RemoveAll();
            AssertHelper.CheckContent(consumer);
        }
        AssertHelper.CheckPoolLength(consumer, 2);
    }
}
}

using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemoveOnNextFrame
{
    [Test]
    public void TestRemoveAll()
    {
        var context  = new BlahPoolsContext();
        var consumer = context.GetSignalRead<MockSignalEntry>();
        var producer = context.GetSignalWrite<MockSignalEntry>();
        
        for (var i = 0; i < 10; i++)
        {
            producer.Add().Val = 1;
            producer.Add().Val = 2;
            producer.Add().Val = 3;
            AssertHelper.CheckContent(consumer, 1, 2, 3);

            context.OnNextFrame();
            
            AssertHelper.CheckContent(consumer);
        }
        
        AssertHelper.CheckPoolLength(consumer, 4); //1 2 4
    }
}
}

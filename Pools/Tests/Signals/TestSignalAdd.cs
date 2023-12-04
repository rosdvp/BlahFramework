using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalAdd
{
    [TestCase(2, 1)]
    [TestCase(2, 1, 2)]
    [TestCase(4, 1, 2, 3)]
    [TestCase(4, 1, 2, 3, 4)]
    [TestCase(8, 1, 2, 3, 4, 5)]
    public void Test(int expectedPoolLength, params int[] expectedValues)
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

            foreach (int val in expectedValues)
                producer.Add().Val = val;

            AssertHelper.CheckContent(consumer, expectedValues);
        }
        AssertHelper.CheckPoolLength(consumer, expectedPoolLength);
    }
}
}

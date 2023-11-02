using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalRemove
{
    [TestCaseSource(nameof(GetTestCases))]
    public void TestRemove(TestCaseData data)
    {
        int[] values         = data.Values;
        int[] valuesToRemove = data.ValuesToRemove;
            
        var context  = new BlahPoolsContext();
        var consumer = context.GetSignalConsumer<MockSignalEntry>();
        var producer = context.GetSignalProducer<MockSignalEntry>();

        for (var i = 0; i < 3; i++)
        {
            foreach (int val in values)
                producer.Add().Val = val;

            var iterCount = 0;
            foreach (ref var ev in consumer)
            {
                if (Array.IndexOf(valuesToRemove, ev.Val) != -1)
                    consumer.Remove();
                iterCount++;
            }
            Assert.AreEqual(values.Length, iterCount);

            var expectedValues = new List<int>(values);
            foreach (int val in valuesToRemove)
                expectedValues.Remove(val);
            AssertHelper.CheckContent(consumer, expectedValues.ToArray());
            
            context.ToNextFrame();
            AssertHelper.CheckContent(consumer);
        }

        int expectedPoolLength = values.Length switch
        {
            1 => 2,
            2 => 2,
            3 => 4,
            4 => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        AssertHelper.CheckPoolLength(consumer, expectedPoolLength);
    }

    public struct TestCaseData
    {
        public int[] Values;
        public int[] ValuesToRemove;

        public override string ToString()
        {
            var str = "values: ";
            foreach (int v in Values)
                str += $"{v}, ";
            str += "to_remove: ";
            foreach (int v in ValuesToRemove)
                str += $"{v}, ";
            return str;
        }
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        foreach (int[] values in AssertHelper.GenLinArray())
        foreach (int[] valuesToRemove in AssertHelper.GenCombArray())
            yield return new TestCaseData { Values = values, ValuesToRemove = valuesToRemove };
    }


    [Test]
    public void TestRemoveWithNextFrame()
    {
        var context  = new BlahPoolsContext();
        var consumer = context.GetSignalNextFrameConsumer<MockSignalNextFrameEntry>();
        var producer = context.GetSignalNextFrameProducer<MockSignalNextFrameEntry>();

        for (var i = 0; i < 10; i++)
        {
            producer.AddNextFrame().Val = 1;
            producer.AddNextFrame().Val = 2;
            context.ToNextFrame();

            producer.AddNextFrame().Val = 3;

            AssertHelper.CheckContent(consumer, 1, 2);

            foreach (ref var ev in consumer)
                if (ev.Val == 1)
                    consumer.Remove();
            AssertHelper.CheckContent(consumer, 2);

            context.ToNextFrame();
            AssertHelper.CheckContent(consumer, 3);

            context.ToNextFrame();
            AssertHelper.CheckContent(consumer);
        }
        AssertHelper.CheckPoolLength(consumer, 4); // 1 2 4
    }
}
}

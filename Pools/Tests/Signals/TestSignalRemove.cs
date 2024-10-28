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
            
        var context = new BlahPoolsContext();
        var read    = context.GetSignalRead<MockSignalEntry>();
        var write   = context.GetSignalWrite<MockSignalEntry>();

        for (var i = 0; i < 3; i++)
        {
            foreach (int val in values)
                write.Add().Val = val;

            var iterCount = 0;
            foreach (ref var ev in read)
            {
                if (Array.IndexOf(valuesToRemove, ev.Val) != -1)
                    read.Remove();
                iterCount++;
            }
            Assert.AreEqual(values.Length, iterCount);

            var expectedValues = new List<int>(values);
            foreach (int val in valuesToRemove)
                expectedValues.Remove(val);
            AssertHelper.CheckContent(read, expectedValues.ToArray());
            
            context.OnNextFrame();
            AssertHelper.CheckContent(read);
        }

        int expectedPoolLength = values.Length switch
        {
            1 => 2,
            2 => 2,
            3 => 4,
            4 => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        AssertHelper.CheckPoolLength(read, expectedPoolLength);
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
        var context = new BlahPoolsContext();
        var read    = context.GetNfSignalRead<MockSignalNextFrameEntry>();
        var write   = context.GetNfSignalWrite<MockSignalNextFrameEntry>();

        for (var i = 0; i < 10; i++)
        {
            write.AddNf().Val = 1;
            write.AddNf().Val = 2;
            context.OnNextFrame();

            write.AddNf().Val = 3;

            AssertHelper.CheckContent(read, 1, 2);

            foreach (ref var ev in read)
                if (ev.Val == 1)
                    read.Remove();
            AssertHelper.CheckContent(read, 2);

            context.OnNextFrame();
            AssertHelper.CheckContent(read, 3);

            context.OnNextFrame();
            AssertHelper.CheckContent(read);
        }
        AssertHelper.CheckPoolLength(read, 4); // 1 2 4
    }
}
}
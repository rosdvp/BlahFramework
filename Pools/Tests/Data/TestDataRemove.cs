﻿using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Pools.Tests.Data
{
internal class TestDataRemove
{
    [TestCaseSource(nameof(GetTestCases))]
    public void TestRemove(TestCaseData data)
    {
        int[] values         = data.Values;
        int[] valuesToRemove = data.ValuesToRemove;
            
        var context = new BlahPoolsContext();
        var datas   = context.GetDataFull<MockDataEntry>();

        for (var i = 0; i < 3; i++)
        {
            foreach (int val in values)
                datas.Add().Val = val;

            var iterCount = 0;
            foreach (ref var ev in datas)
            {
                if (Array.IndexOf(valuesToRemove, ev.Val) != -1)
                    datas.Remove();
                iterCount++;
            }
            Assert.AreEqual(values.Length, iterCount);

            var expectedValues = new List<int>(values);
            foreach (int val in valuesToRemove)
                expectedValues.Remove(val);
            AssertHelper.CheckContent(datas, expectedValues.ToArray());
            
            datas.RemoveAll();
            AssertHelper.CheckContent(datas);
        }

        int expectedPoolLength = values.Length switch
        {
            1 => 2,
            2 => 2,
            3 => 4,
            4 => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        AssertHelper.CheckPoolLength(datas, expectedPoolLength);
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
}
}

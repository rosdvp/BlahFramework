using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalNextFrame
{
    [TestCaseSource(nameof(GetTestCases))]
    public void Test(TestCaseData data)
    {
        int[] frame1 = data.Frame1;
        int[] frame2 = data.Frame2;
        int[] frame3 = data.Frame3;
        
        var context  = new BlahPoolsContext();
        var producer = context.GetSignalNextFrameProducer<MockSignalNextFrameEntry>();

        for (var iter = 0; iter < 3; iter++)
        {
            foreach (int val in frame1)
                producer.AddNextFrame().Val = val;
            AssertHelper.CheckContent(producer);
            context.ToNextFrame();
            AssertHelper.CheckContent(producer, frame1);

            foreach (int val in frame2)
                producer.AddNextFrame().Val = val;
            AssertHelper.CheckContent(producer, frame1);
            context.ToNextFrame();
            AssertHelper.CheckContent(producer, frame2);

            foreach (int val in frame3)
                producer.AddNextFrame().Val = val;
            AssertHelper.CheckContent(producer, frame2);
            context.ToNextFrame();
            AssertHelper.CheckContent(producer, frame3);

            context.ToNextFrame();
            AssertHelper.CheckContent(producer);
        }
    }


    public static IEnumerable<TestCaseData> GetTestCases()
    {
        //only next frame
        yield return new TestCaseData { Frame1 = new[] { 1 }, Frame2 = new int[] { }, Frame3 = new[] { 3 } };
        yield return new TestCaseData { Frame1 = new[] { 1, 2 }, Frame2 = new int[] { }, Frame3 = new[] { 3, 4 } };
        yield return new TestCaseData { Frame1 = new[] { 1, 2, 3 }, Frame2 = new int[] { }, Frame3 = new[] { 4 } };
        yield return new TestCaseData { Frame1 = new[] { 1, 2 }, Frame2 = new int[] { }, Frame3 = new[] { 3, 4, 5 } };
        //next frame and curr frame
        yield return new TestCaseData { Frame1 = new[] { 1 }, Frame2    = new[] { 2 }, Frame3    = new[] { 3 } };
        yield return new TestCaseData { Frame1 = new[] { 1, 2 }, Frame2 = new[] { 3 }, Frame3    = new[] { 4, 5 } };
        yield return new TestCaseData { Frame1 = new[] { 1 }, Frame2    = new[] { 2, 3 }, Frame3 = new[] { 4, 5, 6 } };
        yield return new TestCaseData
            { Frame1 = new[] { 1, 2, 3 }, Frame2 = new[] { 4, 5, 6, 7 }, Frame3 = new[] { 8 } };
    }


    public struct TestCaseData
    {
        public int[] Frame1;
        public int[] Frame2;
        public int[] Frame3;

        public override string ToString()
        {
            var str = "";
            foreach (int v in Frame1)
                str += $"{v} ";
            str += "| ";

            foreach (int v in Frame2)
                str += $"{v}, ";
            str += "| ";

            foreach (int v in Frame3)
                str += $"{v}, ";

            return str;
        }
    }
}
}

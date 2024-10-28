using NUnit.Framework;

namespace Blah.Pools.Tests.Data
{
internal class TestDataRemoveThenAddDuringIteration
{
    [Test]
    public void Test()
    {
        var context = new BlahPoolsContext();
        var datas   = context.GetDataFull<MockDataEntry>();

        for (var i = 0; i < 10; i++)
        {
            datas.RemoveAll();
            AssertHelper.CheckContent(datas);

            datas.Add().Val = 1;
            datas.Add().Val = 2;
            AssertHelper.CheckContent(datas, 1, 2);

            var iterCount = 0;
            foreach (ref var ev in datas)
            {
                if (ev.Val != 1 && ev.Val != 2)
                    Assert.Fail();
                if (ev.Val == 1)
                {
                    datas.Remove();
                    datas.Add().Val = 3;
                }
                iterCount++;
            }
            Assert.AreEqual(2, iterCount);
            
            AssertHelper.CheckContent(datas, 2, 3);
        }
        AssertHelper.CheckPoolLength(datas, 4); //1 2 4
    }
}
}

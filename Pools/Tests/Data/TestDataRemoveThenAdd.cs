using NUnit.Framework;

namespace Blah.Pools.Tests.Data
{
internal class TestDataRemoveThenAdd
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
            datas.Add().Val = 3;

            foreach (ref var ev in datas)
            {
                if (ev.Val == 2)
                    datas.Remove();
            }

            datas.Add().Val   = 4;
            datas.Add().Val   = 5;

            AssertHelper.CheckContent(datas, 1, 3, 4, 5);
        }
        AssertHelper.CheckPoolLength(datas, 4); //1 2 4
    }
}
}

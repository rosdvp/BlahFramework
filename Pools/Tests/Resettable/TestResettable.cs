using NUnit.Framework;

namespace Blah.Pools.Tests.Resettable
{
internal class TestResettable
{
    private struct TestStruct
    {
        public int X;
    }

    private class TestClass
    {
        public int Y;
    }
    
    private struct ResettableSignal : IBlahEntrySignal, IBlahEntryAutoReset
    {
        public int        IntVal;
        public string     StrVal;
        public TestStruct StructVal;
        public TestClass  ClassVal;
    }
    
    [Test]
    public void Test()
    {
        var context  = new BlahPoolsContext();
        var producer = context.GetSignalProducer<ResettableSignal>();
        var consumer = context.GetSignalConsumer<ResettableSignal>();

        ref var evSent = ref producer.Add();
        evSent.IntVal      = 1;
        evSent.StrVal      = "123";
        evSent.StructVal.X = 2;
        evSent.ClassVal    = new TestClass { Y = 3 };
        
        
        foreach (ref var ev in consumer)
        {
            Assert.AreEqual(1, ev.IntVal);
            Assert.AreEqual("123", ev.StrVal);
            Assert.AreEqual(2, ev.StructVal.X);
            Assert.AreEqual(3, ev.ClassVal.Y);
        }

        context.ToNextFrame();
        Assert.AreEqual(true, consumer.IsEmpty);

        producer.Add();

        foreach (ref var ev in consumer)
        {
            Assert.AreEqual(0, ev.IntVal);
            Assert.AreEqual(null, ev.StrVal);
            Assert.AreEqual(0, ev.StructVal.X);
            Assert.AreEqual(null, ev.ClassVal);
        }
    }
}
}

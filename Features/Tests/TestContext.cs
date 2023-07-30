using NUnit.Framework;

namespace Blah.Features.Tests
{
internal class TestContext
{
	[Test]
	public void Test_Context()
	{
		var context = new MockContext();
		context.Init(null, null);
		context.RequestSwitchSystemsGroup(0, false);

		var producer = context.Pools.GetSignalProducer<MockCmdA>();
		producer.Add().Val = 25;

		context.Run();

		var consumer = context.Pools.GetDataConsumer<MockDataB>();
		Assert.AreEqual(1, consumer.Count);
		foreach (var data in consumer)
			Assert.AreEqual(25, data.Val);
	}
}
}
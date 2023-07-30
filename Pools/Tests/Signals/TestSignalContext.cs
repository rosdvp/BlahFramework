using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalContext
{
	[Test]
	public void Test()
	{
		var context = new BlahPoolsContext();

		var consumer = context.GetSignalConsumer<MockSignalEntry>();
		var producer = context.GetSignalProducer<MockSignalEntry>();
		
		Assert.AreSame(producer, consumer);
	}
}
}
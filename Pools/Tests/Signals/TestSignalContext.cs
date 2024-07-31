using NUnit.Framework;

namespace Blah.Pools.Tests.Signals
{
internal class TestSignalContext
{
	[Test]
	public void Test()
	{
		var context = new BlahPoolsContext();

		var consumer = context.GetSignalRead<MockSignalEntry>();
		var producer = context.GetSignalWrite<MockSignalEntry>();
		
		Assert.AreSame(producer, consumer);
	}
}
}
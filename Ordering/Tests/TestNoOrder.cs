using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestNoOrder
{
	[Test]
	public void Test_NoOrder([Range(0, 10)] int offset)
	{
		var systems = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		AssertHelper.Shift(systems, offset);
		var expected = systems.ToArray();
		BlahOrderer.Order(ref systems);
		
		foreach (var exp in expected)
			if (!systems.Remove(exp))
				Assert.Fail($"{exp} is not found");
		if (systems.Count > 0)
			Assert.Fail($"{systems[0]} should not present");
	}


	private class SystemA { }
	private class SystemB { }
	private class SystemC { }
}
}
using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestAttributeAfter
{
	[Test]
	public void Test_AllAttributed([Range(0, 10)] int offset)
	{
		var systems = new List<Type>
		{
			typeof(SystemC),
			typeof(SystemB),
			typeof(SystemA)
		};
		AssertHelper.Shift(systems, offset);
		var expected = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};

		BlahOrderer.Order(ref systems);

		AssertHelper.AssertEqual(expected, systems);
	}

	[Test]
	public void Test_SomeAttributed([Range(0, 10)] int offset)
	{
		var systems = new List<Type>
		{
			typeof(SystemC),
			typeof(SystemD),
			typeof(SystemA),
			typeof(SystemE),
			typeof(SystemB),
		};
		AssertHelper.Shift(systems, offset);
		var expectedOrder = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		
		BlahOrderer.Order(ref systems);
		
		AssertHelper.AssertOrder(expectedOrder, systems);
	}


	private class SystemA { }
	
	[BlahAfter(typeof(SystemA))]
	private class SystemB { }

	[BlahAfter(typeof(SystemB))]
	private class SystemC { }

	private class SystemD { }

	private class SystemE { }
}
}
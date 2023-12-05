using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestAttributesAfterAll
{
	[Test]
	public void Test()
	{
		var systems = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemE),
			typeof(SystemB),
			typeof(SystemD),
			typeof(SystemC),
		};
		var expected = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
			typeof(SystemD),
			typeof(SystemE),
		};

		BlahOrderer.Order(ref systems);

		AssertHelper.AssertEqual(expected, systems);
	}
	
	private class SystemA { }
	
	[BlahAfter(typeof(SystemA))]
	private class SystemB { }

	[BlahAfter(typeof(SystemB))]
	private class SystemC { }

	[BlahAfterAll]
	private class SystemD { }

	[BlahAfterAll(1)]
	private class SystemE { }
}
}
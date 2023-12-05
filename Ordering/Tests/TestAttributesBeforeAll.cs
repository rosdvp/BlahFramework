using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestAttributesBeforeAll
{
	[Test]
	public void Test()
	{
		var systems = new List<Type>
		{
			typeof(SystemC),
			typeof(SystemB),
			typeof(SystemD),
			typeof(SystemA),
			typeof(SystemE),
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
	
	[BlahBeforeAll(1)]
	private class SystemA { }
	
	[BlahBeforeAll]
	private class SystemB { }

	private class SystemC { }

	[BlahAfter(typeof(SystemC))]
	private class SystemD { }

	[BlahAfter(typeof(SystemD))]
	private class SystemE { }
}
}
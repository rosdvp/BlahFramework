using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestAttributeMixed
{
	[Test]
	public void Test_AllAttributed()
	{
		var systems = new List<Type>
		{
			typeof(SystemD),
			typeof(SystemA),
			typeof(SystemC),
			typeof(SystemB),
		};
		var expected = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
			typeof(SystemD)
		};

		BlahOrderer.Order(ref systems);

		AssertHelper.AssertEqual(expected, systems);
	}

	[Test]
	public void Test_SomeAttributed()
	{
		var systems = new List<Type>
		{
			typeof(SystemC),
			typeof(SystemD),
			typeof(SystemE),
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemF)
		};
		var expectedOrder = new[]
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
			typeof(SystemD)
		};

		BlahOrderer.Order(ref systems);

		AssertHelper.AssertOrder(expectedOrder, systems);
	}


	[BlahBefore(typeof(SystemB))]
	private class SystemA { }

	private class SystemB { }

	[BlahAfter(typeof(SystemB))]
	[BlahBefore(typeof(SystemD))]
	private class SystemC { }

	private class SystemD { }

	private class SystemE { }

	private class SystemF { }
}
}
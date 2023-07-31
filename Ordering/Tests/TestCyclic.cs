using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestCyclic
{
	[Test]
	public void Test_Cyclic_Throws()
	{
		var systems = new List<Type>()
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC)
		};
		try
		{
			BlahOrderer.Order(ref systems);
			Assert.Fail();
		}
		catch (BlahOrdererSortingException e)
		{
			Assert.Pass(e.Message);
		}
	}


	[BlahAfter(typeof(SystemC))]
	private class SystemA { }

	[BlahAfter(typeof(SystemA))]
	private class SystemB { }

	[BlahAfter(typeof(SystemB))]
	private class SystemC { }

	[BlahAfter(typeof(SystemC))]
	private class SystemD { }

	[BlahAfter(typeof(SystemD))]
	private class SystemE { }
}
}
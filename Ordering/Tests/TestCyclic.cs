using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Pools;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal class TestCyclic
{
	[Test]
	public void Test_Cyclic_Throws([Range(0, 10)] int offset)
	{
		var systems = new List<Type>()
		{
			typeof(SystemA1),
			typeof(SystemA2),
			typeof(SystemA3),
			typeof(SystemA4),
			typeof(SystemA5)
		};
		AssertHelper.Shift(systems, offset);
		try
		{
			BlahOrderer.Order(ref systems);
			Assert.Fail();
		}
		catch (BlahOrdererSortingException e)
		{
			Assert.Pass(e.GetFullMsg());
		}
	}

	[Test]
	public void Test_CyclicSelf_Throw()
	{
		var systems = new List<Type>
		{
			typeof(SystemB1),
			typeof(SystemB2),
		};
		try
		{
			BlahOrderer.Order(ref systems);
			Assert.Fail();
		}
		catch (BlahOrdererSortingException e)
		{
			Assert.Pass(e.GetFullMsg());
		}
	}
	

	[BlahAfter(typeof(SystemA4))]
	private class SystemA1 { }

	[BlahAfter(typeof(SystemA1))]
	private class SystemA2 { }

	[BlahAfter(typeof(SystemA2))]
	private class SystemA3 { }

	[BlahAfter(typeof(SystemA3))]
	private class SystemA4 { }

	[BlahAfter(typeof(SystemA4))]
	private class SystemA5 { }


	[BlahAfter(typeof(SystemB1))]
	private class SystemB1 { }
	
	[BlahAfter(typeof(SystemB1))]
	private class SystemB2 { }
}
}
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
public class TestAssertHelper
{
	[Test]
	public void Test_AssertEqual_True()
	{
		var expected = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
		};
		var actual = expected.ToArray();

		AssertHelper.AssertEqual(expected, actual);
	}

	[Test]
	public void Test_AssertEqual_False()
	{
		var expected = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
		};
		var actual = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemC),
			typeof(SystemB),
		};

		Assert.Throws<AssertionException>(() => AssertHelper.AssertEqual(expected, actual));
	}

	[Test]
	public void Test_AssertOrder_True()
	{
		var expected = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
		};
		var actual = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemD),
			typeof(SystemB),
			typeof(SystemE),
			typeof(SystemC),
		};
		AssertHelper.AssertOrder(expected, actual);
	}

	[Test]
	public void Test_AssertOrder_False()
	{
		var expected = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemB),
			typeof(SystemC),
		};
		var actual = new List<Type>
		{
			typeof(SystemA),
			typeof(SystemD),
			typeof(SystemC),
			typeof(SystemE),
			typeof(SystemB),
		};

		Assert.Throws<AssertionException>(() => AssertHelper.AssertOrder(expected, actual));
	}




	private class SystemA { }

	private class SystemB { }

	private class SystemC { }

	private class SystemD { }

	private class SystemE { }
}
}
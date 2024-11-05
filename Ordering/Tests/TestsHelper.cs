using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blah.Ordering.Tests
{
internal static class AssertHelper
{
	public static void Shift(List<Type> list, int offset)
	{
		for (var offs = 0; offs < offset; offs++)
		{
			var first = list[0];
			for (var i = 0; i < list.Count - 1; i++)
				list[i] = list[i + 1];
			list[^1] = first;
		}
	}
	
	public static void AssertEqual(IReadOnlyList<Type> expected, IReadOnlyList<Type> actual)
	{
		string errorStr = BuildErrorStr(expected, actual);
		
		Assert.AreEqual(expected.Count, actual.Count, $"different count.{errorStr}");

		for (var i = 0; i < expected.Count; i++)
			Assert.AreEqual(expected[i], actual[i], $"idx {i}.{errorStr}");
	}

	public static void AssertOrder(IReadOnlyList<Type> expectedOrder, IReadOnlyList<Type> actualOrder)
	{
		var expIdx = 0;
		var actIdx = 0;

		while (expIdx < expectedOrder.Count && actIdx < actualOrder.Count)
		{
			if (expectedOrder[expIdx] == actualOrder[actIdx])
				expIdx += 1;
			actIdx++;
		}

		if (expIdx < expectedOrder.Count)
			Assert.Fail($"expected {expectedOrder[expIdx]} is not found." +
			            $"{BuildErrorStr(expectedOrder, actualOrder)}");
	}

	private static string BuildErrorStr(IReadOnlyList<Type> expected, IReadOnlyList<Type> actual)
	{
		var str = "\nexpected:";
		foreach (var exp in expected)
			str += "\n" + exp.Name;
		str += "\nactual:";
		foreach (var act in actual)
			str += "\n" + act.Name;
		return str;
	}
}
}
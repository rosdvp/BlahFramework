using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Blah.Pools.Tests
{
internal static class AssertHelper
{
	public static void CheckPoolLength<T>(T target, int expectedLength)
	{
		var type = target.GetType().BaseType;
		var fieldSet = type.GetField("Entries",
		                             BindingFlags.NonPublic | BindingFlags.Instance
		);
		object set = fieldSet.GetValue(target);
		var fieldEntries = fieldSet.FieldType.GetField("_entries",
		                                              BindingFlags.NonPublic | BindingFlags.Instance
		);
		int actualLength = ((Array)fieldEntries.GetValue(set)).Length;
		Assert.AreEqual(expectedLength, actualLength);
	}

	public static void CheckContent<T>(IBlahSignalRead<T> pool, params int[] expectedValues) 
		where T: struct, IBlahEntrySignal, IMockEntry 
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
	public static void CheckContent<T>(IBlahSignalWrite<T> pool, params int[] expectedValues) 
		where T: struct, IBlahEntrySignal, IMockEntry 
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
	
	public static void CheckContent<T>(IBlahNfSignalRead<T> pool, params int[] expectedValues) 
		where T: struct, IBlahEntryNfSignal, IMockEntry 
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
	public static void CheckContent<T>(IBlahNfSignalWrite<T> pool, params int[] expectedValues) 
		where T: struct, IBlahEntryNfSignal, IMockEntry 
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
    
	public static void CheckContent<T>(IBlahDataGet<T> pool, params int[] expectedValues)
		where T : struct, IBlahEntryData, IMockEntry
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
	public static void CheckContent<T>(IBlahDataFull<T> pool, params int[] expectedValues)
		where T : struct, IBlahEntryData, IMockEntry
	{
		CheckContent((BlahPool<T>)pool, expectedValues);
	}
	

	private static void CheckContent<T>(BlahPool<T> pool, params int[] expectedValues) 
		where T: struct, IMockEntry
	{
		var values = new List<int>(expectedValues);
		
		Assert.AreEqual(expectedValues.Length == 0, pool.IsEmpty, "isEmpty check");
		Assert.AreEqual(expectedValues.Length, pool.Count, "count check");
		var iterCount = 0;
		foreach (ref var ev in pool)
		{
			if (!values.Contains(ev.Value))
				Assert.Fail($"{ev.Value} is not expected");
			values.Remove(ev.Value);
			iterCount++;
		}
		Assert.AreEqual(expectedValues.Length, iterCount, "iterations count check");
		foreach (int value in values)
			Assert.Fail($"{value} is not in the pool");
	}
	
	
	public static IEnumerable<int[]> GenLinArray()
	{
		yield return new[] { 1 };
		yield return new[] { 1, 2 };
		yield return new[] { 1, 2, 3 };
		yield return new[] { 1, 2, 3, 4 };
	}

	public static IEnumerable<int[]> GenCombArray()
	{
		var values = new[] { 1, 2, 3, 4 };

		return Enumerable.Range(0, 1 << values.Length)
		                 .Select(index => values
		                                  .Where((v, i) => (index & (1 << i)) != 0)
		                                  .ToArray()
		                 );
	}
}
}
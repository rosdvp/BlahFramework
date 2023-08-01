using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blah.Ordering
{
internal static class BlahOrdererTopologicalSort
{
	public static List<Type> Sort(
		List<Type>                   items,
		Dictionary<Type, List<Type>> sourceItemToPrevItems)
	{
		var visited = new List<Type>();
		var result  = new List<Type>();
		
		var itemToPrevItems = Copy(sourceItemToPrevItems);

		var overflowCounter = 0;

		while (items.Count > 0 || visited.Count > 0)
		{
			if (visited.Count == 0)
			{
				visited.Add(PopLast(items));
			}
			if (itemToPrevItems.TryGetValue(visited[^1], out var prevItems) &&
			    prevItems.Count > 0 &&
			    items.Remove(prevItems[^1]))
			{
				visited.Add(PopLast(prevItems));
			}
			else
			{
				result.Add(PopLast(visited));
			}

			if (++overflowCounter >= 1000000)
				throw new Exception("overflow");
		}

		ThrowOnCyclicDependency(result, Copy(sourceItemToPrevItems));

		return result;
	}


	private static void ThrowOnCyclicDependency(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToPrevItems.TryGetValue(item, out var prevItems))
				foreach (var prevItem in prevItems)
					if (i < items.IndexOf(prevItem))
					{
						var cycle = RecFindCycle(item, null, itemToPrevItems);
						throw new BlahOrdererSortingException(cycle);
					}
		}
	}

	private static List<Type> RecFindCycle(
		Type startItem,
		Type currItem,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		if (startItem == currItem)
			return new List<Type> { currItem };
		
		currItem ??= startItem;
		
		Debug.Log(currItem);
		
		if (itemToPrevItems.TryGetValue(currItem, out var prevItems))
			foreach (var prevItem in prevItems)
			{
				var cycle = RecFindCycle(startItem, prevItem, itemToPrevItems);
				if (cycle != null)
				{
					cycle.Add(currItem);
					return cycle;
				}
			}

		return null;
	}


	private static Dictionary<Type, List<Type>> Copy(Dictionary<Type, List<Type>> source)
	{
		var result = new Dictionary<Type, List<Type>>();
		foreach (var pair in source)
			result[pair.Key] = new List<Type>(pair.Value);
		return result;
	}

	private static Type PopLast(List<Type> list)
	{
		var item = list[^1];
		list.RemoveAt(list.Count-1);
		return item;
	}
}

public class BlahOrdererSortingException : Exception
{
	public readonly IReadOnlyList<Type> Cycle;

	internal BlahOrdererSortingException(List<Type> cycle)
		: base($"cyclic dependency, use {nameof(GetFullMsg)} for more info")
	{
		Cycle = cycle;
	}

	public string GetFullMsg()
	{
		var s = "cyclic dependency: ";
		if (Cycle == null)
			s += "failed to identify";
		else
			foreach (var item in Cycle)
				s += $"-> {item.Name} ";
		return s;
	}
}
}
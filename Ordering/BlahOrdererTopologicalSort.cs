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
		foreach (var (item, prevItems) in itemToPrevItems)
			if (prevItems.Contains(item))
				throw new BlahOrdererSortingException(item, null);
		
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToPrevItems.TryGetValue(item, out var prevItems))
				foreach (var prevItem in prevItems)
					if (i < items.IndexOf(prevItem))
					{
						var cycle = FindCycle(items, itemToPrevItems);
						throw new BlahOrdererSortingException(null, cycle);
					}
		}
	}

	private static List<Type> FindCycle(List<Type> items, Dictionary<Type, List<Type>> itemToPrevItems)
	{
		var visitedItems = new HashSet<Type>();
		foreach (var item in items)
		{
			visitedItems.Clear();
			var result = RecFindCycle(item, visitedItems, itemToPrevItems);
			if (result != null)
				return result;
		}
		return null;
	}

	private static List<Type> RecFindCycle(
		Type currItem,
		HashSet<Type> visitedItems,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		if (visitedItems.Contains(currItem))
			return new List<Type> { currItem };

		visitedItems.Add(currItem);
		if (itemToPrevItems.TryGetValue(currItem, out var prevItems))
			foreach (var prevItem in prevItems)
			{
				var cycle = RecFindCycle(prevItem, visitedItems, itemToPrevItems);
				if (cycle != null)
				{
					cycle.Add(currItem);
					return cycle;
				}
			}
		visitedItems.Remove(currItem);

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
	public readonly Type                SelfCyclicType;
	public readonly IReadOnlyList<Type> Cycle;

	internal BlahOrdererSortingException(Type selfCyclicType, List<Type> cycle)
		: base($"cyclic dependency, use {nameof(GetFullMsg)} for more info")
	{
		SelfCyclicType = selfCyclicType;
		Cycle          = cycle;
	}

	public string GetFullMsg()
	{
		var s = "cyclic dependency: ";
		if (SelfCyclicType != null)
			s = $"{SelfCyclicType.Name} self cyclic";
		else if (Cycle != null)
			foreach (var item in Cycle)
				s += $"-> {item.Name} ";
		else
			s += "failed to identify";
		return s;
	}
}
}
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
		//var visited = new List<Type>();
		//var result  = new List<Type>();
		//
		//var itemToPrevItems = Copy(sourceItemToPrevItems);
		//
		//var overflowCounter = 0;
		//
		//while (items.Count > 0 || visited.Count > 0)
		//{
		//	if (visited.Count == 0)
		//	{
		//		visited.Add(PopLast(items));
		//	}
		//	if (itemToPrevItems.TryGetValue(visited[^1], out var prevItems) &&
		//	    prevItems.Count > 0 &&
		//	    items.Remove(prevItems[^1]))
		//	{
		//		visited.Add(PopLast(prevItems));
		//	}
		//	else
		//	{
		//		result.Add(PopLast(visited));
		//	}
		//
		//	if (++overflowCounter >= 1000000)
		//		throw new Exception("overflow");
		//}

		var result = Sort(items,
		                  item =>
		                  {
			                  if (sourceItemToPrevItems.TryGetValue(item, out var prevItems))
				                  return prevItems;
			                  return null;
		                  }
		);
		if (result == null)
		{
			ThrowOnCyclicDependency(items, Copy(sourceItemToPrevItems));
			throw new Exception("undefined");
		}

		return result;
	}
	
	private static List<T> Sort<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies)
	{
		var sorted  = new List<T>();
		var visited = new Dictionary<T, bool>();

		foreach (var item in source)
		{
			if (!Visit(item, getDependencies, sorted, visited))
				return null;
		}

		return sorted;
	}

	private static bool Visit<T>(T       item,   Func<T, IEnumerable<T>> getDependencies, 
	                            List<T> sorted, Dictionary<T, bool>     visited)
	{
		bool inProcess;
		var  alreadyVisited = visited.TryGetValue(item, out inProcess);

		if (alreadyVisited)
		{
			if (inProcess)
			{
				return false;
			}
		}
		else
		{
			visited[item] = true;

			var dependencies = getDependencies(item);
			if (dependencies != null)
			{
				foreach (var dependency in dependencies)
				{
					if (!Visit(dependency, getDependencies, sorted, visited))
						return false;
				}
			}

			visited[item] = false;
			sorted.Add(item);
		}
		return true;
	}
	

	private static void ThrowOnCyclicDependency(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		foreach (var (item, prevItems) in itemToPrevItems)
			if (prevItems.Contains(item))
				throw new BlahOrdererSortingException(
					item,
					null,
					null,
					null,
					null
				);

		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToPrevItems.TryGetValue(item, out var prevItems))
				foreach (var prevItem in prevItems)
					if (i < items.IndexOf(prevItem))
					{
						var cycle = FindCycle(items, itemToPrevItems);
						throw new BlahOrdererSortingException(
							null,
							cycle,
							prevItem,
							item,
							items
						);
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
	public readonly Type                SelfCyclicItem;
	public readonly IReadOnlyList<Type> Cycle;
	public readonly Type                IssuePrevItem;
	public readonly Type                IssueNextItem;
	public readonly List<Type>          IssueOrder;

	internal BlahOrdererSortingException(
		Type       selfCyclicItem,
		List<Type> cycle,
		Type       issuePrevItem,
		Type       issueNextItem,
		List<Type> issueOrder)
		: base($"cyclic dependency, use {nameof(GetFullMsg)} for more info")
	{
		SelfCyclicItem = selfCyclicItem;
		Cycle          = cycle;
		IssuePrevItem     = issuePrevItem;
		IssueNextItem     = issueNextItem;
		IssueOrder     = issueOrder;
	}

	public string GetFullMsg()
	{
		var s = "cyclic dependency: ";
		if (SelfCyclicItem != null)
			s = $"{SelfCyclicItem.Name} self cyclic";
		else if (Cycle != null)
			foreach (var item in Cycle)
				s += $"-> {item.Name} ";
		else
		{
			s += $"failed to identify, but {IssuePrevItem} must go before {IssueNextItem}.\nResulted order:";
			foreach (var item in IssueOrder)
				s += $"{item.Name}, \n";
		}
		return s;
	}
}
}
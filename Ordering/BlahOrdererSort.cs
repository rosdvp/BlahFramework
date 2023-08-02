using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blah.Ordering
{
internal static class BlahOrdererSort
{
	public static List<Type> Sort(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		var sorted = TopolSort(items, itemToPrevItems);
		if (sorted == null)
		{
			ThrowOnSelfCyclic(itemToPrevItems);
			ThrowOnCyclic(items, itemToPrevItems);
			throw new BlahOrdererSortingException();
		}
		ThrowOnFinalCheck(sorted, itemToPrevItems);
		return sorted;
	}
	
	private static List<Type> TopolSort(List<Type> items, Dictionary<Type, List<Type>> itemToPrevItems)
	{
		var sorted  = new List<Type>();
		var itemToVisitStatus = new Dictionary<Type, bool>();

		foreach (var item in items)
		{
			if (!RecTopolVisit(item, itemToPrevItems, sorted, itemToVisitStatus))
				return null;
		}

		return sorted;
	}

	private static bool RecTopolVisit(Type      item,   Dictionary<Type, List<Type>> itemToPrevItems, 
	                            List<Type> sorted, Dictionary<Type, bool> itemToVisitState)
	{
		bool isVisited = itemToVisitState.TryGetValue(item, out bool isVisiting);
		if (isVisited)
		{
			if (isVisiting)
				return false;
		}
		else
		{
			itemToVisitState[item] = true;

			if (itemToPrevItems.TryGetValue(item, out var prevItems) &&
			    prevItems != null)
			{
				foreach (var prevItem in prevItems)
				{
					if (!RecTopolVisit(prevItem, itemToPrevItems, sorted, itemToVisitState))
						return false;
				}
			}

			itemToVisitState[item] = false;
			sorted.Add(item);
		}
		return true;
	}


	private static void ThrowOnSelfCyclic(Dictionary<Type, List<Type>> itemToPrevItems)
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
	}
	
	private static void ThrowOnCyclic(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToPrevItems)
	{
		var visitedItems = new HashSet<Type>();
		foreach (var item in items)
		{
			visitedItems.Clear();
			var cycle = RecFindCycle(item, visitedItems, itemToPrevItems);
			if (cycle != null)
				throw new BlahOrdererSortingException(
					null,
					cycle,
					null,
					null,
					null
				);
		}
	}
	
	private static List<Type> RecFindCycle(
		Type                         currItem,
		HashSet<Type>                visitedItems,
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

	private static void ThrowOnFinalCheck(List<Type> items, Dictionary<Type, List<Type>> itemToPrevItems)
	{
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToPrevItems.TryGetValue(item, out var prevItems))
				foreach (var prevItem in prevItems)
					if (i < items.IndexOf(prevItem))
					{
						throw new BlahOrdererSortingException(
							null,
							null,
							prevItem,
							item,
							items
						);
					}
		}
	}
}



public class BlahOrdererSortingException : Exception
{
	public readonly Type                SelfCyclicItem;
	public readonly IReadOnlyList<Type> Cycle;
	public readonly Type                IssuePrevItem;
	public readonly Type                IssueNextItem;
	public readonly List<Type>          IssueOrder;

	internal BlahOrdererSortingException() : this(
		null,
		null,
		null,
		null,
		null
	) { }

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
		else if (IssuePrevItem != null && IssueNextItem != null && IssueOrder != null)
		{
			s += $"failed to identify, but {IssuePrevItem} must go before {IssueNextItem}.\nresulted order:";
			foreach (var item in IssueOrder)
				s += $"\n{item.Name},";
		}
		else
			s += "undefined";
		return s;
	}
}
}
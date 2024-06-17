using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blah.Ordering
{
internal static class BlahOrdererTpSort
{
	public static void Sort(
		int                                      systemsPriority,
		ISet<Type>                               items,
		IReadOnlyDictionary<Type, HashSet<Type>> itemToPrevItems,
		Dictionary<Type, bool>                   itemToVisitState,
		List<Type>                               result)
	{
		foreach (var item in items)
			if (!Rec(item))
			{
				ThrowOnCyclic(items, itemToPrevItems, null, systemsPriority);
				throw new BlahOrdererSortingException(systemsPriority: systemsPriority);
			}
		return;

		bool Rec(Type item)
		{
			bool isVisited = itemToVisitState.TryGetValue(item, out bool isVisiting);
			if (isVisited)
			{
				if (isVisiting)
					return false;
				return true;
			}

			itemToVisitState[item] = true;

			if (itemToPrevItems.TryGetValue(item, out var prevItems) &&
			    prevItems != null)
			{
				foreach (var prevItem in prevItems)
					if (!Rec(prevItem))
						return false;
			}

			itemToVisitState[item] = false;
			result.Add(item);
			return true;
		}
	}

	public static void ThrowOnCyclic(
		ICollection<Type>                items,
		IReadOnlyDictionary<Type, HashSet<Type>> itemToPrevItems,
		string                                   errorDesc,
		int?                                     systemsPriority)
	{
		foreach (var (item, prevItems) in itemToPrevItems)
			if (prevItems.Contains(item))
				throw new BlahOrdererSortingException(
					null,
					systemsPriority,
					item,
					null,
					null,
					null,
					null
				);

		var visitedItems = new HashSet<Type>();
		foreach (var item in items)
		{
			visitedItems.Clear();
			var cycle = RecFindCycle(item, visitedItems, itemToPrevItems);
			if (cycle != null)
				throw new BlahOrdererSortingException(
					null,
					systemsPriority,
					null,
					cycle,
					null,
					null,
					null
				);
		}
	}

	private static List<Type> RecFindCycle(
		Type                                     currItem,
		HashSet<Type>                            visitedItems,
		IReadOnlyDictionary<Type, HashSet<Type>> itemToPrevItems)
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

	public static void ThrowOnFinalCheck(
		List<Type>                               items,
		IReadOnlyDictionary<Type, HashSet<Type>> itemToPrevItems)
	{
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToPrevItems.TryGetValue(item, out var prevItems))
				foreach (var prevItem in prevItems)
					if (i < items.IndexOf(prevItem))
					{
						throw new BlahOrdererSortingException(
							"final check",
							0,
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
	public readonly string              Desc;
	public readonly int?                SystemsPriority;
	public readonly Type                SelfCyclicItem;
	public readonly IReadOnlyList<Type> Cycle;
	public readonly Type                IssuePrevItem;
	public readonly Type                IssueNextItem;
	public readonly List<Type>          IssueOrder;

	internal BlahOrdererSortingException(
		string     desc            = null,
		int?       systemsPriority = null,
		Type       selfCyclicItem  = null,
		List<Type> cycle           = null,
		Type       issuePrevItem   = null,
		Type       issueNextItem   = null,
		List<Type> issueOrder      = null)
		: base("failed to order, use editor tool menu to get more info")
	{
		Desc            = desc;
		SystemsPriority = systemsPriority;
		SelfCyclicItem  = selfCyclicItem;
		Cycle           = cycle;
		IssuePrevItem   = issuePrevItem;
		IssueNextItem   = issueNextItem;
		IssueOrder      = issueOrder;
	}

	public string GetFullMsg()
	{
		var s = "failed to order\n";
		s += $"desc: {Desc}\n";
		s += $"systems priority: {SystemsPriority}\n";
		s += "type: ";
		if (SelfCyclicItem != null)
		{
			s += "self-cyclic\n";
			s += SelfCyclicItem.Name;
		}
		else if (Cycle != null)
		{
			s += "cycle\n";
			foreach (var item in Cycle)
				s += $"-> {item.Name} ";
		}
		else if (IssuePrevItem != null && IssueNextItem != null && IssueOrder != null)
		{
			s += "unknown\n";
			s += $"{IssuePrevItem} must go before {IssueNextItem}.\nresulted order:";
			s += "resulted order:\n";
			foreach (var item in IssueOrder)
				s += $"{item.Name},\n";
		}
		else
			s += "unknown";
		return s;
	}
}
}
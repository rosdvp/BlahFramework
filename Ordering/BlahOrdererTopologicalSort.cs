using System;
using System.Collections.Generic;

namespace Blah.Ordering
{
internal static class BlahOrdererTopologicalSort
{
	public static List<Type> Sort(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToItemsGoingBefore)
	{
		var visited = new List<Type>();
		var result  = new List<Type>();

		var overflowCounter = 0;

		while (items.Count > 0 || visited.Count > 0)
		{
			if (visited.Count == 0)
			{
				visited.Add(PopLast(items));
			}
			if (itemToItemsGoingBefore.TryGetValue(visited[^1], out var itemsGoingBefore) &&
			    itemsGoingBefore.Count > 0 &&
			    items.Remove(itemsGoingBefore[^1]))
			{
				visited.Add(PopLast(itemsGoingBefore));
			}
			else
			{
				result.Add(PopLast(visited));
			}

			if (++overflowCounter >= 1000000)
				throw new Exception("overflow");
		}

		ThrowOnCyclicDependency(result, itemToItemsGoingBefore);

		return result;
	}


	private static void ThrowOnCyclicDependency(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToItemsGoingBefore)
	{
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (itemToItemsGoingBefore.TryGetValue(item, out var itemsGoingBefore))
				foreach (var itemGoingBefore in itemsGoingBefore)
					if (i < items.IndexOf(itemGoingBefore))
						throw new BlahOrdererSortingException(item, itemGoingBefore);
		}
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
	public Type ItemType;
	public Type ItemMustGoingBeforeType;

	internal BlahOrdererSortingException(Type itemType, Type itemMustGoingBeforeType)
		: base($"cyclic dependency, {itemMustGoingBeforeType} must go before {itemType}")
	{
		ItemType             = itemType;
		ItemMustGoingBeforeType = itemMustGoingBeforeType;
	}
}
}
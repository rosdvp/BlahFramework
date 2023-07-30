using System;
using System.Collections.Generic;

namespace Blah.Ordering
{
internal static class BlahOrdererTopologicalSort
{
	public static List<Type> Sort(
		List<Type>                   items,
		Dictionary<Type, List<Type>> itemToDepends)
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
			if (itemToDepends.TryGetValue(visited[^1], out var depends) &&
			    depends.Count > 0 &&
			    items.Remove(depends[^1]))
			{
				visited.Add(PopLast(depends));
			}
			else
			{
				result.Add(PopLast(visited));
			}

			if (++overflowCounter >= 1000000)
				throw new Exception("overflow");
		}
		return result;
	}

	private static Type PopLast(List<Type> list)
	{
		var item = list[^1];
		list.RemoveAt(list.Count-1);
		return item;
	}
}
}
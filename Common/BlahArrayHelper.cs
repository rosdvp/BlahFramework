using System;

namespace Blah.Common
{
public static class BlahArrayHelper
{
	public static void ResizeOnDemand<T>(ref T[] array, int demandedIdx)
	{
		if (demandedIdx >= array.Length)
			Array.Resize(ref array, demandedIdx * 2);
	}

	public static void ResizeOnDemand<T>(ref T[] array, int demandedIdx, T defaultVal)
	{
		if (demandedIdx < array.Length)
			return;
		int prevLength = array.Length;
        Array.Resize(ref array, demandedIdx * 2);
        for (int i = prevLength; i < array.Length; i++)
	        array[i] = defaultVal;
	}
}
}
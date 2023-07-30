using System;

namespace Blah.Common
{
public class BlahSet<T>
{
	private T[] _entries;
	private int _entriesCount;

	private int[] _releasedPtrs;
	private int   _releasedCount;
	
	public BlahSet(int baseCapacity, int firstPtr)
	{
		_entries       = new T[baseCapacity + firstPtr];
		_entriesCount  = firstPtr;
		_releasedPtrs  = new int[_entries.Length];
		_releasedCount = 0;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public int Add()
	{
		if (_releasedCount > 0)
		{
			return _releasedPtrs[--_releasedCount];
		}
		if (_entriesCount == _entries.Length)
		{
			int newLength = _entriesCount * 2;
			Array.Resize(ref _entries, newLength);
			Array.Resize(ref _releasedPtrs, newLength);
		}
		return _entriesCount++;
	}

	public ref T Get(int ptr) => ref _entries[ptr];

	public void Remove(int ptr)
	{
		_releasedPtrs[_releasedCount++] = ptr;
	}

	public void RemoveAll()
	{
		_entriesCount  = 0;
		_releasedCount = 0;
	}
}
}
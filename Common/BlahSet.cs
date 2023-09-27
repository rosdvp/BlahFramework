using System;

namespace Blah.Common
{
public class BlahSet<T>
{
	private readonly int _ptrsOffset;
	
	private T[] _entries;
	private int _entriesCount;

	private int[] _releasedPtrs;
	private int   _releasedCount;
	
	public BlahSet(int baseCapacity, int ptrsOffset)
	{
		_ptrsOffset = ptrsOffset;
		
		_entries       = new T[baseCapacity];
		_entriesCount  = 0;
		_releasedPtrs  = new int[baseCapacity];
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
		return _ptrsOffset + _entriesCount++;
	}

	public ref T Get(int ptr) => ref _entries[ptr - _ptrsOffset];

	public void Remove(int ptr)
	{
		_releasedPtrs[_releasedCount++] = ptr - _ptrsOffset;
	}

	public void RemoveAll()
	{
		_entriesCount  = 0;
		_releasedCount = 0;
	}
}
}
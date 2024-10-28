﻿using System;
using Blah.Common;

namespace Blah.Pools
{
public interface IBlahEntryData { }

public interface IBlahDataGet<T> where T : struct, IBlahEntryData
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	/// <returns>Pointer to current data in most nested foreach loop</returns>
	public BlahDataPtr GetPtr(int iteratorLevel = -1);
	/// <returns>True if such data exists.</returns>
	public bool IsPtrValid(BlahDataPtr  ptr);
	/// <returns>True if ptr is not null and such data exists.</returns>
	public bool IsPtrValid(BlahDataPtr? ptr);
	public ref T Get(BlahDataPtr        ptr);

	/// <remarks>
	/// Method exists to simplify using nullable BlahDataPtr.<br/>
	/// It is implied that <see cref="IsPtrValid(Blah.Pools.BlahDataPtr?)"/> is used beforehand.
	/// </remarks>
	/// <exception cref="NullReferenceException">Ptr is null.</exception>
	public ref T Get(BlahDataPtr? ptr);


	public void Remove(int         iteratorLevel = -1);
	public void Remove(BlahDataPtr ptr);
	public void RemoveAll();

	public ref T GetAny();
	public void Sort(Comparison<T> comp);
}

public interface IBlahDataFull<T> : IBlahDataGet<T> where T : struct, IBlahEntryData
{
	public ref T Add();
	public ref T Add(out BlahDataPtr ptr);
}

internal class BlahDataPool<T> :
	BlahPool<T>,
	IBlahDataGet<T>,
	IBlahDataFull<T> where T : struct, IBlahEntryData
{
	private BlahSet<BlahDataPtr> _set = new(1, 0);

	private int[] _entryPtrToSetPtr = new int[1] { -1 };


	public ref T Add(out BlahDataPtr ptr)
	{
		if (GoingIteratorsCount > 0)
			throw new Exception("is not supported during iteration");
		
		ref var entry = ref Add();

		int alivePtr = AliveEntriesPtrs[AliveEntriesCount - 1];
		int setPtr   = _entryPtrToSetPtr[alivePtr];
		ptr = _set.Get(setPtr);

		return ref entry;
	}

	public BlahDataPtr GetPtr(int iteratorLevel)
	{
		if (iteratorLevel == -1)
			iteratorLevel = GoingIteratorsCount - 1;
		else if (iteratorLevel >= GoingIteratorsCount)
			throw new Exception($"current max level is {GoingIteratorsCount - 1}, but {iteratorLevel} passed");

		int cursor   = IteratorCursorByLevel[iteratorLevel];
		int alivePtr = AliveEntriesPtrs[cursor];
		int setPtr   = _entryPtrToSetPtr[alivePtr];
		return _set.Get(setPtr);
	}

	public bool IsPtrValid(BlahDataPtr ptr)
	{
		if (ptr.DataType != typeof(T))
			return false;
		if (ptr.EntryPtr >= _entryPtrToSetPtr.Length)
			return false;
		int setPtr = _entryPtrToSetPtr[ptr.EntryPtr];
		if (setPtr == -1)
			return false;
		if (_set.Get(setPtr).Gen != ptr.Gen)
			return false;
		return true;
	}

	public bool IsPtrValid(BlahDataPtr? ptr)
	{
		return ptr != null && IsPtrValid(ptr.Value);
	}

	public ref T Get(BlahDataPtr ptr)
	{
		if (!IsPtrValid(ptr))
			throw new Exception("ptr is invalid");
		return ref Entries.Get(ptr.EntryPtr);
	}

	public ref T Get(BlahDataPtr? ptr)
	{
		if (ptr == null)
			throw new Exception("ptr is null");
		return ref Get(ptr.Value);
	}

	public void Remove(BlahDataPtr ptr)
	{
		if (IsPtrValid(ptr))
			RemoveSafe(ptr.EntryPtr);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	protected override void OnAliveEntryAdded(int entryPtr)
	{
		int setPtr = _set.Add();

		ref var dataPtr = ref _set.Get(setPtr);
		dataPtr.DataType =  typeof(T);
		dataPtr.EntryPtr =  entryPtr;
		dataPtr.Gen      += 1;

		BlahArrayHelper.ResizeOnDemand(ref _entryPtrToSetPtr, entryPtr, -1);
		_entryPtrToSetPtr[entryPtr] = setPtr;
	}

	protected override void OnAliveEntryRemoved(int entryPtr)
	{
		int setPtr = _entryPtrToSetPtr[entryPtr];
		_set.Remove(setPtr);

		_entryPtrToSetPtr[entryPtr] = -1;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public override void Clear()
	{
		base.Clear();
		_set.Clear();
		for (var i = 0; i < _entryPtrToSetPtr.Length; i++)
			_entryPtrToSetPtr[i] = -1;
	}
}
}
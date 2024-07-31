using System;
using Blah.Common;

namespace Blah.Pools
{
internal interface IBlahPoolInternal
{
    void OnNextFrame();
	void Clear();
}

public class BlahPool<T> : IBlahPoolInternal
{
	protected readonly BlahSet<T> Entries = new(2, 0);

	protected int[] AliveEntriesPtrs = new int[1];
	protected int   AliveEntriesCount = 0;
    
	protected int[] IteratorCursorByLevel = new int[1];
	protected int   GoingIteratorsCount = 0;
    
	private DelayedOp[] _delayedOps = new DelayedOp[1];
	private int         _delayedOpsCount = 0;

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public int  Count   => AliveEntriesCount;
	public bool IsEmpty => AliveEntriesCount == 0;
	
	public ref T Add()
	{
		int ptr = Entries.Add();

		if (GoingIteratorsCount == 0)
			AddWithoutDelay(ptr);
		else
			AddDelayedOp(true, ptr);

		Entries.Get(ptr) = default;
		return ref Entries.Get(ptr);
	}

	private void AddWithoutDelay(int entryPtr)
	{
		BlahArrayHelper.ResizeOnDemand(ref AliveEntriesPtrs, AliveEntriesCount);
		AliveEntriesPtrs[AliveEntriesCount++] = entryPtr;
		
		OnAliveEntryAdded(entryPtr);
	}


	public void Remove(int iteratorLevel)
	{
		if (iteratorLevel == -1)
			iteratorLevel = GoingIteratorsCount - 1;
		else if (iteratorLevel < 0 || iteratorLevel >= GoingIteratorsCount)
			throw new Exception($"current max level is {GoingIteratorsCount - 1}, but {iteratorLevel} passed");

		int cursor   = IteratorCursorByLevel[iteratorLevel];
		int alivePtr = AliveEntriesPtrs[cursor];
		AddDelayedOp(false, alivePtr);
	}
    
	protected void RemoveSafe(int entryPtr)
	{
		if (GoingIteratorsCount > 0)
			AddDelayedOp(false, entryPtr);
		else
			RemoveWithoutDelay(entryPtr);
	}
	
	private void RemoveWithoutDelay(int entryPtr)
	{
		for (var i = 0; i < AliveEntriesCount; i++)
			if (AliveEntriesPtrs[i] == entryPtr)
			{
				if (AliveEntriesCount == 1)
					AliveEntriesCount = 0;
				else
					AliveEntriesPtrs[i] = AliveEntriesPtrs[--AliveEntriesCount];
				Entries.Remove(entryPtr);
				
				OnAliveEntryRemoved(entryPtr);
				break;
			}
	}
    
	private void AddDelayedOp(bool isAdd, int entryPtr)
	{
		BlahArrayHelper.ResizeOnDemand(ref _delayedOps, _delayedOpsCount);
		ref var op = ref _delayedOps[_delayedOpsCount++];
		op.IsAdd    = isAdd;
		op.EntryPtr = entryPtr;
	}
    
	protected virtual void OnAliveEntryAdded(int   entryPtr) { }
	protected virtual void OnAliveEntryRemoved(int entryPtr) { }

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref T GetAny()
	{
		if (AliveEntriesCount == 0)
			throw new Exception("Signal is empty!");
		return ref Entries.Get(AliveEntriesPtrs[0]);
	}

	/// <summary>
	/// Use this method if you need a specific order in foreach loop.<br/>
	/// Very slow.<br/>
	/// Does not affect nextFrame signals.
	/// </summary>
	/// <param name="comp">
	/// Lambda should return:<br/>
	/// -1 if first less second;<br/>
	/// 0 if first equals second;<br/>
	/// 1 if first greater second;<br/> 
	/// </param>
	public void Sort(Comparison<T> comp)
	{
		if (GoingIteratorsCount > 0)
			throw new Exception("Sorting during foreach loop is not allowed");

		var sortedPtrs = new int[AliveEntriesCount];
		Array.Copy(AliveEntriesPtrs, sortedPtrs, AliveEntriesCount);
		Array.Sort(sortedPtrs, (idxA, idxB) => comp.Invoke(Entries.Get(idxA), Entries.Get(idxB)));
		Array.Copy(sortedPtrs, AliveEntriesPtrs, AliveEntriesCount);
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public virtual void OnNextFrame() { }

	public virtual void Clear()
	{
		Entries.Clear();
		AliveEntriesCount = 0;
		_delayedOpsCount  = 0;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private struct DelayedOp
	{
		public bool IsAdd;
		public int  EntryPtr;
	}

	private void ApplyDelayedOps()
	{
		for (var opIdx = 0; opIdx < _delayedOpsCount; opIdx++)
			if (_delayedOps[opIdx].IsAdd)
				AddWithoutDelay(_delayedOps[opIdx].EntryPtr);
			else
				RemoveWithoutDelay(_delayedOps[opIdx].EntryPtr);
		_delayedOpsCount = 0;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public Enumerator GetEnumerator() => new(this);

	public readonly struct Enumerator : IDisposable
	{
		private readonly BlahPool<T> _owner;

		private readonly int _level;

		public Enumerator(BlahPool<T> owner)
		{
			_owner = owner;

			BlahArrayHelper.ResizeOnDemand(ref _owner.IteratorCursorByLevel, _owner.GoingIteratorsCount);
			_level = _owner.GoingIteratorsCount++;

			// alivePtrs starts from 0, but we need -1 for first MoveNext
			_owner.IteratorCursorByLevel[_level] = -1;
		}

		public ref T Current => ref _owner.Entries.Get(
			_owner.AliveEntriesPtrs[_owner.IteratorCursorByLevel[_level]]
		);

		public bool MoveNext() => ++_owner.IteratorCursorByLevel[_level] < _owner.AliveEntriesCount;

		public void Dispose()
		{
			if (--_owner.GoingIteratorsCount == 0 && _owner._delayedOpsCount > 0)
				_owner.ApplyDelayedOps();
		}
	}
}
}
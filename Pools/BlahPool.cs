using System;
using System.Runtime.CompilerServices;
using Blah.Common;

namespace Blah.Pools
{
internal class BlahDataPool<T> :
	BlahPool<T>,
	IBlahDataConsumer<T>,
	IBlahDataProducer<T> where T : IBlahEntryData
{
	public BlahDataPool() : base(false) { }
}

internal class BlahSignalPool<T> :
	BlahPool<T>,
	IBlahSignalConsumer<T>,
	IBlahSignalProducer<T> where T : IBlahEntrySignal
{
	public BlahSignalPool() : base(true) { }
}

internal class BlahSignalNextFramePool<T> :
	BlahPool<T>,
	IBlahSignalNextFrameConsumer<T>,
	IBlahSignalNextFrameProducer<T> where T : IBlahEntryNextFrameSignal
{
	public BlahSignalNextFramePool() : base(true) { }
}

internal interface IBlahPoolInternal
{
	void ToNextFrame();
	void RemoveAll();
}

public class BlahPool<T> : IBlahPoolInternal
{
	private readonly bool _isSignal;
	private readonly bool _isAutoReset;

	private readonly BlahSet<T> _set = new(2, 0);

	private int[] _alivePtrs;
	private int   _aliveCount;

	private int[] _nextFramePtrs;
	private int   _nextFrameCount;

	private DelayedOp[] _delayedOps;
	private int         _delayedOpsCount;

	private int   _goingIteratorsCount;
	private int[] _iteratorCursorByLevel;

	protected BlahPool(bool isSignal)
	{
		_isSignal  = isSignal;

		_alivePtrs             = new int[1];
		_nextFramePtrs         = new int[1];
		_delayedOps            = new DelayedOp[1];
		_iteratorCursorByLevel = new int[1];

		var type = typeof(T);
		foreach (var api in type.GetInterfaces())
			if (api == typeof(IBlahEntryAutoReset))
			{
				_isAutoReset = true;
				break;
			}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public int  Count   => _aliveCount;
	public bool IsEmpty => _aliveCount == 0;
	
	public ref T Add()
	{
		int ptr = _set.Add();
		
		if (_goingIteratorsCount == 0)
		{
			ValidateSize(ref _alivePtrs, _aliveCount);
			_alivePtrs[_aliveCount++] = ptr;
		}
		else
		{
			ValidateSize(ref _delayedOps, _delayedOpsCount);
			
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.IsAdd = true;
			op.Ptr   = ptr;
		}
		
		if (_isAutoReset)
			_set.Get(ptr) = default;
		return ref _set.Get(ptr);
	}

	public ref T AddNextFrame()
	{
		ValidateSize(ref _nextFramePtrs, _nextFrameCount);

		int ptr = _set.Add();
		_nextFramePtrs[_nextFrameCount++] = ptr;
		return ref _set.Get(ptr);
	}

	public void Remove(int iteratorLevel)
	{
		if (iteratorLevel >= _goingIteratorsCount)
			throw new Exception($"current max level is {_goingIteratorsCount - 1}");
		
		ValidateSize(ref _delayedOps, _delayedOpsCount);
		ref var op = ref _delayedOps[_delayedOpsCount++];
		op.IsAdd = false;
		op.Ptr   = _alivePtrs[_iteratorCursorByLevel[iteratorLevel]];
	}

	public void RemoveAll()
	{
		if (_goingIteratorsCount > 0)
			throw new Exception($"{nameof(RemoveAll)} is not supported during iteration");

		_set.RemoveAll();
		_aliveCount      = 0;
		_nextFrameCount  = 0;
		_delayedOpsCount = 0;
	}
	
	
	public ref T GetAny()
	{
		if (_aliveCount == 0)
			throw new Exception("Signal is empty!");
		return ref _set.Get(_alivePtrs[0]);
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
		if (_goingIteratorsCount > 0)
			throw new Exception("Sorting during foreach loop is not allowed");

		var sortedAlivePtrs = new int[_aliveCount];
		Array.Copy(_alivePtrs, sortedAlivePtrs, _aliveCount);
		Array.Sort(sortedAlivePtrs, (idxA, idxB) => comp.Invoke(_set.Get(idxA), _set.Get(idxB)));
		Array.Copy(sortedAlivePtrs, _alivePtrs, _aliveCount);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private struct DelayedOp
	{
		public bool IsAdd;
		public int  Ptr;
	}

	private void ApplyDelayedOps()
	{
		for (var opIdx = 0; opIdx < _delayedOpsCount; opIdx++)
			if (_delayedOps[opIdx].IsAdd)
			{
				ValidateSize(ref _alivePtrs, _aliveCount);
				_alivePtrs[_aliveCount++] = _delayedOps[opIdx].Ptr;
			}
			else
			{
				for (var i = 0; i < _aliveCount; i++)
					if (_alivePtrs[i] == _delayedOps[opIdx].Ptr)
					{
						if (_aliveCount == 1)
							_aliveCount = 0;
						else
							_alivePtrs[i] = _alivePtrs[--_aliveCount];
						_set.Remove(_delayedOps[opIdx].Ptr);
						break;
					}
			}
		_delayedOpsCount = 0;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public Enumerator GetEnumerator()
	{
		if (_goingIteratorsCount == _iteratorCursorByLevel.Length)
			Array.Resize(ref _iteratorCursorByLevel, _goingIteratorsCount * 2);

		return new Enumerator(this, _goingIteratorsCount++);
	}

	private void OnEnumeratorEnd()
	{
		if (--_goingIteratorsCount == 0)
			ApplyDelayedOps();
	}
	
	
	public readonly struct Enumerator : IDisposable
	{
		private readonly BlahPool<T> _owner;
		private readonly BlahSet<T>  _set;
		private readonly int[]       _alivePtrs;
		private readonly int         _aliveCount;

		private readonly int   _level;

		public Enumerator(BlahPool<T> owner, int level)
		{
			_owner         = owner;
			_set           = owner._set;
			_alivePtrs     = owner._alivePtrs;
			_aliveCount    = owner._aliveCount;
			_level         = level;

			_owner._iteratorCursorByLevel[_level] = -1; // alivePtrs starts from 0, but we need -1 for first MoveNext
		}

		public ref T Current => ref _set.Get(_alivePtrs[_owner._iteratorCursorByLevel[_level]]);

		public bool MoveNext() => ++_owner._iteratorCursorByLevel[_level] < _aliveCount;

		public void Dispose()
		{
			_owner.OnEnumeratorEnd();
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	void IBlahPoolInternal.ToNextFrame()
	{
		if (_isSignal)
		{
			if (_nextFrameCount > 0)
			{
				for (var i = 0; i < _aliveCount; i++)
					_set.Remove(_alivePtrs[i]);
			}
			else
				_set.RemoveAll();
			_aliveCount = 0;
		}
		if (_nextFrameCount > 0)
		{
			ValidateSize(ref _alivePtrs, _aliveCount + _nextFrameCount);
			for (var i = 0; i < _nextFrameCount; i++)
				_alivePtrs[_aliveCount++] = _nextFramePtrs[i];
			_nextFrameCount = 0;
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ValidateSize<T1>(ref T1[] arr, int count)
	{
		if (arr.Length <= count)
			Array.Resize(ref arr, count * 2);
	}
}
}
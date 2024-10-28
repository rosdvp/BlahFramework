using Blah.Common;

namespace Blah.Pools
{
public interface IBlahEntryNfSignal { }


public interface IBlahNfSignalRead<T> where T : struct, IBlahEntryNfSignal
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public ref T GetAny();
	
	public void Remove(int iteratorLevel = -1);
	public void RemoveAll();
}

public interface IBlahNfSignalWrite<T> where T : struct, IBlahEntryNfSignal
{
	public ref T AddNf();
}

internal class BlahNfSignalPool<T> :
	BlahPool<T>,
	IBlahNfSignalRead<T>,
	IBlahNfSignalWrite<T> where T : struct, IBlahEntryNfSignal
{
	private int[] _nextFramePtrs  = new int[1];
	private int   _nextFrameCount = 0;

	public ref T AddNf()
	{
		BlahArrayHelper.ResizeOnDemand(ref _nextFramePtrs, _nextFrameCount);

		int ptr = Entries.Add();
		_nextFramePtrs[_nextFrameCount++] = ptr;
		return ref Entries.Get(ptr);
	}

	public override void OnNextFrame()
	{
		if (_nextFrameCount > 0)
		{
			for (var i = 0; i < AliveEntriesCount; i++)
				Entries.Remove(AliveEntriesPtrs[i]);
			AliveEntriesCount = 0;

			BlahArrayHelper.ResizeOnDemand(ref AliveEntriesPtrs, _nextFrameCount);
			for (var i = 0; i < _nextFrameCount; i++)
				AliveEntriesPtrs[AliveEntriesCount++] = _nextFramePtrs[i];

			_nextFrameCount = 0;
		}
		else
		{
			Entries.Clear();
			AliveEntriesCount = 0;
		}
	}

	public override void Clear()
	{
		base.Clear();
		_nextFrameCount = 0;
	}
}
}
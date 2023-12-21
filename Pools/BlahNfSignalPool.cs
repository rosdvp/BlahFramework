using Blah.Common;

namespace Blah.Pools
{
public interface IBlahEntryNextFrameSignal { }

public interface IBlahNfSignalProducer<T> where T : IBlahEntryNextFrameSignal
{
	public ref T AddNf();
}

public interface IBlahNfSignalConsumer<T> where T : IBlahEntryNextFrameSignal
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public void Remove(int iteratorLevel = -1);

	public ref T GetAny();
}

internal class BlahNfSignalPool<T> :
	BlahPool<T>,
	IBlahNfSignalConsumer<T>,
	IBlahNfSignalProducer<T> where T : IBlahEntryNextFrameSignal
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
			Entries.RemoveAll();
			AliveEntriesCount = 0;
		}
	}

	public override void RemoveAll()
	{
		base.RemoveAll();
		_nextFrameCount = 0;
	}
}
}
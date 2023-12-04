using System;

namespace Blah.Pools
{
public interface IBlahEntrySignal { }

public interface IBlahSignalConsumer<T> where T : IBlahEntrySignal
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public void Remove(int iteratorLevel = 0);

	public ref T GetAny();
	public void Sort(Comparison<T> comp);
}

public interface IBlahSignalProducer<T> where T : IBlahEntrySignal
{
	public ref T Add();
}

internal class BlahSignalPool<T> :
	BlahPool<T>,
	IBlahSignalConsumer<T>,
	IBlahSignalProducer<T> where T : IBlahEntrySignal
{
	public override void OnNextFrame()
	{
		RemoveAll();
	}
}
}
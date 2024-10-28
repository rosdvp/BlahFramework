using System;

namespace Blah.Pools
{
public interface IBlahEntrySignal { }

public interface IBlahSignalRead<T> where T : struct, IBlahEntrySignal
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public ref T GetAny();
	public void Sort(Comparison<T> comp);

	public void Remove(int iteratorLevel = -1);
	public void RemoveAll();
}

public interface IBlahSignalWrite<T> where T : struct, IBlahEntrySignal
{
	public ref T Add();
}

internal class BlahSignalPool<T> :
	BlahPool<T>,
	IBlahSignalRead<T>,
	IBlahSignalWrite<T> where T : struct, IBlahEntrySignal
{
	public override void OnNextFrame()
	{
		Clear();
	}
}
}
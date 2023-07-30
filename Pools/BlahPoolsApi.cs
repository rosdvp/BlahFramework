using System;

namespace Blah.Pools
{
public interface IBlahEntryAutoReset { }
public interface IBlahEntryData { }

public interface IBlahEntrySignalConsumable { }
public interface IBlahEntrySignal : IBlahEntrySignalConsumable { }
public interface IBlahEntryNextFrameSignal : IBlahEntrySignalConsumable { }

public interface IBlahDataConsumer<T> where T : IBlahEntryData
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public void Remove(int iteratorLevel = 0);
	public void RemoveAll();

	public ref T GetAny();
	public void Sort(Comparison<T> comp);
}

public interface IBlahDataProducer<T> where T : IBlahEntryData
{
	public ref T Add();
}

public interface IBlahSignalConsumer<T> where T : IBlahEntrySignalConsumable
{
	public bool IsEmpty { get; }
	public int  Count   { get; }

	public BlahPool<T>.Enumerator GetEnumerator();

	public void Remove(int iteratorLevel = 0);
	public void RemoveAll();

	public ref T GetAny();
	public void Sort(Comparison<T> comp);
}

public interface IBlahSignalProducer<T> where T : IBlahEntrySignal
{
	public ref T Add();
}

public interface IBlahSignalNextFrameProducer<T> where T : IBlahEntryNextFrameSignal
{
	public ref T AddNextFrame();
}
}
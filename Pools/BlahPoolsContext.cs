using System;
using System.Collections.Generic;

namespace Blah.Pools
{
public class BlahPoolsContext
{
	private readonly Dictionary<Type, IBlahPoolInternal> _map = new();
	private readonly List<IBlahPoolInternal>             _all = new();

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IBlahDataConsumer<T> GetDataConsumer<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataConsumer<T>)cached;
		return (IBlahDataConsumer<T>)AddPool(new BlahDataPool<T>());
	}

	public IBlahDataProducer<T> GetDataProducer<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataProducer<T>)cached;
		return (IBlahDataProducer<T>)AddPool(new BlahDataPool<T>());
	}

	public IBlahSignalConsumer<T> GetSignalConsumer<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalConsumer<T>)cached;
		return (IBlahSignalConsumer<T>)AddPool(new BlahSignalPool<T>());
	}

	public IBlahSignalProducer<T> GetSignalProducer<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalProducer<T>)cached;
		return (IBlahSignalProducer<T>)AddPool(new BlahSignalPool<T>());
	}

	public IBlahSignalConsumer<T> GetSignalConsumerNextFrame<T>() where T: IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalConsumer<T>)cached;
		return (IBlahSignalConsumer<T>)AddPool(new BlahSignalNextFramePool<T>());
	}

	public IBlahSignalNextFrameProducer<T> GetSignalProducerNextFrame<T>() where T: IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalNextFrameProducer<T>)cached;
		return (IBlahSignalNextFrameProducer<T>)AddPool(new BlahSignalNextFramePool<T>());
	}


	private BlahPool<T> AddPool<T>(BlahPool<T> pool)
	{
		_map[typeof(T)] = pool;
		_all.Add(pool);
		return pool;
	}

	/// <summary>
	/// Clear current frame, and switch to next frame.<br/>
	/// Entries produced via <see cref="IBlahSignalNextFrameProducer{T}"/> will become available.
	/// </summary>
	public void ToNextFrame()
	{
		for (var i = 0; i < _all.Count; i++)
			_all[i].ToNextFrame();
	}

	/// <summary>
	/// Clear all entries.
	/// </summary>
	public void Clear()
	{
		for (var i = 0; i < _all.Count; i++)
			_all[i].RemoveAll();
	}
}
}
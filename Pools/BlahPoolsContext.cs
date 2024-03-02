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
	public void Get<T>(out IBlahDataConsumer<T> consumer) where T : IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			consumer = (IBlahDataConsumer<T>)cached;
		else
			consumer = (IBlahDataConsumer<T>)AddPool<T>(new BlahDataPool<T>());
	}
	
	public void Get<T>(out IBlahDataProducer<T> producer) where T : IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			producer = (IBlahDataProducer<T>)cached;
		else
			producer = (IBlahDataProducer<T>)AddPool<T>(new BlahDataPool<T>());
	}
	
	public void Get<T>(out IBlahSignalConsumer<T> consumer) where T : IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			consumer = (IBlahSignalConsumer<T>)cached;
		else
			consumer = (IBlahSignalConsumer<T>)AddPool<T>(new BlahSignalPool<T>());
	}
	
	public void Get<T>(out IBlahSignalProducer<T> producer) where T : IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			producer = (IBlahSignalProducer<T>)cached;
		else
			producer = (IBlahSignalProducer<T>)AddPool<T>(new BlahSignalPool<T>());
	}
	public void Get<T>(out IBlahNfSignalConsumer<T> consumer) where T : IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			consumer = (IBlahNfSignalConsumer<T>)cached;
		else
			consumer = (IBlahNfSignalConsumer<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}
	
	public void Get<T>(out IBlahNfSignalProducer<T> producer) where T : IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			producer = (IBlahNfSignalProducer<T>)cached;
		else
			producer = (IBlahNfSignalProducer<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}
    
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IBlahDataConsumer<T> GetDataConsumer<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataConsumer<T>)cached;
		return (IBlahDataConsumer<T>)AddPool<T>(new BlahDataPool<T>());
	}

	public IBlahDataProducer<T> GetDataProducer<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataProducer<T>)cached;
		return (IBlahDataProducer<T>)AddPool<T>(new BlahDataPool<T>());
	}

	public IBlahSignalConsumer<T> GetSignalConsumer<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalConsumer<T>)cached;
		return (IBlahSignalConsumer<T>)AddPool<T>(new BlahSignalPool<T>());
	}

	public IBlahSignalProducer<T> GetSignalProducer<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalProducer<T>)cached;
		return (IBlahSignalProducer<T>)AddPool<T>(new BlahSignalPool<T>());
	}

	public IBlahNfSignalConsumer<T> GetNfSignalConsumer<T>() where T: IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahNfSignalConsumer<T>)cached;
		return (IBlahNfSignalConsumer<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}

	public IBlahNfSignalProducer<T> GetNfSignalProducer<T>() where T: IBlahEntryNextFrameSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahNfSignalProducer<T>)cached;
		return (IBlahNfSignalProducer<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}

	public IBlahSoloSignalConsumer<T> GetSoloSignalConsumer<T>() where T : IBlahEntrySoloSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSoloSignalConsumer<T>)cached;
		return (IBlahSoloSignalConsumer<T>)AddPool<T>(new BlahSoloSignalPool<T>());
	}

	public IBlahSoloSignalProducer<T> GetSoloSignalProducer<T>() where T : IBlahEntrySoloSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSoloSignalProducer<T>)cached;
		return (IBlahSoloSignalProducer<T>)AddPool<T>(new BlahSoloSignalPool<T>());
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private IBlahPoolInternal AddPool<T>(IBlahPoolInternal pool)
	{
		_map[typeof(T)] = pool;
		_all.Add(pool);
		return pool;
	}

	public void OnNextFrame()
	{
		for (var i = 0; i < _all.Count; i++)
			_all[i].OnNextFrame();
	}

	/// <summary>
	/// Clear all entries.
	/// </summary>
	public void Clear()
	{
		for (var i = 0; i < _all.Count; i++)
			_all[i].Clear();
	}
}
}
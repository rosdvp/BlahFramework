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
	public void Get<T>(out IBlahDataGet<T> getter) where T : IBlahEntryData
		=> getter = GetDataGetter<T>();

	public void Get<T>(out IBlahDataAdd<T> adder) where T : IBlahEntryData
		=> adder = GetDataAdder<T>();

	public void Get<T>(out IBlahSignalRead<T> read) where T : IBlahEntrySignal
		=> read = GetSignalRead<T>();

	public void Get<T>(out IBlahSignalWrite<T> write) where T : IBlahEntrySignal
		=> write = GetSignalWrite<T>();

	public void Get<T>(out IBlahNfSignalRead<T> read) where T : IBlahEntryNfSignal
		=> read = GetNfSignalRead<T>();

	public void Get<T>(out IBlahNfSignalWrite<T> write) where T : IBlahEntryNfSignal
		=> write = GetNfSignalWrite<T>();
    
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IBlahSignalRead<T> GetSignalRead<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalRead<T>)cached;
		return (IBlahSignalRead<T>)AddPool<T>(new BlahSignalPool<T>());
	}

	public IBlahSignalWrite<T> GetSignalWrite<T>() where T: IBlahEntrySignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahSignalWrite<T>)cached;
		return (IBlahSignalWrite<T>)AddPool<T>(new BlahSignalPool<T>());
	}

	public IBlahNfSignalRead<T> GetNfSignalRead<T>() where T: IBlahEntryNfSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahNfSignalRead<T>)cached;
		return (IBlahNfSignalRead<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}

	public IBlahNfSignalWrite<T> GetNfSignalWrite<T>() where T: IBlahEntryNfSignal
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahNfSignalWrite<T>)cached;
		return (IBlahNfSignalWrite<T>)AddPool<T>(new BlahNfSignalPool<T>());
	}
	
	public IBlahDataGet<T> GetDataGetter<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataGet<T>)cached;
		return (IBlahDataGet<T>)AddPool<T>(new BlahDataPool<T>());
	}

	public IBlahDataAdd<T> GetDataAdder<T>() where T: IBlahEntryData
	{
		if (_map.TryGetValue(typeof(T), out var cached))
			return (IBlahDataAdd<T>)cached;
		return (IBlahDataAdd<T>)AddPool<T>(new BlahDataPool<T>());
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
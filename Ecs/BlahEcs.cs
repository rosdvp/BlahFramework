using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public class BlahEcs
{
	private BlahEntities _entities;
	
	private readonly object[] _compPoolConstructParams = new object[3];
	
	private List<IBlahCompPoolInternal>             _compPools      = new();
	private Dictionary<Type, IBlahCompPoolInternal> _compTypeToPool = new();

	private List<BlahFilterCore>                   _filtersCores     = new();
	private Dictionary<int, BlahFilterCore>        _hashToFilterCore = new();
	private Dictionary<Type, List<BlahFilterCore>> _incCompToFilters = new();
	private Dictionary<Type, List<BlahFilterCore>> _excCompToFilters = new();


	public BlahEcs()
	{
		_entities = new BlahEntities(16);

		_compPoolConstructParams[0] = _entities;
		_compPoolConstructParams[1] = (Action<Type, BlahEnt>)OnCompAdded;
		_compPoolConstructParams[2] = (Action<Type, BlahEnt>)OnCompRemoved;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref BlahEnt CreateEnt() => ref _entities.Create();

	public bool IsEntAlive(BlahEnt entity) => _entities.IsAlive(entity);
	
	public void DestroyEnt(BlahEnt entity)
	{
		foreach (var pool in _compPools)
			if (pool.Has(entity))
				pool.RemoveWithoutCb(entity);
		
		foreach (var filter in _filtersCores)
			filter.OnIncCompRemovedOrExcAdded(entity);
		
		_entities.Destroy(entity);
	}



	private void OnCompAdded(Type type, BlahEnt ent)
	{
		if (_incCompToFilters.TryGetValue(type, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(ent);
		if (_excCompToFilters.TryGetValue(type, out filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(ent);
	}

	private void OnCompRemoved(Type type, BlahEnt ent)
	{
		if (_incCompToFilters.TryGetValue(type, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(ent);
		if (_excCompToFilters.TryGetValue(type, out filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(ent);
	}

	public void Clear()
	{
		_entities.Clear();
		foreach (var pool in _compPools)
			pool.Clear();
		foreach (var filter in _filtersCores)
			filter.Clear();
	}


	public T GetFilter<T>() where T : BlahFilter, new()
	{
		BlahFilter.BuilderIncCompsTypes.Clear();
		BlahFilter.BuilderExcCompsTypes.Clear();

		var filter = new T();

		var incCompsTypes = BlahFilter.BuilderIncCompsTypes;
		var excCompsTypes = BlahFilter.BuilderExcCompsTypes;

		SortTypes(incCompsTypes);

		int hash = incCompsTypes[0].GetHashCode();
		for (var i = 1; i < incCompsTypes.Count; i++)
			hash = HashCode.Combine(hash, incCompsTypes[i]);
		hash *= 31;
		if (excCompsTypes != null)
		{
			SortTypes(excCompsTypes);
			foreach (var excCompType in excCompsTypes)
				hash = HashCode.Combine(hash, excCompType);
		}

		if (_hashToFilterCore.TryGetValue(hash, out var core))
		{
			filter.Set(core);
			return filter;
		}

		var incCompsPools = new IBlahCompPoolInternal[incCompsTypes.Count];
		for (var i = 0; i < incCompsPools.Length; i++)
			incCompsPools[i] = GetPool(incCompsTypes[i]);

		IBlahCompPoolInternal[] excCompsPools = null;
		if (excCompsTypes.Count == 0)
		{
			excCompsPools = Array.Empty<IBlahCompPoolInternal>();
		}
		else
		{
			excCompsPools = new IBlahCompPoolInternal[excCompsTypes.Count];
			for (var i = 0; i < excCompsPools.Length; i++)
				excCompsPools[i] = GetPool(excCompsTypes[i]);
		}

		core = new BlahFilterCore(_entities, incCompsPools, excCompsPools);
		_filtersCores.Add(core);
		_hashToFilterCore[hash] = core;

		foreach (var type in incCompsTypes)
		{
			if (!_incCompToFilters.TryGetValue(type, out var cores))
			{
				cores                 = new List<BlahFilterCore>();
				_incCompToFilters[type] = cores;
			}
			cores.Add(core);
		}
		if (excCompsTypes.Count > 0)
			foreach (var type in excCompsTypes)
			{
				if (!_excCompToFilters.TryGetValue(type, out var cores))
				{
					cores                   = new List<BlahFilterCore>();
					_excCompToFilters[type] = cores;
				}
				cores.Add(core);
			}
		filter.Set(core);
		return filter;
	}


	public IBlahCompGet<T> GetCompGetter<T>() where T : struct, IBlahEntryComp
		=> GetPool<T>();

	public IBlahCompFull<T> GetCompFull<T>() where T : struct, IBlahEntryComp
		=> GetPool<T>();
	
	private BlahCompPool<T> GetPool<T>() where T : struct, IBlahEntryComp
	{
		var type = typeof(T);
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			pool = new BlahCompPool<T>(_entities, OnCompAdded, OnCompRemoved);

			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return (BlahCompPool<T>)pool;
	}

	private IBlahCompPoolInternal GetPool(Type type)
	{
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			var poolType = typeof(BlahCompPool<>).MakeGenericType(type);
			pool = (IBlahCompPoolInternal)Activator.CreateInstance(poolType, _compPoolConstructParams);
			
			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return pool;
	}
    
	private void SortTypes(List<Type> types)
	{
		types.Sort((a, b) => a.GetHashCode().CompareTo(b.GetHashCode()));
	}
}
}
using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public class BlahEcs
{
	private BlahEcsEntities _entities;
	
	private List<IBlahEcsCompInternal>             _compPools      = new();
	private Dictionary<Type, IBlahEcsCompInternal> _compTypeToPool = new();

	private List<BlahEcsFilterCore>                   _filters          = new();
	private Dictionary<int, BlahEcsFilterCore>        _hashToFilter     = new();
	private Dictionary<Type, List<BlahEcsFilterCore>> _incCompToFilters = new();
	private Dictionary<Type, List<BlahEcsFilterCore>> _excCompToFilters = new();
		
		
	private readonly object[] _compPoolConstructParams = new object[3];

	public BlahEcs()
	{
		_entities = new BlahEcsEntities(16);

		_compPoolConstructParams[0] = _entities;
		_compPoolConstructParams[1] = (Action<Type, BlahEcsEntity>)OnCompAdded;
		_compPoolConstructParams[2] = (Action<Type, BlahEcsEntity>)OnCompRemoved;
	}
	
	public void Clear()
	{
		_entities.Clear();
		foreach (var pool in _compPools)
			pool.Clear();
		foreach (var filter in _filters)
			filter.Clear();
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref BlahEcsEntity CreateEntity() => ref _entities.Create();

	public bool IsEntityAlive(BlahEcsEntity entity) => _entities.IsAlive(entity);
	
	public void DestroyEntity(BlahEcsEntity entity)
	{
		foreach (var pool in _compPools)
			if (pool.Has(entity))
				pool.RemoveWithoutCb(entity);
		
		foreach (var filter in _filters)
			filter.OnIncCompRemovedOrExcAdded(entity);
		
		_entities.Destroy(entity);
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IBlahEcsGet<T> GetCompGetter<T>() where T : IBlahEntryEcs 
		=> GetPool<T>();

	public IBlahEcsFull<T> GetCompFull<T>() where T : IBlahEntryEcs
		=> GetPool<T>();
	
	internal BlahEcsPool<T> GetPool<T>() where T : IBlahEntryEcs
	{
		var type = typeof(T);
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			pool = new BlahEcsPool<T>(_entities, OnCompAdded, OnCompRemoved);

			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return (BlahEcsPool<T>)pool;
	}

	internal IBlahEcsCompInternal GetPool(Type type)
	{
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			var poolType = typeof(BlahEcsPool<>).MakeGenericType(type);
			pool = (IBlahEcsCompInternal)Activator.CreateInstance(poolType, _compPoolConstructParams);
			
			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return pool;
	}
	

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public T CreateFilter<T>() where T: BlahEcsFilter, new()
	{
		return BlahEcsFilter.Create<T>(this);
	}

	internal BlahEcsFilterCore GetFilterCore(
		List<IBlahEcsCompInternal> incPools,
		List<IBlahEcsCompInternal> excPools)
	{
		// Calculate mask
		incPools.Sort((a, b) => a.CompType.GetHashCode().CompareTo(b.CompType.GetHashCode()));
		excPools.Sort((a, b) => a.CompType.GetHashCode().CompareTo(b.CompType.GetHashCode()));
		int hash = incPools[0].CompType.GetHashCode();
		for (var i = 1; i < incPools.Count; i++)
			hash = HashCode.Combine(hash, incPools[i].CompType);
		hash *= 31;
		for (var i = 0; i < excPools.Count; i++)
			hash = HashCode.Combine(hash, excPools[i].CompType);

		// Try return existing filter with the same mask
		if (_hashToFilter.TryGetValue(hash, out var filter))
			return filter;


		filter = new BlahEcsFilterCore(_entities, incPools.ToArray(), excPools.ToArray());
		_filters.Add(filter);
		_hashToFilter[hash] = filter;

		// Register filter for comps updates
		foreach (var incPool in incPools)
		{
			if (!_incCompToFilters.TryGetValue(incPool.CompType, out var filters))
			{
				filters                             = new List<BlahEcsFilterCore>();
				_incCompToFilters[incPool.CompType] = filters;
			}
			filters.Add(filter);
		}
		foreach (var excPool in excPools)
		{
			if (!_excCompToFilters.TryGetValue(excPool.CompType, out var filters))
			{
				filters                             = new List<BlahEcsFilterCore>();
				_excCompToFilters[excPool.CompType] = filters;
			}
			filters.Add(filter);
		}
		return filter;
	}

	private void OnCompAdded(Type type, BlahEcsEntity ent)
	{
		if (_incCompToFilters.TryGetValue(type, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(ent);
		if (_excCompToFilters.TryGetValue(type, out filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(ent);
	}

	private void OnCompRemoved(Type type, BlahEcsEntity ent)
	{
		if (_incCompToFilters.TryGetValue(type, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(ent);
		if (_excCompToFilters.TryGetValue(type, out filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(ent);
	}
}
}
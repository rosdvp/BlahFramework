using System;
using System.Collections.Generic;
using Blah.Common;

namespace Blah.Ecs
{
public class BlahEcs
{
	private BlahSet<BlahEcsEntity> _entitiesSet        = new(1, 1);
	private int[]                  _aliveEntitiesIds   = new int[1];
	private int                    _aliveEntitiesCount = 0;

	private List<IBlahEcsPool>             _compPools      = new();
	private Dictionary<Type, IBlahEcsPool> _compTypeToPool = new();

	private List<BlahEcsFilter>                   _filters          = new();
	private Dictionary<int, BlahEcsFilter>        _hashToFilter     = new();
	private Dictionary<Type, List<BlahEcsFilter>> _incCompToFilters = new();
	private Dictionary<Type, List<BlahEcsFilter>> _excCompToFilters = new();

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref BlahEcsEntity CreateEntity()
	{
		int id = _entitiesSet.Add();

		if (_aliveEntitiesIds.Length == _aliveEntitiesCount)
			Array.Resize(ref _aliveEntitiesIds, _aliveEntitiesCount * 2);
		
		_aliveEntitiesIds[_aliveEntitiesCount++] = id;

		ref var entity = ref _entitiesSet.Get(id);
		entity.World = this;
		entity.Id    = id;

		return ref _entitiesSet.Get(id);
	}

	internal void DestroyEntity(BlahEcsEntity entity)
	{
		foreach (var pool in _compPools)
			if (pool.Has(entity.Id))
				pool.Remove(entity.Id);
		
		foreach (var filter in _filters)
			filter.OnIncCompRemovedOrExcAdded(entity);
	}


	internal ref T AddComp<T>(BlahEcsEntity entity) where T : IBlahEntryEcs
	{
		var pool = GetPool<T>();

		if (pool.Has(entity.Id))
			throw new Exception($"entity {entity.Id} already have {nameof(T)}");
		
		pool.Add(entity.Id);

		var compType = typeof(T);
		if (_incCompToFilters.TryGetValue(compType, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(entity);
		if (_excCompToFilters.TryGetValue(compType, out filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(entity);

		return ref pool.Get(entity.Id);
	}

	internal void RemoveComp<T>(BlahEcsEntity entity) where T : IBlahEntryEcs
	{
		var pool = GetPool<T>();

		if (!pool.Has(entity.Id))
			throw new Exception($"entity {entity.Id} does not have {nameof(T)}");
		
		pool.Remove(entity.Id);

		var compType = typeof(T);
		if (_incCompToFilters.TryGetValue(compType, out var filters))
			foreach (var filter in filters)
				filter.OnIncCompRemovedOrExcAdded(entity);
		
		if (_excCompToFilters.TryGetValue(compType, out filters))
			foreach (var filter in filters)
				filter.OnIncCompAddedOrExcRemoved(entity);
	}

	internal ref T GetComp<T>(BlahEcsEntity entity) where T : IBlahEntryEcs
	{
		var pool = GetPool<T>();
		if (!pool.Has(entity.Id))
			throw new Exception($"entity {entity.Id} does not have {nameof(T)}");
		return ref pool.Get(entity.Id);
	}

	internal bool HasComp<T>(BlahEcsEntity entity) where T : IBlahEntryEcs
	{
		return GetPool<T>().Has(entity.Id);
	}


	internal (BlahSet<BlahEcsEntity> set, int[] alivePtrs, int aliveCounts) GetEntities() 
		=> (_entitiesSet, _aliveEntitiesIds, _aliveEntitiesCount);


	public BlahEcsFilterProxy GetFilter<T>(Type[] incCompTypes, Type[] excCompTypes) where T : BlahEcsFilterProxy
	{
		SortTypes(ref incCompTypes);

		int hash = incCompTypes[0].GetHashCode();
		for (var i = 1; i < incCompTypes.Length; i++)
			hash = HashCode.Combine(hash, incCompTypes[i]);
		hash *= 31;
		if (excCompTypes != null)
		{
			SortTypes(ref excCompTypes);

			for (var i = 0; i < excCompTypes.Length; i++)
				hash = HashCode.Combine(hash, excCompTypes[i]);
		}

		if (!_hashToFilter.TryGetValue(hash, out var filter))
		{
			var incCompsPools = new IBlahEcsPool[incCompTypes.Length];
			for (var i = 0; i < incCompsPools.Length; i++)
				incCompsPools[i] = GetPool(incCompTypes[i]);

			IBlahEcsPool[] excCompsPools = null;
			if (excCompTypes == null)
				excCompsPools = Array.Empty<IBlahEcsPool>();
			else
			{
				excCompsPools = new IBlahEcsPool[excCompTypes.Length];
				for (var i = 0; i < excCompsPools.Length; i++)
					excCompsPools[i] = GetPool(excCompTypes[i]);
			}

			filter = new BlahEcsFilter(this, incCompsPools, excCompsPools);
			_filters.Add(filter);
			_hashToFilter[hash] = filter;

			foreach (var type in incCompTypes)
			{
				if (!_incCompToFilters.TryGetValue(type, out var filters))
				{
					filters                 = new List<BlahEcsFilter>();
					_incCompToFilters[type] = filters;
				}
				filters.Add(filter);
			}
			if (excCompTypes != null)
				foreach (var type in excCompTypes)
				{
					if (!_excCompToFilters.TryGetValue(type, out var filters))
					{
						filters                 = new List<BlahEcsFilter>();
						_excCompToFilters[type] = filters;
					}
					filters.Add(filter);
				}
		}

		var proxy = Activator.CreateInstance<T>();
		proxy.Finalize(filter);
		return proxy;
	}


	private BlahEcsPool<T> GetPool<T>() where T : IBlahEntryEcs
	{
		var type = typeof(T);
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			pool = new BlahEcsPool<T>();

			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return (BlahEcsPool<T>)pool;
	}

	private IBlahEcsPool GetPool(Type type)
	{
		if (!_compTypeToPool.TryGetValue(type, out var pool))
		{
			var poolType = typeof(BlahEcsPool<>).MakeGenericType(type);
			pool = (IBlahEcsPool)Activator.CreateInstance(poolType);
			
			_compPools.Add(pool);
			_compTypeToPool[type] = pool;
		}
		return pool;
	}
    
	private void SortTypes(ref Type[] types)
	{
		Array.Sort(
			types,
			(a, b) => a.GetHashCode().CompareTo(b.GetHashCode())
		);
	}
}
}
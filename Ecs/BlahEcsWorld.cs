using System;
using System.Collections.Generic;
using Blah.Common;

namespace Blah.Ecs
{
public class BlahEcsWorld
{
	private BlahSet<BlahEcsEntity> _entitiesSet;
	private int[]                  _aliveEntitiesPtrs;
	private int                    _aliveEntitiesCount;

	private List<IBlahEcsPool>             _compPools      = new();
	private Dictionary<Type, IBlahEcsPool> _compTypeToPool = new();

	private Dictionary<int, BlahEcsFilter> _hashToFilter = new();

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref BlahEcsEntity CreateEntity()
	{
		int ptr = _entitiesSet.Add();
		
		_aliveEntitiesPtrs[_aliveEntitiesCount++] = ptr;

		ref var entity = ref _entitiesSet.Get(ptr);
		entity.World = this;
		entity.Id    = ptr;

		return ref _entitiesSet.Get(ptr);
	}

	public void DestroyEntity(int entityId)
	{
		foreach (var pool in _compPools)
			if (pool.Has(entityId))
				pool.Remove(entityId);
	}

	internal BlahEcsPool<T> GetPool<T>()
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

	internal IBlahEcsPool GetPool(Type type)
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
	
	internal (BlahSet<BlahEcsEntity> set, int[] alivePtrs, int aliveCounts) GetEntities() 
		=> (_entitiesSet, _aliveEntitiesPtrs, _aliveEntitiesCount);


	public BlahEcsFilter GetFilter(Type[] incCompTypes, Type[] excCompTypes)
	{
		int hash = incCompTypes[0].GetHashCode();
		for (var i = 1; i < incCompTypes.Length; i++)
			hash = HashCode.Combine(hash, incCompTypes[i]);
		hash *= 31;
		if (excCompTypes != null)
			for (var i = 0; i < excCompTypes.Length; i++)
				hash = HashCode.Combine(hash, excCompTypes[i]);

		if (!_hashToFilter.TryGetValue(hash, out var filter))
		{
			var incCompPools = new IBlahEcsPool[incCompTypes.Length];
			for (var i = 0; i < incCompPools.Length; i++)
				incCompPools[i] = GetPool(incCompTypes[i]);

			IBlahEcsPool[] excCompPools = null;
			if (excCompTypes == null)
				excCompPools = Array.Empty<IBlahEcsPool>();
			else
			{
				excCompPools = new IBlahEcsPool[excCompTypes.Length];
				for (var i = 0; i < excCompPools.Length; i++)
					excCompPools[i] = GetPool(excCompTypes[i]);
			}

			filter              = new BlahEcsFilter(this, incCompPools, excCompPools);
			_hashToFilter[hash] = filter;
		}
		return filter;
	}
}
}
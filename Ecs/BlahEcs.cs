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

	public void Clear()
	{
		_entities.Clear();
		foreach (var pool in _compPools)
			pool.Clear();
		foreach (var filter in _filters)
			filter.Clear();
	}
	
	

	public BlahEcsFilterCore GetFilterCore(Type[] incCompTypes, Type[] excCompTypes)
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
			var incCompsPools = new IBlahEcsCompInternal[incCompTypes.Length];
			for (var i = 0; i < incCompsPools.Length; i++)
				incCompsPools[i] = GetPool(incCompTypes[i]);

			IBlahEcsCompInternal[] excCompsPools = null;
			if (excCompTypes == null)
				excCompsPools = Array.Empty<IBlahEcsCompInternal>();
			else
			{
				excCompsPools = new IBlahEcsCompInternal[excCompTypes.Length];
				for (var i = 0; i < excCompsPools.Length; i++)
					excCompsPools[i] = GetPool(excCompTypes[i]);
			}

			filter = new BlahEcsFilterCore(_entities, incCompsPools, excCompsPools);
			_filters.Add(filter);
			_hashToFilter[hash] = filter;

			foreach (var type in incCompTypes)
			{
				if (!_incCompToFilters.TryGetValue(type, out var filters))
				{
					filters                 = new List<BlahEcsFilterCore>();
					_incCompToFilters[type] = filters;
				}
				filters.Add(filter);
			}
			if (excCompTypes != null)
				foreach (var type in excCompTypes)
				{
					if (!_excCompToFilters.TryGetValue(type, out var filters))
					{
						filters                 = new List<BlahEcsFilterCore>();
						_excCompToFilters[type] = filters;
					}
					filters.Add(filter);
				}
		}
		return filter;
	}


	public IBlahEcsCompRead<T> GetRead<T>() where T : IBlahEntryEcs
		=> GetPool<T>();

	public IBlahEcsCompWrite<T> GetWrite<T>() where T : IBlahEntryEcs
		=> GetPool<T>();
	
	private BlahEcsPool<T> GetPool<T>() where T : IBlahEntryEcs
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

	private IBlahEcsCompInternal GetPool(Type type)
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
    
	private void SortTypes(ref Type[] types)
	{
		Array.Sort(
			types,
			(a, b) => a.GetHashCode().CompareTo(b.GetHashCode())
		);
	}
}
}
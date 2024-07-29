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
	public BlahEcsFilterCore GetFilterCore(List<Type> incCompTypes, List<Type> excCompTypes)
	{
		incCompTypes.Sort((a, b) => a.GetHashCode().CompareTo(b.GetHashCode()));
		excCompTypes.Sort((a, b) => a.GetHashCode().CompareTo(b.GetHashCode()));
		int hash = incCompTypes[0].GetHashCode();
		for (var i = 1; i < incCompTypes.Count; i++)
			hash = HashCode.Combine(hash, incCompTypes[i]);
		hash *= 31;
		for (var i = 0; i < excCompTypes.Count; i++)
			hash = HashCode.Combine(hash, excCompTypes[i]);

		if (!_hashToFilter.TryGetValue(hash, out var filter))
		{
			var incCompsPools = new IBlahEcsCompInternal[incCompTypes.Count];
			for (var i = 0; i < incCompsPools.Length; i++)
				incCompsPools[i] = GetPool(incCompTypes[i]);

			IBlahEcsCompInternal[] excCompsPools = null;
			if (excCompTypes.Count > 0)
			{
				excCompsPools = new IBlahEcsCompInternal[excCompTypes.Count];
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
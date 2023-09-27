using System;
using Blah.Common;

namespace Blah.Ecs
{
public class BlahEcsFilter
{
	private readonly IBlahEcsPool[] _incCompsPools;
	private readonly IBlahEcsPool[] _excCompsPools;

	private BlahEcsEntity[] _entities = new BlahEcsEntity[1];
	private int             _entitiesCount;

	private int[] _entityIdToIdx = { -1 };

	private DelayedOp[] _delayedOps = new DelayedOp[1];
	private int         _delayedOpsCount;

	private int _goingIteratorsCount;
    
	
	
	public BlahEcsFilter(BlahEcsEntities entities, IBlahEcsPool[] incCompsPools, IBlahEcsPool[] excCompsPools)
	{
		_incCompsPools = incCompsPools;
		_excCompsPools = excCompsPools;

		(var set, int[] alivePtrs, int aliveCount) = entities.GetAllAlive();
		for (var i = 0; i < aliveCount; i++)
		{
			ref var entity = ref set.Get(alivePtrs[i]);
			if (IsSuitable(entity.Id))
				AddEntity(entity);
		}
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private bool Has(int entityId) => entityId < _entityIdToIdx.Length && _entityIdToIdx[entityId] != -1;
    
	
	internal void OnIncCompAddedOrExcRemoved(BlahEcsEntity entity)
	{
		if (_goingIteratorsCount > 0)
		{
			BlahArrayHelper.ResizeOnDemand(ref _delayedOps, _delayedOpsCount);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = entity;
			op.IsTryAdd = true;
			return;
		}
        
		if (!Has(entity.Id) && IsSuitable(entity.Id))
			AddEntity(entity);
	}

	internal void OnIncCompRemovedOrExcAdded(BlahEcsEntity entity)
	{
		if (_goingIteratorsCount > 0)
		{
			BlahArrayHelper.ResizeOnDemand(ref _delayedOps, _delayedOpsCount);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = entity;
			op.IsTryAdd = false;
			return;
		}
		
        TryRemoveEntity(entity.Id);
	}
    

	private bool IsSuitable(int entityId)
	{
		foreach (var pool in _incCompsPools)
			if (!pool.Has(entityId))
				return false;
		foreach (var pool in _excCompsPools)
			if (pool.Has(entityId))
				return false;
		return true;
	}

	private void AddEntity(BlahEcsEntity entity)
	{
		BlahArrayHelper.ResizeOnDemand(ref _entities, _entitiesCount);
		BlahArrayHelper.ResizeOnDemand(ref _entityIdToIdx, entity.Id, -1);

		int idx = _entitiesCount++;
		_entities[idx]            = entity;
		_entityIdToIdx[entity.Id] = idx;
	}

	private void TryRemoveEntity(int entityId)
	{
		if (!Has(entityId))
			return;
		
		int idx = _entityIdToIdx[entityId];
		_entityIdToIdx[entityId] = -1;

		if (_entitiesCount == 1)
		{
			_entitiesCount = 0;
		}
		else
		{
			int lastEntityId = _entities[_entitiesCount - 1].Id;
			_entityIdToIdx[lastEntityId] = idx;
			
			_entities[idx] = _entities[--_entitiesCount];
		}
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private void ApplyDelayedOps()
	{
		for (var i = 0; i < _delayedOpsCount; i++)
		{
			if (_delayedOps[i].IsTryAdd)
				OnIncCompAddedOrExcRemoved(_delayedOps[i].Entity);
			else
				OnIncCompRemovedOrExcAdded(_delayedOps[i].Entity);
		}
		_delayedOpsCount = 0;
	}
	
	private struct DelayedOp
	{
		public bool          IsTryAdd;
		public BlahEcsEntity Entity;
	}
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	internal (BlahEcsEntity[] entities, int entitiesCount) BeginIteration()
	{
		_goingIteratorsCount++;
		return (_entities, _entitiesCount);
	}

	internal void EndIteration()
	{
		if (--_goingIteratorsCount == 0 && _delayedOpsCount > 0)
			ApplyDelayedOps();
	}
}
}
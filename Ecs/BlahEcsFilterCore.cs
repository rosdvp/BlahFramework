using System;
using Blah.Common;
using UnityEngine;

namespace Blah.Ecs
{
public class BlahEcsFilterCore
{
	private readonly IBlahEcsCompInternal[] _incCompsPools;
	private readonly IBlahEcsCompInternal[] _excCompsPools;

	private BlahEcsEntity[] _entities = new BlahEcsEntity[1];
	private int             _entitiesCount;

	private int[] _entityIdToIdx = { -1 };

	private DelayedOp[] _delayedOps = new DelayedOp[1];
	private int         _delayedOpsCount;

	private int _goingIteratorsCount;



	internal BlahEcsFilterCore(BlahEcsEntities        entities, IBlahEcsCompInternal[] incCompsPools,
	                           IBlahEcsCompInternal[] excCompsPools)
	{
		_incCompsPools = incCompsPools;
		_excCompsPools = excCompsPools;

		(var set, int[] alivePtrs, int aliveCount) = entities.GetAllAlive();
		for (var i = 0; i < aliveCount; i++)
		{
			ref var entity = ref set.Get(alivePtrs[i]);
			if (IsSuitable(entity))
				AddEntity(entity);
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private bool Has(BlahEcsEntity ent)
	{
		return ent.Id < _entityIdToIdx.Length && _entityIdToIdx[ent.Id] != -1;
	}


	internal void OnIncCompAddedOrExcRemoved(BlahEcsEntity ent)
	{
		if (_goingIteratorsCount > 0)
		{
			BlahArrayHelper.ResizeOnDemand(ref _delayedOps, _delayedOpsCount);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = ent;
			op.IsTryAdd = true;
			return;
		}
        
		if (!Has(ent) && IsSuitable(ent))
			AddEntity(ent);
	}

	internal void OnIncCompRemovedOrExcAdded(BlahEcsEntity ent)
	{
		if (_goingIteratorsCount > 0)
		{
			BlahArrayHelper.ResizeOnDemand(ref _delayedOps, _delayedOpsCount);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = ent;
			op.IsTryAdd = false;
			return;
		}
		
        TryRemoveEntity(ent);
	}
    

	private bool IsSuitable(BlahEcsEntity ent)
	{
		foreach (var pool in _incCompsPools)
			if (!pool.Has(ent))
				return false;
		foreach (var pool in _excCompsPools)
			if (pool.Has(ent))
				return false;
		return true;
	}

	private void AddEntity(BlahEcsEntity ent)
	{
		BlahArrayHelper.ResizeOnDemand(ref _entities, _entitiesCount);
		BlahArrayHelper.ResizeOnDemand(ref _entityIdToIdx, ent.Id, -1);

		int idx = _entitiesCount++;
		_entities[idx]            = ent;
		_entityIdToIdx[ent.Id] = idx;
	}

	private void TryRemoveEntity(BlahEcsEntity ent)
	{
		if (!Has(ent))
			return;
		
		int idx = _entityIdToIdx[ent.Id];
		_entityIdToIdx[ent.Id] = -1;

		if (idx == _entitiesCount -1)
		{
			_entitiesCount -= 1;
		}
		else
		{
			int lastEntityId = _entities[_entitiesCount - 1].Id;
			_entityIdToIdx[lastEntityId] = idx;
            
			_entities[idx] = _entities[--_entitiesCount];
		}
	}


	internal void Clear()
	{
		_entitiesCount   = 0;
		_delayedOpsCount = 0;

		for (var i = 0; i < _entityIdToIdx.Length; i++)
			_entityIdToIdx[i] = -1;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	internal (BlahEcsEntity[] entities, int entitiesCount) BeginIteration()
	{
		_goingIteratorsCount += 1;
		return (_entities, _entitiesCount);
	}

	internal void EndIteration()
	{
		if (--_goingIteratorsCount == 0 && _delayedOpsCount > 0)
			ApplyDelayedOps();
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
}
}
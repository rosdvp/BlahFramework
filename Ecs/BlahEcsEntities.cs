using System;
using Blah.Common;

namespace Blah.Ecs
{
public class BlahEcsEntities
{
	private readonly BlahSet<BlahEcsEntity> _set;
	
	private int[] _aliveIds;
	private int   _aliveCount;

	private int[] _idToAliveIdx;
	private int[] _idToAliveGen;

	internal BlahEcsEntities(int baseCapacity)
	{
		_set          = new BlahSet<BlahEcsEntity>(baseCapacity, 0);
		_aliveIds     = new int[baseCapacity];
		_idToAliveIdx = new int[baseCapacity];
		_idToAliveGen      = new int[baseCapacity];
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	internal ref BlahEcsEntity Create()
	{
		BlahArrayHelper.ResizeOnDemand(ref _aliveIds, _aliveCount);

		int id       = _set.Add();
		int aliveIdx = _aliveCount++;
		
		_aliveIds[aliveIdx] = id;
		
		BlahArrayHelper.ResizeOnDemand(ref _idToAliveIdx, id);
		_idToAliveIdx[id] = aliveIdx;
		BlahArrayHelper.ResizeOnDemand(ref _idToAliveGen, id);
		_idToAliveGen[id] += 1; 

		ref var entity = ref _set.Get(id);
		entity.Id  = id;
		entity.Gen = _idToAliveGen[id];

		return ref entity;
	}

	internal bool IsAlive(BlahEcsEntity ent)
	{
		return ent.Gen != 0 && ent.Id < _idToAliveGen.Length && ent.Gen == _idToAliveGen[ent.Id];
	}

	internal void Destroy(BlahEcsEntity ent)
	{
		if (!IsAlive(ent))
			return;

		if (_aliveCount == 1)
			_aliveCount = 0;
		else
			_aliveIds[_idToAliveIdx[ent.Id]] = _aliveIds[--_aliveCount];

		_idToAliveGen[ent.Id] += 1;
		
		_set.Remove(ent.Id);
	}

	internal (BlahSet<BlahEcsEntity> entities, int[] aliveIds, int aliveCount) GetAllAlive() 
		=> (_set, _aliveIds, _aliveCount);


	internal void Clear()
	{
		_aliveCount = 0;
		for (var i = 0; i < _idToAliveIdx.Length; i++)
			_idToAliveIdx[i] = -1;
		
		_set.Clear();
	}
}
}
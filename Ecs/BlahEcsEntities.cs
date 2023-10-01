using System;
using Blah.Common;

namespace Blah.Ecs
{
public class BlahEcsEntities
{
	private readonly BlahEcs _ecs;
	
	private readonly BlahSet<BlahEcsEntity> _set = new(1, 0);
	
	private int[] _aliveIds = new int[1];
	private int   _aliveCount;

	private int[] _idToAliveIdx = { -1 };


	public BlahEcsEntities(BlahEcs ecs)
	{
		_ecs = ecs;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref BlahEcsEntity Create()
	{
		BlahArrayHelper.ResizeOnDemand(ref _aliveIds, _aliveCount);

		int id       = _set.Add();
		int aliveIdx = _aliveCount++;
		
		_aliveIds[aliveIdx] = id;
		
		BlahArrayHelper.ResizeOnDemand(ref _idToAliveIdx, id, -1);
		_idToAliveIdx[id] = aliveIdx;

		ref var entity = ref _set.Get(id);
		entity.Ecs = _ecs;
		entity.Id  = id;

		return ref entity;
	}

	public bool IsAlive(int id) => id != -1 && id < _idToAliveIdx.Length && _idToAliveIdx[id] != -1;

	public void Destroy(int id)
	{
		if (!IsAlive(id))
			return;

		if (_aliveCount == 1)
			_aliveCount = 0;
		else
			_aliveIds[_idToAliveIdx[id]] = _aliveIds[--_aliveCount];

		_idToAliveIdx[id] = -1;
		
		_set.Remove(id);
	}

	public (BlahSet<BlahEcsEntity> entities, int[] aliveIds, int aliveCount) GetAllAlive() 
		=> (_set, _aliveIds, _aliveCount);


	public void Clear()
	{
		_aliveCount = 0;
		for (var i = 0; i < _idToAliveIdx.Length; i++)
			_idToAliveIdx[i] = -1;
		
		_set.RemoveAll();
	}
}
}
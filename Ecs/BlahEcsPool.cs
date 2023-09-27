using System;
using Blah.Common;

namespace Blah.Ecs
{
public interface IBlahEcsPool
{
	bool Has(int    entityId);
	void Remove(int entityId);
}

public class BlahEcsPool<T> : IBlahEcsPool where T : IBlahEntryEcs
{
	private readonly BlahSet<T> _set = new(1, 0);

	private int[] _entityIdToPtr = { -1 };

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public void Add(int entityId)
	{
		BlahArrayHelper.ResizeOnDemand(ref _entityIdToPtr, entityId, -1);
		
		int ptr = _set.Add();
		_entityIdToPtr[entityId] = ptr;
	}

	public bool Has(int entityId) => entityId < _entityIdToPtr.Length && _entityIdToPtr[entityId] != -1;

	public ref T Get(int entityId)
	{
		return ref _set.Get(_entityIdToPtr[entityId]);
	}

	public void Remove(int entityId)
	{
		_set.Remove(_entityIdToPtr[entityId]);
		_entityIdToPtr[entityId] = -1;
	}
}
}
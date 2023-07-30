using System;
using Blah.Common;

namespace Blah.Ecs
{
public interface IBlahEcsPool
{
	bool Has(int    entityId);
	void Remove(int entityId);
}

public class BlahEcsPool<T> : IBlahEcsPool
{
	private BlahSet<T> _set = new(1, 1);

	private int[] _entityIdToPtr;

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref T Add(int entityId)
	{
		if (entityId >= _entityIdToPtr.Length)
			Array.Resize(ref _entityIdToPtr, entityId * 2);

		if (_entityIdToPtr[entityId] != 0)
			throw new Exception($"entity {entityId} already has {nameof(T)} component");

		int ptr = _set.Add();
		_entityIdToPtr[entityId] = ptr;
		return ref _set.Get(ptr);
	}

	public bool Has(int entityId) => entityId < _entityIdToPtr.Length && _entityIdToPtr[entityId] != 0;

	public void Remove(int entityId)
	{
		if (entityId >= _entityIdToPtr.Length || _entityIdToPtr[entityId] == 0)
			throw new Exception($"entity {entityId} does not have {nameof(T)}");
		
		_set.Remove(_entityIdToPtr[entityId]);
		_entityIdToPtr[entityId] = 0;
	}
}
}
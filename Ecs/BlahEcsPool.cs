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

	private int[] _entityIdToPtr = new int[2];
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public void Add(int entityId)
	{
		if (entityId >= _entityIdToPtr.Length)
			Array.Resize(ref _entityIdToPtr, entityId * 2);
		
		int ptr = _set.Add();
		_entityIdToPtr[entityId] = ptr;
	}

	public bool Has(int entityId) => entityId < _entityIdToPtr.Length && _entityIdToPtr[entityId] != 0;

	public ref T Get(int entityId)
	{
		return ref _set.Get(_entityIdToPtr[entityId]);
	}

	public void Remove(int entityId)
	{
		_set.Remove(_entityIdToPtr[entityId]);
		_entityIdToPtr[entityId] = 0;
	}
}
}
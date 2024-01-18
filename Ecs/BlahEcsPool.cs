using System;
using Blah.Common;

namespace Blah.Ecs
{
internal interface IBlahEcsCompInternal
{
	bool Has(BlahEcsEntity    entityId);
	void RemoveWithoutCb(BlahEcsEntity entityId);
	void Clear();
}

public interface IBlahEcsCompRead<T> where T: IBlahEntryEcs
{
	public bool Has(BlahEcsEntity  ent);
	public ref T Get(BlahEcsEntity ent);
}

public interface IBlahEcsCompWrite<T> where T: IBlahEntryEcs
{
	public ref T Add(BlahEcsEntity   ent);
	public void Remove(BlahEcsEntity ent);
}

public class BlahEcsPool<T> : 
	IBlahEcsCompInternal,
	IBlahEcsCompRead<T>,
	IBlahEcsCompWrite<T>
	where T : IBlahEntryEcs
{
	private readonly BlahSet<T> _set = new(1, 0);

	private readonly Action<Type, BlahEcsEntity> _cbAdded;
	private readonly Action<Type, BlahEcsEntity> _cbRemoved;
	
	private int[] _entityIdToPtr = { -1 };


	public BlahEcsPool(Action<Type, BlahEcsEntity> cbAdded, Action<Type, BlahEcsEntity> cbRemoved)
	{
		_cbAdded   = cbAdded;
		_cbRemoved = cbRemoved;
	}
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref T Add(BlahEcsEntity ent)
	{
		if (Has(ent))
			throw new Exception($"{ent} already has {typeof(T).Name}");
		
		BlahArrayHelper.ResizeOnDemand(ref _entityIdToPtr, ent.Id, -1);
		
		int ptr = _set.Add();
		_entityIdToPtr[ent.Id] = ptr;
		
		_cbAdded.Invoke(typeof(T), ent);
		
		return ref _set.Get(ptr);
	}

	public bool Has(BlahEcsEntity ent)
	{
		return ent.Id < _entityIdToPtr.Length && _entityIdToPtr[ent.Id] != -1;
	}

	public ref T Get(BlahEcsEntity ent)
	{
		if (!Has(ent))
			throw new Exception($"{ent} does not have {typeof(T).Name}");
		return ref _set.Get(_entityIdToPtr[ent.Id]);
	}

	public void Remove(BlahEcsEntity ent)
	{
		if (!Has(ent))
			throw new Exception($"{ent} does not have {typeof(T).Name}");
		RemoveWithoutCb(ent);
		_cbRemoved.Invoke(typeof(T), ent);
	}

	public void RemoveWithoutCb(BlahEcsEntity ent)
	{
		_set.Remove(_entityIdToPtr[ent.Id]);
		_entityIdToPtr[ent.Id] = -1;
	}

	public void Clear()
	{
		_set.Clear();
		for (var i = 0; i < _entityIdToPtr.Length; i++)
			_entityIdToPtr[i] = -1;
	}
}
}
using System;
using Blah.Common;

namespace Blah.Ecs
{
public interface IBlahCompPoolInternal
{
	bool Has(BlahEnt             ent);
	void RemoveWithoutCb(BlahEnt ent);
	void Clear();
}

public interface IBlahCompGet<T> where T: struct, IBlahEntryComp
{
	public bool Has(BlahEnt        ent);
	public bool Has(BlahEnt?       ent);
	public ref T Get(BlahEnt       ent);
	public ref T Get(BlahEnt?      ent);
	public void Remove(BlahEnt     ent);
	public void Remove(BlahEnt?    ent);
	public bool TryRemove(BlahEnt  ent);
	public bool TryRemove(BlahEnt? ent);
}

public interface IBlahCompFull<T> : IBlahCompGet<T> where T: struct, IBlahEntryComp
{
	public ref T Add(BlahEnt       ent);
	public ref T Add(BlahEnt?      ent);
	public ref T AddOrGet(BlahEnt  ent);
	public ref T AddOrGet(BlahEnt? ent);
}

public class BlahCompPool<T> : 
	IBlahCompPoolInternal,
	IBlahCompGet<T>,
	IBlahCompFull<T>
	where T : struct, IBlahEntryComp
{
	private readonly BlahSet<T> _set = new(1, 0);

	private readonly BlahEntities _entities;
	
	private readonly Action<Type, BlahEnt> _cbAdded;
	private readonly Action<Type, BlahEnt> _cbRemoved;
	
	private int[] _entityIdToPtr = { -1 };


	public BlahCompPool(BlahEntities             entities,
	                   Action<Type, BlahEnt> cbAdded,
	                   Action<Type, BlahEnt> cbRemoved)
	{
		_entities  = entities;
		_cbAdded   = cbAdded;
		_cbRemoved = cbRemoved;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref T Add(BlahEnt ent)
	{
		if (Has(ent))
			throw new Exception($"{ent} already has {typeof(T).Name}");
		if (!_entities.IsAlive(ent))
			throw new Exception($"{ent} is not alive");
		
		BlahArrayHelper.ResizeOnDemand(ref _entityIdToPtr, ent.Id, -1);
		
		int ptr = _set.Add();
		_entityIdToPtr[ent.Id] = ptr;
		
		_cbAdded.Invoke(typeof(T), ent);

		ref var comp = ref _set.Get(ptr);
		comp = default;
		return ref comp;
	}

	public ref T Add(BlahEnt? ent)
	{
		if (ent == null)
			throw new Exception($"ent is null");
		return ref Add(ent.Value);
	}

	
	public ref T AddOrGet(BlahEnt ent)
	{
		if (Has(ent))
			return ref Get(ent);
		return ref Add(ent);
	}

	public ref T AddOrGet(BlahEnt? ent)
	{
		if (ent == null)
			throw new Exception($"ent is null");
		return ref AddOrGet(ent.Value);
	}
	

	public bool Has(BlahEnt ent)
	{
		return ent.Id < _entityIdToPtr.Length
		       && _entityIdToPtr[ent.Id] != -1
		       && _entities.IsAlive(ent);
	}

	public bool Has(BlahEnt? ent)
	{
		return ent != null && Has(ent.Value);
	}

	public ref T Get(BlahEnt ent)
	{
		if (!Has(ent))
			throw new Exception($"{ent} does not have {typeof(T).Name}");
		return ref _set.Get(_entityIdToPtr[ent.Id]);
	}

	public ref T Get(BlahEnt? ent)
	{
		if (ent == null)
			throw new Exception($"ent is null");
		return ref Get(ent.Value);
	}

	public void Remove(BlahEnt ent)
	{
		if (!Has(ent))
		{
			if (_entities.IsAlive(ent))
				throw new Exception($"{ent} does not have {typeof(T).Name}");
			throw new Exception($"{ent} is not alive");
		}
		RemoveWithoutCb(ent);
		_cbRemoved.Invoke(typeof(T), ent);
	}

	public void Remove(BlahEnt? ent)
	{
		if (ent == null)
			throw new Exception($"ent is null");
		Remove(ent.Value);
	}

	public bool TryRemove(BlahEnt ent)
	{
		if (!Has(ent))
			return false;
		Remove(ent);
        return true;
	}

	public bool TryRemove(BlahEnt? ent)
	{
		return ent != null && TryRemove(ent.Value);
	}

	public void RemoveWithoutCb(BlahEnt ent)
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
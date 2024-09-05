using System;
using Blah.Common;

namespace Blah.Ecs
{
internal interface IBlahEcsCompInternal
{
	bool Has(BlahEcsEntity             ent);
	void RemoveWithoutCb(BlahEcsEntity ent);
	void Clear();
}

public interface IBlahEcsGet<T> where T : IBlahEntryEcs
{
	public bool Has(BlahEcsEntity       ent);
	public ref T Get(BlahEcsEntity      ent);
	public void Remove(BlahEcsEntity    ent);
	public bool TryRemove(BlahEcsEntity ent);
}

public interface IBlahEcsFull<T> where T : IBlahEntryEcs
{
	public ref T Add(BlahEcsEntity      ent);
	public bool Has(BlahEcsEntity       ent);
	public ref T Get(BlahEcsEntity      ent);
	public void Remove(BlahEcsEntity    ent);
	public bool TryRemove(BlahEcsEntity ent);
}

public readonly struct BlahEcsGet<T> where T : IBlahEntryEcs
{
	private readonly BlahEcsPool<T> _pool;

	internal BlahEcsGet(BlahEcsPool<T> pool)
	{
		_pool = pool;
	}

	public bool Has(BlahEcsEntity       ent) => _pool.Has(ent);
	public ref T Get(BlahEcsEntity      ent) => ref _pool.Get(ent);
	public void Remove(BlahEcsEntity    ent) => _pool.Remove(ent);
	public bool TryRemove(BlahEcsEntity ent) => _pool.TryRemove(ent);


	public static implicit operator BlahEcsGet<T>(BlahEcsFilter.Include  inc) => new(inc.Get<T>());
	public static implicit operator BlahEcsGet<T>(BlahEcsFilter.Optional opt) => new(opt.Get<T>());
	public static implicit operator BlahEcsGet<T>(BlahEcsFilter.Exclude  exc) => new(exc.Get<T>());

#if BLAH_TESTS
	public object TestsPool => _pool;
#endif
}

public readonly struct BlahEcsFull<T> where T : IBlahEntryEcs
{
	private readonly BlahEcsPool<T> _pool;

	internal BlahEcsFull(BlahEcsPool<T> pool)
	{
		_pool = pool;
	}

	public ref T Add(BlahEcsEntity ent) => ref _pool.Add(ent);

	public bool Has(BlahEcsEntity  ent) => _pool.Has(ent);
	public ref T Get(BlahEcsEntity ent) => ref _pool.Get(ent);

	public void Remove(BlahEcsEntity    ent) => _pool.Remove(ent);
	public bool TryRemove(BlahEcsEntity ent) => _pool.TryRemove(ent);
}

internal class BlahEcsPool<T> : IBlahEcsCompInternal,
	IBlahEcsGet<T>,
	IBlahEcsFull<T>
	where T : IBlahEntryEcs
{
	private readonly BlahSet<T> _set = new(1, 0);

	private readonly BlahEcsEntities _entities;
	
	private readonly Action<Type, BlahEcsEntity> _cbAdded;
	private readonly Action<Type, BlahEcsEntity> _cbRemoved;
	
	private int[] _entityIdToPtr = { -1 };


	public BlahEcsPool(BlahEcsEntities             entities,
	                   Action<Type, BlahEcsEntity> cbAdded,
	                   Action<Type, BlahEcsEntity> cbRemoved)
	{
		_entities  = entities;
		_cbAdded   = cbAdded;
		_cbRemoved = cbRemoved;
	}
	
	public void Clear()
	{
		_set.Clear();
		for (var i = 0; i < _entityIdToPtr.Length; i++)
			_entityIdToPtr[i] = -1;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public ref T Add(BlahEcsEntity ent)
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

	public bool Has(BlahEcsEntity ent)
	{
		return ent.Id < _entityIdToPtr.Length
		       && _entityIdToPtr[ent.Id] != -1
		       && _entities.IsAlive(ent);
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
		{
			if (_entities.IsAlive(ent))
				throw new Exception($"{ent} does not have {typeof(T).Name}");
			throw new Exception($"{ent} is not alive");
		}
		RemoveWithoutCb(ent);
		_cbRemoved.Invoke(typeof(T), ent);
	}

	public bool TryRemove(BlahEcsEntity ent)
	{
		if (!Has(ent))
			return false;
		Remove(ent);
        return true;
	}

	public void RemoveWithoutCb(BlahEcsEntity ent)
	{
		_set.Remove(_entityIdToPtr[ent.Id]);
		_entityIdToPtr[ent.Id] = -1;
	}
}
}
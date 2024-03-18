using System;

namespace Blah.Ecs
{
public class BlahEcsFilter : IEquatable<BlahEcsFilter>
{
	private BlahEcsFilterCore _filter;

	public void Set(BlahEcsFilterCore filter)
	{
		_filter = filter;
	}

	public bool IsEmpty => _filter.IsEmpty;

	public BlahEcsEntity GetAny() => _filter.GetAny();

	public bool TryGetAny(out BlahEcsEntity ent) => _filter.TryGetAny(out ent);
	
	
	public Enumerator GetEnumerator() => new(_filter);

	public struct Enumerator : IDisposable
	{
		private readonly BlahEcsFilterCore _owner;
		private readonly BlahEcsEntity[]   _entities;
		private readonly int               _entitiesCount;

		private int _cursor;

		public Enumerator(BlahEcsFilterCore owner)
		{
			_owner                      = owner;
			(_entities, _entitiesCount) = owner.BeginIteration();
			_cursor = -1;
		}

		public BlahEcsEntity Current => _entities[_cursor];

		public bool MoveNext() => ++_cursor < _entitiesCount;

		public void Dispose()
		{
			_owner.EndIteration();
		}
	}

	public static bool operator ==(BlahEcsFilter a, BlahEcsFilter b)
		=> a._filter == b._filter;
	public static bool operator !=(BlahEcsFilter a, BlahEcsFilter b)
		=> a._filter != b._filter;
	
	public bool Equals(BlahEcsFilter other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return Equals(_filter, other._filter);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != this.GetType())
			return false;
		return Equals((BlahEcsFilter)obj);
	}

	public override int GetHashCode()
	{
		return (_filter != null ? _filter.GetHashCode() : 0);
	}
}

internal interface IBlahEcsFilterExc { }

public class BlahEcsFilter<T0> : BlahEcsFilter
{
	public class Exc<E0> : BlahEcsFilter<T0>, IBlahEcsFilterExc { }
}

public class BlahEcsFilter<T0, T1> : BlahEcsFilter<T0>
{
	public new class Exc<E0> : BlahEcsFilter<T0, T1>, IBlahEcsFilterExc { }
}

public class BlahEcsFilter<T0, T1, T2> : BlahEcsFilter<T0, T1>
{
	public new class Exc<E0> : BlahEcsFilter<T0, T1, T2>, IBlahEcsFilterExc { }
}

public class BlahEcsFilter<T0, T1, T2, T3> : BlahEcsFilter<T0, T1, T2>
{
	public new class Exc<E0> : BlahEcsFilter<T0, T1, T2, T3>, IBlahEcsFilterExc { }
}
}
using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public abstract class BlahFilter : IEquatable<BlahFilter>
{
	private BlahFilterCore _core;

	internal void Set(BlahFilterCore core)
	{
		_core = core;
	}

	public bool IsEmpty => _core.IsEmpty;

	public BlahEnt GetAny() => _core.GetAny();

	public bool TryGetAny(out BlahEnt ent) => _core.TryGetAny(out ent);


	public Enumerator GetEnumerator() => new(_core);

	public struct Enumerator : IDisposable
	{
		private readonly BlahFilterCore _owner;
		private readonly BlahEnt[]      _entities;
		private readonly int            _entitiesCount;

		private int _cursor;

		public Enumerator(BlahFilterCore owner)
		{
			_owner                      = owner;
			(_entities, _entitiesCount) = owner.BeginIteration();
			_cursor                     = -1;
		}

		public BlahEnt Current => _entities[_cursor];

		public bool MoveNext() => ++_cursor < _entitiesCount;

		public void Dispose()
		{
			_owner.EndIteration();
		}
	}

	public static bool operator ==(BlahFilter a, BlahFilter b)
		=> a?._core == b?._core;

	public static bool operator !=(BlahFilter a, BlahFilter b)
		=> a?._core != b?._core;

	public bool Equals(BlahFilter other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return Equals(_core, other._core);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		return Equals((BlahFilter)obj);
	}

	public override int GetHashCode()
	{
		return (_core != null ? _core.GetHashCode() : 0);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	internal static List<Type> BuilderIncCompsTypes = new();
	internal static List<Type> BuilderExcCompsTypes = new();

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public BlahFilterCore TestsCore => _core;
#endif
}

public class BlahFilter<T0> : BlahFilter
{
	public BlahFilter()
	{
		BuilderIncCompsTypes.Add(typeof(T0));
	}

	public class Exc<E0> : BlahFilter<T0>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
		}
	}

	public class Exc<E0, E1> : BlahFilter<T0>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
			BuilderExcCompsTypes.Add(typeof(E1));
		}
	}
}

public class BlahFilter<T0, T1> : BlahFilter
{
	public BlahFilter()
	{
		BuilderIncCompsTypes.Add(typeof(T0));
		BuilderIncCompsTypes.Add(typeof(T1));
	}

	public class Exc<E0> : BlahFilter<T0, T1>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
		}
	}

	public class Exc<E0, E1> : BlahFilter<T0, T1>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
			BuilderExcCompsTypes.Add(typeof(E1));
		}
	}
}

public class BlahFilter<T0, T1, T2> : BlahFilter
{
	public BlahFilter()
	{
		BuilderIncCompsTypes.Add(typeof(T0));
		BuilderIncCompsTypes.Add(typeof(T1));
		BuilderIncCompsTypes.Add(typeof(T2));
	}
	
	public class Exc<E0> : BlahFilter<T0, T1, T2>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
		}
	}

	public class Exc<E0, E1> : BlahFilter<T0, T1, T2>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
			BuilderExcCompsTypes.Add(typeof(E1));
		}
	}
}

public class BlahFilter<T0, T1, T2, T3> : BlahFilter
{
	public BlahFilter()
	{
		BuilderIncCompsTypes.Add(typeof(T0));
		BuilderIncCompsTypes.Add(typeof(T1));
		BuilderIncCompsTypes.Add(typeof(T2));
		BuilderIncCompsTypes.Add(typeof(T3));
	}
	
	public class Exc<E0> : BlahFilter<T0, T1, T2, T3>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
		}
	}

	public class Exc<E0, E1> : BlahFilter<T0, T1, T2, T3>
	{
		public Exc()
		{
			BuilderExcCompsTypes.Add(typeof(E0));
			BuilderExcCompsTypes.Add(typeof(E1));
		}
	}
}
}
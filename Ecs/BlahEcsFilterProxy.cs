using System;
using UnityEngine;

namespace Blah.Ecs
{
public class BlahEcsFilterProxy
{
	private BlahEcsFilter _filter;
    
	
	public void Finalize(BlahEcsFilter filter) => _filter = filter;

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_filter);
	}


	public bool IsSame(BlahEcsFilterProxy filter) => _filter == filter._filter;
	
	
	public struct Enumerator : IDisposable
	{
		private readonly BlahEcsFilter   _owner;
		private readonly BlahEcsEntity[] _entities;
		private readonly int             _entitiesCount;

		private int _cursor;

		public Enumerator(BlahEcsFilter owner)
		{
			_owner  = owner;
			_cursor = 0; // entities starts from 1, but we need 0 for first MoveNext
			
			(_entities, _entitiesCount) = _owner.BeginIteration();
		}
		
		public BlahEcsEntity Current => _entities[_cursor];

		public bool MoveNext() => ++_cursor < _entitiesCount;

		public void Dispose()
		{
			_owner.EndIteration();
		}
	}
}

    
public class BlahEcsFilter<T0> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}

public class BlahEcsFilter<T0, T1, T2> : BlahEcsFilterProxy
{
	//public class Exc<TE0> : BlahEcsFilter { }
}
}
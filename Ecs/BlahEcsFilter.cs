using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public abstract class BlahEcsFilter
{
	private BlahEcsFilterCore _core;

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public bool IsEmpty => _core.IsEmpty;

	public BlahEcsEntity GetAny() => _core.GetAny();

	public bool TryGetAny(out BlahEcsEntity ent) => _core.TryGetAny(out ent);

	public BlahEcsFilterCore.Enumerator GetEnumerator() => new(_core);


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	protected static Include  Inc => new();
	protected static Optional Opt => new();
	protected static Exclude  Exc => new();


	public readonly ref struct Include
	{
		internal BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
		{
			_maskInc.Add(typeof(T));
			return _ecs.GetPool<T>();
		}
	}

	public readonly ref struct Optional
	{
		internal BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
		{
			return _ecs.GetPool<T>();
		}
	}

	public readonly ref struct Exclude
	{
		internal BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
		{
			_maskExc.Add(typeof(T));
			return _ecs.GetPool<T>();
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private static BlahEcs    _ecs;
	private static List<Type> _maskInc = new();
	private static List<Type> _maskExc = new();


	internal static T Create<T>(BlahEcs ecs) where T : BlahEcsFilter, new()
	{
		_maskInc.Clear();
		_maskExc.Clear();

		_ecs = ecs;
		var filter = new T();
		_ecs = null;

		if (_maskInc.Count == 0)
			throw new Exception($"{typeof(T).Name} does not have Inc pools");

		filter._core = ecs.GetFilterCore(_maskInc, _maskExc);
		return filter;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public BlahEcsFilterCore TestsCore => _core;
#endif
}
}
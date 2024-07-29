using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public struct TestComp : IBlahEntryEcs { }

public class TestFilter : BlahEcsFilter
{
	public BlahEcsGet<TestComp> A = Inc;
	public BlahEcsGet<TestComp> B = Opt;
	public BlahEcsGet<TestComp> C = Exc;
}

public abstract class BlahEcsFilter : IEquatable<BlahEcsFilter>
{
	private BlahEcsFilterCore _core;

	public void SetCore(BlahEcsFilterCore core)
	{
		_core = core;
	}

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
		public BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
		{
			_maskInc.Add(typeof(T));
			return _ecs.GetPool<T>();
		}
	}

	public readonly ref struct Optional
	{
		public BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
		{
			return _ecs.GetPool<T>();
		}
	}
    
	public readonly ref struct Exclude
	{
		public BlahEcsPool<T> Get<T>() where T : IBlahEntryEcs
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


	public static T Create<T>(BlahEcs ecs) where T: BlahEcsFilter, new()
	{
		_maskInc.Clear();
		_maskExc.Clear();
		
		_ecs = ecs;
		var filter = new T();
		_ecs = null;
		
		var core   = ecs.GetFilterCore(_maskInc, _maskExc);
		filter.SetCore(core);
		return filter;
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	
	public static bool operator ==(BlahEcsFilter a, BlahEcsFilter b)
		=> a?._core == b?._core;
	public static bool operator !=(BlahEcsFilter a, BlahEcsFilter b)
		=> a?._core != b?._core;
	
	public bool Equals(BlahEcsFilter other)
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
		if (obj.GetType() != this.GetType())
			return false;
		return Equals((BlahEcsFilter)obj);
	}

	public override int GetHashCode()
	{
		return (_core != null ? _core.GetHashCode() : 0);
	}
}
}
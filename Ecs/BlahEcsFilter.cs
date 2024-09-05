using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blah.Ecs
{
public struct BlahEcsFilterExc<T> where T : IBlahEntryEcs { }

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
	private static List<IBlahEcsCompInternal> _incPools = new();
	private static List<IBlahEcsCompInternal> _excPools = new();


	internal static T Create<T>(BlahEcs ecs) where T : BlahEcsFilter, new()
	{
		_incPools.Clear();
		_excPools.Clear();

		var filter     = new T();
		var filterType = typeof(T);
		foreach (var field in filterType.GetFields(BindingFlags.Instance
		                                           | BindingFlags.Public
		                                           | BindingFlags.NonPublic
		         ))
		{
			if (!field.FieldType.IsGenericType)
				continue;
			var genDef = field.FieldType.GetGenericTypeDefinition();
			var genArg = field.FieldType.GetGenericArguments()[0];
			if (genDef == typeof(IBlahEcsGet<>) || genDef == typeof(IBlahEcsFull<>))
			{
				var pool = ecs.GetPool(genArg);
				_incPools.Add(pool);
				field.SetValue(filter, pool);
			}
			else if (genDef == typeof(BlahEcsFilterExc<>))
			{
				var pool = ecs.GetPool(genArg);
				_excPools.Add(pool);
			}
		}
		if (_incPools.Count == 0)
			throw new Exception($"{typeof(T).Name} does not have Inc pools");

		filter._core = ecs.GetFilterCore(_incPools, _excPools);
		return filter;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public BlahEcsFilterCore TestsCore => _core;
#endif
}
}
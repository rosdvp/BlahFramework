using System;
using System.Collections.Generic;

namespace Blah.Ecs
{
public class BlahEcsInjectSource
{
	private readonly BlahEcs _ecs;

	private readonly HashSet<Type> _tempSet  = new();
	private readonly List<Type> _tempList = new();
	
	public BlahEcsInjectSource(BlahEcs ecs)
	{
		_ecs = ecs;
	}

	public BlahEcs GetEcs() => _ecs;

	public IBlahEcsCompRead<T> GetRead<T>() where T : IBlahEntryEcs
		=> _ecs.GetRead<T>();

	public IBlahEcsCompWrite<T> GetWrite<T>() where T : IBlahEntryEcs
		=> _ecs.GetWrite<T>();
		
	public object GetFilter<T>() where T : BlahEcsFilter, new()
	{
		Type[] incCompsTypes = null;
		Type[] excCompsTypes = null;

		var type = typeof(T);
		if (typeof(IBlahEcsFilterExc).IsAssignableFrom(type))
		{
			incCompsTypes = type.BaseType.GenericTypeArguments;
			foreach (var incCompType in incCompsTypes)
				_tempSet.Add(incCompType);
			foreach (var excCompType in type.GenericTypeArguments)
				if (!_tempSet.Contains(excCompType))
					_tempList.Add(excCompType);
			
			excCompsTypes = _tempList.ToArray();
			
			_tempSet.Clear();
			_tempList.Clear();
		}
		else
		{
			incCompsTypes = type.GenericTypeArguments;
		}

		var core   = _ecs.GetFilterCore(incCompsTypes, excCompsTypes);
		var filter = new T();
		filter.Set(core);
		return filter;
	}
}
}
using System;

namespace Blah.Ecs
{
public class BlahEcsInjectSource
{
	private readonly BlahEcs _ecs;
	
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
	
		var type           = typeof(T);
		if (typeof(IBlahEcsFilterExc).IsAssignableFrom(type))
		{
			excCompsTypes = type.GenericTypeArguments;
			incCompsTypes = type.BaseType.GenericTypeArguments;
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
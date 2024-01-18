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
		var type          = typeof(T);
		var incCompsTypes = type.GenericTypeArguments;

		var core   = _ecs.GetFilterCore(incCompsTypes, null);
		var filter = new T();
		filter.Set(core);
		return filter;
	}
}
}
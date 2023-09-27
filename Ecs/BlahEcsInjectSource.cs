namespace Blah.Ecs
{
public class BlahEcsInjectSource
{
	private readonly BlahEcs _world;
	
	public BlahEcsInjectSource(BlahEcs world)
	{
		_world = world;
	}

	public BlahEcs GetWorld() => _world;

	public object GetFilter<T>() where T : BlahEcsFilterProxy
	{
		var type          = typeof(T);
		var incCompsTypes = type.GenericTypeArguments;
		var filter        = _world.GetFilter<T>(incCompsTypes, null);

		return filter;
	}
}
}
namespace Blah.Ecs
{
public class BlahEcsInjectSource
{
	private readonly BlahEcsWorld _world;
	
	public BlahEcsInjectSource(BlahEcsWorld world)
	{
		_world = world;
	}

	public BlahEcsWorld GetWorld() => _world;

	public object GetFilter<T>() where T : BlahEcsFilterProxy
	{
		var type          = typeof(T);
		var incCompsTypes = type.GenericTypeArguments;
		var filter        = _world.GetFilter<T>(incCompsTypes, null);

		return filter;
	}
}
}
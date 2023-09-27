namespace Blah.Ecs
{
public struct BlahEcsEntity
{
	internal BlahEcsWorld World;
	internal int          Id;

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	public void Destroy() => World.DestroyEntity(Id);

	public ref T Add<T>() => ref World.AddComp<T>(this);

	public void Remove<T>() => World.RemoveComp<T>(this);

	public ref T Get<T>() => ref World.GetComp<T>(this);

	public bool Has<T>() => World.HasComp<T>(this);
}
}
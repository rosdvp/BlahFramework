namespace Blah.Ecs
{
public struct BlahEcsEntity
{
	internal BlahEcsWorld World;
	internal int          Id;

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	public void Destroy() => World.DestroyEntity(Id);

	public ref T Add<T>() where T : IBlahEntryEcs => ref World.AddComp<T>(this);

	public void Remove<T>() where T : IBlahEntryEcs => World.RemoveComp<T>(this);

	public ref T Get<T>() where T : IBlahEntryEcs => ref World.GetComp<T>(this);

	public bool Has<T>() where T : IBlahEntryEcs => World.HasComp<T>(this);
}
}
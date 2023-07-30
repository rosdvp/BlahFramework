namespace Blah.Ecs
{
public struct BlahEcsEntity
{
	internal BlahEcsWorld World;
	internal int          Id;

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	public void Destroy() => World.DestroyEntity(Id);
	
	public ref T AddComp<T>() => ref World.GetPool<T>().Add(Id);

	public void RemoveComp<T>() => World.GetPool<T>().Remove(Id);

	public bool HasComp<T>() => World.GetPool<T>().Has(Id);
}
}
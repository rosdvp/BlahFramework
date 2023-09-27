namespace Blah.Ecs
{
public struct BlahEcsEntity
{
	internal BlahEcs Ecs;
	internal int     Id;

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	public void Destroy() => Ecs.DestroyEntity(this);

	public ref T Add<T>() where T : IBlahEntryEcs => ref Ecs.AddComp<T>(this);

	public void Remove<T>() where T : IBlahEntryEcs => Ecs.RemoveComp<T>(this);

	public ref T Get<T>() where T : IBlahEntryEcs => ref Ecs.GetComp<T>(this);

	public bool Has<T>() where T : IBlahEntryEcs => Ecs.HasComp<T>(this);



	public static bool operator ==(BlahEcsEntity a, BlahEcsEntity b)
	{
		return a.Id == b.Id;
	}

	public static bool operator !=(BlahEcsEntity a, BlahEcsEntity b)
	{
		return a.Id != b.Id;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public static BlahEcsEntity Null { get; } = new()
	{
		Ecs = null, Id = -1
	};
}
}
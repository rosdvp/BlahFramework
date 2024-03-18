using System;

namespace Blah.Ecs
{
public struct BlahEcsEntity
{
	internal int Id;
	internal int Gen;

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public static bool operator ==(BlahEcsEntity a, BlahEcsEntity b)
	{
		return a.Id == b.Id && a.Gen == b.Gen;
	}

	public static bool operator !=(BlahEcsEntity a, BlahEcsEntity b)
	{
		return a.Id != b.Id || a.Gen != b.Gen;
	}
    
	public bool Equals(BlahEcsEntity other) => Id == other.Id && Gen == other.Gen;

	public override bool Equals(object obj) => obj is BlahEcsEntity other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Gen.GetHashCode() + Id.GetHashCode());
    

	public override string ToString()
	{
		return $"{Id}-{Gen}";
	}
}
}
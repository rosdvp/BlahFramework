using System;

namespace Blah.Ecs
{
public struct BlahEnt
{
	internal int Id;
	internal int Gen;

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public static bool operator ==(BlahEnt a, BlahEnt b)
	{
		return a.Id == b.Id && a.Gen == b.Gen;
	}

	public static bool operator !=(BlahEnt a, BlahEnt b)
	{
		return a.Id != b.Id || a.Gen != b.Gen;
	}

	public static bool operator <(BlahEnt a, BlahEnt b)
	{
		return a.Gen < b.Gen || (a.Gen == b.Gen && a.Id < b.Id);
	}
	
	public static bool operator >(BlahEnt a, BlahEnt b)
	{
		return a.Gen > b.Gen || (a.Gen == b.Gen && a.Id > b.Id);
	}
    
	public bool Equals(BlahEnt other) => Id == other.Id && Gen == other.Gen;

	public override bool Equals(object obj) => obj is BlahEnt other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Gen.GetHashCode() + Id.GetHashCode());
    

	public override string ToString()
	{
		return $"{Id}-{Gen}";
	}
}
}
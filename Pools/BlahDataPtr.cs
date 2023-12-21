using System;

namespace Blah.Pools
{
public struct BlahDataPtr : IEquatable<BlahDataPtr>
{
	internal Type DataType;
	internal int  Gen;
	internal int  EntryPtr;


	public static bool operator ==(BlahDataPtr a, BlahDataPtr b)
	{
		return a.DataType == b.DataType && a.EntryPtr == b.EntryPtr && a.Gen == b.Gen;
	}

	public static bool operator !=(BlahDataPtr a, BlahDataPtr b)
	{
		return !(a == b);
	}
    
	public bool Equals(BlahDataPtr other)
	{
		return DataType == other.DataType && Gen == other.Gen && EntryPtr == other.EntryPtr;
	}

	public override bool Equals(object obj)
	{
		return obj is BlahDataPtr other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(DataType, Gen, EntryPtr);
	}
}
}
using System;

namespace Blah.Ordering.Attributes
{
public class BlahAfterAttribute : Attribute
{
	public readonly Type PrevSystem;

	public BlahAfterAttribute(Type prevSystem)
	{
		PrevSystem = prevSystem;
	}
}
}
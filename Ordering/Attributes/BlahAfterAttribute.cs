using System;

namespace Blah.Ordering.Attributes
{
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class BlahAfterAttribute : Attribute
{
	public readonly Type PrevSystem;

	public BlahAfterAttribute(Type prevSystem)
	{
		PrevSystem = prevSystem;
	}
}
}
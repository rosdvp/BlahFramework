using System;

namespace Blah.Ordering.Attributes
{
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class BlahBeforeAttribute : Attribute
{
	public readonly Type NextSystem;

	public BlahBeforeAttribute(Type nextSystem)
	{
		NextSystem = nextSystem;
	}
}
}
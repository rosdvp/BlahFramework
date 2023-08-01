using System;

namespace Blah.Ordering.Attributes
{
public class BlahBeforeAttribute : Attribute
{
	public readonly Type NextSystem;

	public BlahBeforeAttribute(Type nextSystem)
	{
		NextSystem = nextSystem;
	}
}
}
using System;

namespace Blah.Ordering.Attributes
{
public class BlahBeforeAllAttribute : Attribute
{
	public readonly int Priority;

	public BlahBeforeAllAttribute(int priority = 0)
	{
		Priority = priority;
	}
}
}
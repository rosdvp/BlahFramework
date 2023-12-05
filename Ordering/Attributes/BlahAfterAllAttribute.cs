using System;

namespace Blah.Ordering.Attributes
{
public class BlahAfterAllAttribute : Attribute
{
	public readonly int Priority;

	public BlahAfterAllAttribute(int priority = 0)
	{
		Priority = priority;
	}
}
}
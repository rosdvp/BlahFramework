using System;

namespace Blah.Ordering.Attributes
{
public class BlahAfterAllAttribute : Attribute
{
	public readonly int Priority;

	public BlahAfterAllAttribute(int priority = 0)
	{
		if (priority < 0)
			throw new Exception("priority must be >= 0");
		
		Priority = priority;
	}
}
}
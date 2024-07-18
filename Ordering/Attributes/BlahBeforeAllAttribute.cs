using System;

namespace Blah.Ordering.Attributes
{
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlahBeforeAllAttribute : Attribute
{
	public readonly int Priority;

	public BlahBeforeAllAttribute(int priority = 0)
	{
		if (priority < 0)
			throw new Exception("priority must be >= 0");
		
		Priority = priority;
	}
}
}
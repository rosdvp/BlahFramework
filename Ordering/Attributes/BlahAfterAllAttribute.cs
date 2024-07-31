using System;

namespace Blah.Ordering.Attributes
{
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlahAfterAllAttribute : Attribute
{
	public readonly uint Priority;

	public BlahAfterAllAttribute(uint priority = 0)
	{
		Priority = priority;
	}
}
}
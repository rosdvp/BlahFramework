using System;

namespace Blah.Ordering.Attributes
{
public class BlahAfterAttribute : Attribute
{
	public readonly Type SystemGoingBefore;

	public BlahAfterAttribute(Type systemGoingBefore)
	{
		SystemGoingBefore = systemGoingBefore;
	}
}
}
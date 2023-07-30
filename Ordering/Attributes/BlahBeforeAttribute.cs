using System;

namespace Blah.Ordering.Attributes
{
public class BlahBeforeAttribute : Attribute
{
	public readonly Type SystemGoingAfter;

	public BlahBeforeAttribute(Type systemGoingAfter)
	{
		SystemGoingAfter = systemGoingAfter;
	}
}
}
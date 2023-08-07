using System;
using System.Collections.Generic;

namespace Blah.Features
{
public abstract class BlahFeatureBase
{
	public abstract HashSet<Type> ConsumingFromOutside { get; }

	public abstract HashSet<Type> Producing { get; }

	public abstract HashSet<Type> Services { get; }

	public abstract IReadOnlyList<Type> Systems { get; }
}
}

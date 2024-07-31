using System;
using System.Collections.Generic;
using Blah.Systems;

namespace Blah.Features
{
public abstract class BlahFeatureBase
{
	public abstract IReadOnlyList<IBlahSystem> Systems { get; }
}
}

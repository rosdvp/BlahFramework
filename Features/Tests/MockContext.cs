using System.Collections.Generic;

namespace Blah.Features.Tests
{
internal class MockContext : BlahContextBase
{
	protected override Dictionary<int, List<BlahFeatureBase>> FeaturesGroups { get; } = new()
	{
		{
			0, new List<BlahFeatureBase>
			{
				new MockFeatureA(),
				new MockFeatureB()
			}
		}
	};
}
}
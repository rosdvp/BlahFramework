using System.Collections.Generic;

namespace Blah.Features.Tests
{
internal class MockContext : BlahContext
{
	protected override Dictionary<int, List<BlahFeatureBase>> FeaturesBySystemsGroups { get; } = new()
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
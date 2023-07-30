using System.Collections.Generic;

namespace Blah.Features.Tests
{
internal class MockContext : BlahContext
{
	protected override IReadOnlyList<BlahFeatureBase> Features { get; } = new BlahFeatureBase[]
	{
		new MockFeatureB(),
		new MockFeatureA()
	};
}
}
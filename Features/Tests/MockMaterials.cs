using Blah.Pools;
using Blah.Services;

namespace Blah.Features.Tests
{
internal struct MockCmdA : IBlahEntrySignal
{
	public int Val;
}

internal struct MockEventA : IBlahEntrySignal
{
	public int Val;
}

internal struct MockDataB : IBlahEntryData
{
	public int Val;
}


internal class MockServiceA : BlahServiceBase
{
	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services) { }
}

internal class MockServiceB : BlahServiceBase
{
	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services) { }
}
}
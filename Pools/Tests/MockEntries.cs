namespace Blah.Pools.Tests
{
internal interface IMockEntry
{
	public int Value { get; }
}

internal struct MockDataEntry : IBlahEntryData, IMockEntry
{
	public int Val;

	public int Value => Val;
}

internal struct MockSignalEntry : IBlahEntrySignal, IMockEntry
{
	public int Val;

	public int Value => Val;
}

internal struct MockSignalNextFrameEntry : IBlahEntryNextFrameSignal, IMockEntry
{
	public int Val;

	public int Value => Val;
}
}
namespace Blah.Systems
{
public enum EGroupSwitchMode
{
	/// <summary>
	/// The switch will happen immediately.
	/// </summary>
	SwitchNow,
	/// <summary>
	/// The switch will happen at the beginning of next <see cref="BlahSystemsContext{TGroupId}.Run"/> method.
	/// </summary>
	SwitchBeforeNextRun
}
}
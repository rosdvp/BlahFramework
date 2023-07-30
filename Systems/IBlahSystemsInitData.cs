namespace Blah.Systems
{
/// <summary>
/// Implement this interface on a class you want to pass to <see cref="IBlahInitSystem.Init"/>.<br/>
/// Later, inside Init method, cast back the interface to a specific class you need.
/// </summary>
public interface IBlahSystemsInitData { }
}
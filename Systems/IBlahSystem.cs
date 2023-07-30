namespace Blah.Systems
{
public interface IBlahSystem
{
}

public interface IBlahInitSystem : IBlahSystem
{
	/// <summary>
	/// Called only once when the system's group becomes active.<br/>
	/// Called before <see cref="IBlahResumePauseSystem.Resume"/>.<br/>
	/// Called before <see cref="IBlahRunSystem.Run"/>.
	/// </summary>
	/// <param name="initData">Cast this interface to desire type.</param>
	public void Init(IBlahSystemsInitData initData);
}

public interface IBlahRunSystem : IBlahSystem
{
	/// <summary>
	/// Called once per frame. <see cref="BlahSystemsContext{TGroupId}.Run"/>.<br/>
	/// Called after <see cref="IBlahInitSystem.Init"/>.<br/>
	/// Called after <see cref="IBlahResumePauseSystem.Pause"/>.<br/>
	/// </summary>
	public void Run();
}

public interface IBlahResumePauseSystem : IBlahSystem
{
	/// <summary>
	/// Called each time the system's group become active.<br/>
	/// Called after <see cref="IBlahInitSystem.Init"/>.<br/>
	/// Called before <see cref="IBlahRunSystem.Run"/>.<br/>
	/// </summary>
	public void Resume();
	/// <summary>
	/// Called each time the system's group become inactive.<br/>
	/// After call, <see cref="IBlahRunSystem.Run"/> will not be called until <see cref="Resume"/>.
	/// </summary>
	public void Pause();
}
}
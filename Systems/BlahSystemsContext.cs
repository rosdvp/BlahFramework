using System;
using System.Collections.Generic;

namespace Blah.Systems
{
public class BlahSystemsContext
{
	private readonly Dictionary<int, BlahSystemsGroup> _groupsMap = new();
	private readonly IBlahSystemsInitData              _systemsInitData;

	/// <param name="systemsInitData">This data will be passed to <see cref="IBlahInitSystem.Init"/></param>
	public BlahSystemsContext(IBlahSystemsInitData systemsInitData)
	{
		_systemsInitData = systemsInitData;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private BlahSystemsGroup _activeGroup;

	private int? _requestedSwitchGroupId;

	public int? ActiveGroupId { get; private set; }

	public bool IsSwitchGroupRequested { get; private set; }

	/// <summary>
	/// Creates new group to add systems to.<br/>
	/// </summary>
	/// <remarks>No group is active by default.</remarks>
	public BlahSystemsGroup AddGroup(int groupId)
	{
		if (_groupsMap.TryGetValue(groupId, out _))
			throw new Exception($"Group with id {groupId} already exists.");
		var group = new BlahSystemsGroup();
		_groupsMap[groupId] = group;
		return group;
	}

	/// <summary>
	/// Sets the <paramref name="groupId"/> active, and the current one - inactive<br/>
	/// For current group, <see cref="IBlahResumePauseSystem.Pause"/> will be called.<br/>
	/// For new group <see cref="IBlahInitSystem.Init"/> and <see cref="IBlahResumePauseSystem.Resume"/>
	/// will be called.
	/// </summary>
	/// <remarks>
	/// If <see cref="Run"/> is performing, switch will happen at the end of <see cref="Run"/>.<br/>
	/// Else, switch will happen immediately. 
	/// </remarks>
	public void RequestSwitchGroup(int? groupId)
	{
		if (groupId == ActiveGroupId)
			return;

		if (groupId == null || _groupsMap.TryGetValue(groupId.Value, out _))
		{
			_requestedSwitchGroupId   = groupId;
			IsSwitchGroupRequested = true;
		}
		else
			throw new Exception($"Group with id {groupId} does not exists.");
	}


	private void PerformSwitch()
	{
		_activeGroup?.PauseSystems();
		if (_requestedSwitchGroupId == null)
		{
			_activeGroup = null;
		}
		else
		{
			_activeGroup = _groupsMap[_requestedSwitchGroupId.Value];
			_activeGroup.TryInitSystems(_systemsInitData);
			_activeGroup.ResumeSystems();
		}
		ActiveGroupId = _requestedSwitchGroupId;
	}

	/// <summary>
	/// Calls <see cref="IBlahRunSystem.Run"/> for the current group systems.<br/>
	/// </summary>
	public void Run()
	{
		if (IsSwitchGroupRequested)
		{
			PerformSwitch();
			IsSwitchGroupRequested = false;
		}
		
		_activeGroup?.RunSystems();
	}
}
}
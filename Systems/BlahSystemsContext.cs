using System;
using System.Collections.Generic;
using System.Text;

namespace Blah.Systems
{
public class BlahSystemsContext
{
	private readonly Dictionary<int, BlahSystemsGroup> _groupsMap = new();

	private readonly IBlahSystemsInitData _systemsInitData;

	private readonly Action _cbBetweenPauseAndResume;

	public BlahSystemsContext(IBlahSystemsInitData systemsInitData, Action cbBetweenPauseAndResume)
	{
		_systemsInitData = systemsInitData;

		_cbBetweenPauseAndResume = cbBetweenPauseAndResume;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private BlahSystemsGroup _activeGroup;

	private bool _isSwitchRequested;
	private int? _requestedSwitchGroupId;
	public  int? ActiveGroupId { get; private set; }


	public BlahSystemsGroup AddGroup(int groupId)
	{
		if (_groupsMap.TryGetValue(groupId, out _))
			throw new Exception($"Group with id {groupId} already exists.");
		var group = new BlahSystemsGroup();
		_groupsMap[groupId] = group;
		return group;
	}

	public void RequestSwitchGroup(int? groupId)
	{
		if (groupId == ActiveGroupId)
			return;

		if (groupId == null || _groupsMap.TryGetValue(groupId.Value, out _))
		{
			_requestedSwitchGroupId = groupId;
			_isSwitchRequested      = true;
		}
		else
			throw new Exception($"Group with id {groupId} does not exists.");
	}


	private void PerformSwitch()
	{
		_activeGroup?.PauseSystems();

		_cbBetweenPauseAndResume?.Invoke();

		if (_requestedSwitchGroupId == null)
		{
			_activeGroup = null;
		}
		else
		{
			_activeGroup = _groupsMap[_requestedSwitchGroupId.Value];
			_activeGroup.TryInitSystems(_systemsInitData);
			_activeGroup.ResumeSystems(_systemsInitData);
		}
		ActiveGroupId = _requestedSwitchGroupId;
	}


	public void Run()
	{
		if (_isSwitchRequested)
		{
			PerformSwitch();
			_isSwitchRequested = false;
		}
		_activeGroup?.RunSystems();
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IReadOnlyList<IBlahSystem> GetAllSystems(int groupId)
	{
		return _groupsMap[groupId].AllSystem;
	}
}
}
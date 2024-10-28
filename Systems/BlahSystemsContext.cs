using System;
using System.Collections.Generic;
using System.Text;

namespace Blah.Systems
{
public class BlahSystemsContext
{
	private readonly Dictionary<int, BlahSystemsGroup> _groupsMap = new();

	private readonly IBlahSystemsInitData _systemsInitData;

	private readonly Action _cbOnSwitch;
	
	public BlahSystemsContext(IBlahSystemsInitData systemsInitData, Action cbOnSwitch)
	{
		_systemsInitData = systemsInitData;
		_cbOnSwitch      = cbOnSwitch;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private BlahSystemsGroup _activeGroup;

	private bool _isSwitchRequested;
	private int? _requestedSwitchGroupId;

	public int? ActiveGroupId { get; private set; }

	public BlahSystemsGroup AddGroup(int groupId)
	{
		if (_groupsMap.TryGetValue(groupId, out _))
			throw new Exception($"Group with id {groupId} already exists.");
		var group = new BlahSystemsGroup();
		_groupsMap[groupId] = group;
		return group;
	}

	public void RequestSwitchOnNextRun(int? groupId)
	{
		_isSwitchRequested      = true;
		_requestedSwitchGroupId = groupId;
	}
	
	public void Run()
	{
		if (_isSwitchRequested)
		{
			PerformSwitch(_requestedSwitchGroupId);
			_isSwitchRequested = false;
		}
		
		_activeGroup?.RunSystems();
	}

	private void PerformSwitch(int? groupId)
	{
		if (groupId == ActiveGroupId)
			return;
		
		_activeGroup?.PauseSystems();
		_cbOnSwitch?.Invoke();
		
		ActiveGroupId = groupId;
		if (groupId == null)
		{
			_activeGroup = null;
			return;
		}
		
		if (!_groupsMap.TryGetValue(groupId.Value, out _activeGroup))
			throw new Exception($"group {groupId.Value} does not exist");
		_activeGroup.TryInitSystems(_systemsInitData);
		_activeGroup.ResumeSystems(_systemsInitData);
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public IReadOnlyList<IBlahSystem> GetAllSystems(int groupId)
	{
		return _groupsMap[groupId].AllSystem;
	}

#if UNITY_EDITOR
	public string DebugGetSystemsOrderMsg()
	{
		var sb = new StringBuilder();
		foreach ((int groupId, var group) in _groupsMap)
		{
			sb.AppendLine($"--- group {groupId} ---");
			foreach (var system in group.AllSystem)
				sb.AppendLine(system.GetType().Name);
			sb.AppendLine("---------------------");
		}
		return sb.ToString();
	}
#endif
}
}
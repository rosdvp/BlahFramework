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

	/// <param name="systemsInitData">
	/// Passed to systems' Init.
	/// </param>
	/// <param name="cbBetweenPauseAndResume">
	/// Invoked after old group pause and before new group resume.
	/// </param>
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

	/// <summary>
	/// Calls <see cref="IBlahRunSystem.Run"/> for the current group systems.<br/>
	/// </summary>
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
#if UNITY_EDITOR
	public string DebugGetSystemsOrderMsg()
	{
		var sb = new StringBuilder();
		foreach ((int groupId, var group) in _groupsMap)
		{
			sb.AppendLine($"--- group {groupId} ---");
			foreach (var system in group.GetAllSystems())
				sb.AppendLine(system.GetType().Name);
			sb.AppendLine("---------------------");
		}
		return sb.ToString();
	}
#endif
}
}
﻿using System;
using System.Collections.Generic;
using Blah.Profiling;
using UnityEngine.Profiling;

namespace Blah.Systems
{
public class BlahSystemsGroup
{
	private readonly List<IBlahSystem>       _allSystems    = new();
	private readonly List<IBlahInitSystem>   _initSystems   = new();
	private readonly List<IBlahRunSystem>    _runSystems    = new();
	private readonly List<IBlahPauseSystem>  _pauseSystems  = new();
	private readonly List<IBlahResumeSystem> _resumeSystems = new();
	
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private bool _isInited;
	private bool _isActive;

	/// <summary>
	/// Add system to the group.
	/// </summary>
	public BlahSystemsGroup AddSystem(IBlahSystem system)
	{
		if (_isInited)
			throw new Exception("Impossible to add system if systems of the group is already inited.");
		
		_allSystems.Add(system);
		if (system is IBlahInitSystem initSystem)
			_initSystems.Add(initSystem);
		if (system is IBlahRunSystem runSystem)
			_runSystems.Add(runSystem);
		if (system is IBlahPauseSystem pauseSystem)
			_pauseSystems.Add(pauseSystem);
		if (system is IBlahResumeSystem resumeSystem)
			_resumeSystems.Add(resumeSystem);
		return this;
	}

	internal void TryInitSystems(IBlahSystemsInitData initData)
	{
		if (_isInited)
			return;
		_isInited = true;
		for (var i = 0; i < _initSystems.Count; i++)
		{
			BlahProfilerMarkers.SystemInit.Begin();
			_initSystems[i].Init(initData);
			BlahProfilerMarkers.SystemInit.End();
		}
	}

	internal void ResumeSystems(IBlahSystemsInitData initData)
	{
		if (!_isInited)
			throw new Exception("Group is not inited!");
		if (_isActive)
			throw new Exception("Group is already active!");
		_isActive = true;
		for (var i = 0; i < _resumeSystems.Count; i++)
		{
			BlahProfilerMarkers.SystemResume.Begin();
			_resumeSystems[i].Resume(initData);
			BlahProfilerMarkers.SystemResume.End();
		}
	}

	internal void PauseSystems()
	{
		if (!_isInited)
			throw new Exception("Group is not inited!");
		if (!_isActive)
			throw new Exception("Group is already inactive!");
		_isActive = false;
		for (var i = 0; i < _pauseSystems.Count; i++)
		{
			BlahProfilerMarkers.SystemPause.Begin();
			_pauseSystems[i].Pause();
			BlahProfilerMarkers.SystemPause.End();
		}
	}
	
	internal void RunSystems()
	{
		if (!_isInited)
			throw new Exception("Group is not inited!");
		if (!_isActive)
			throw new Exception("Group is inactive!");

		for (var i = 0; i < _runSystems.Count; i++)
		{
			BlahProfilerMarkers.SystemRun.Begin();
			_runSystems[i].Run();
			BlahProfilerMarkers.SystemRun.End();
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	internal IReadOnlyList<IBlahSystem> AllSystem => _allSystems;
}
}
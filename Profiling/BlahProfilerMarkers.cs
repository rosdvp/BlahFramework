using System.Collections.Generic;
using System.Diagnostics;
using Blah.Systems;
using Unity.Profiling;

namespace Blah.Profiling
{
public static class BlahProfilerMarkers
{
	private static Dictionary<IBlahSystem, ProfilerMarker> _systemsInits = new();
	private static Dictionary<IBlahSystem, ProfilerMarker> _systemsResumes = new();
	private static Dictionary<IBlahSystem, ProfilerMarker> _systemsPauses = new();
	private static Dictionary<IBlahSystem, ProfilerMarker> _systemsRuns = new();

	public static ProfilerMarker BeginSystemInit(IBlahSystem system)
	{
		if (!_systemsInits.TryGetValue(system, out var marker))
		{
			marker                = new ProfilerMarker($"{system.GetType().Name}.Init");
			_systemsInits[system] = marker;
		}
		marker.Begin();
		return marker;
	}
	
	public static ProfilerMarker BeginSystemResume(IBlahSystem system)
	{
		if (!_systemsResumes.TryGetValue(system, out var marker))
		{
			marker                  = new ProfilerMarker($"{system.GetType().Name}.Resume");
			_systemsResumes[system] = marker;
		}
		marker.Begin();
		return marker;
	}
	
	public static ProfilerMarker BeginSystemPause(IBlahSystem system)
	{
		if (!_systemsPauses.TryGetValue(system, out var marker))
		{
			marker                 = new ProfilerMarker($"{system.GetType().Name}.Pause");
			_systemsPauses[system] = marker;
		}
		marker.Begin();
		return marker;
	}
	
	public static ProfilerMarker BeginSystemRun(IBlahSystem system)
	{
		if (!_systemsRuns.TryGetValue(system, out var marker))
		{
			marker               = new ProfilerMarker($"{system.GetType().Name}.Run");
			_systemsRuns[system] = marker;
		}
		marker.Begin();
		return marker;
	}
}
}
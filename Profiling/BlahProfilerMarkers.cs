using Unity.Profiling;

namespace Blah.Profiling
{
public static class BlahProfilerMarkers
{
	public static readonly ProfilerMarker SystemInit   = new("System.Init");
	public static readonly ProfilerMarker SystemResume = new("System.Resume");
	public static readonly ProfilerMarker SystemPause  = new("System.Pause");
	public static readonly ProfilerMarker SystemRun    = new("System.Run");
}
}
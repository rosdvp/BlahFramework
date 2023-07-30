using NUnit.Framework;

namespace Blah.Systems.Tests
{
internal static class AssertHelper
{
	public static void AssertSystems(MockBaseSystem[] systems,
	                                 int[]             expectedInitOrders,
	                                 int[]             expectedInitCounts,
	                                 int[]             expectedRunOrders,
	                                 int[]             expectedRunCounts,
	                                 int[]             expectedResumeOrders,
	                                 int[]             expectedResumeCounts,
	                                 int[]             expectedPauseOrders,
	                                 int[]             expectedPauseCounts)
	{
		for (var i = 0; i < systems.Length; i++)
		{
			if (expectedInitOrders != null)
				Assert.AreEqual(expectedInitOrders[i], systems[i].InitOrder, $"InitOrder, system[{i}]");
			if (expectedInitCounts != null)
				Assert.AreEqual(expectedInitCounts[i], systems[i].InitCount, $"InitCount, system[{i}]");

			if (expectedRunOrders != null)
				Assert.AreEqual(expectedRunOrders[i], systems[i].RunOrder, $"RunOrder, system[{i}]");
			if (expectedRunCounts != null)
				Assert.AreEqual(expectedRunCounts[i], systems[i].RunCount, $"RunCount, system[{i}]");

			if (expectedResumeOrders != null)
				Assert.AreEqual(expectedResumeOrders[i], systems[i].ResumeOrder, $"ResumeOrder, system[{i}]");
			if (expectedResumeCounts != null)
				Assert.AreEqual(expectedResumeCounts[i], systems[i].ResumeCount, $"ResumeCount, system[{i}]");

			if (expectedPauseOrders != null)
				Assert.AreEqual(expectedPauseOrders[i], systems[i].PauseOrder, $"PauseOrder, system[{i}]");
			if (expectedPauseCounts != null)
				Assert.AreEqual(expectedPauseCounts[i], systems[i].PauseCount, $"PauseCount, system[{i}]");
			
			systems[i].ResetOrders();
		}
	}
}
}
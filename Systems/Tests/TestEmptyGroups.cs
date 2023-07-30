using NUnit.Framework;

namespace Blah.Systems.Tests
{
internal class TestEmptyGroups
{
	[Test]
	public void Test_SwitchToA_ABecomesActive()
	{
		var context = new BlahSystemsContext(null);
		context.AddGroup((int)EMockGroupId.GroupA);
		context.AddGroup((int)EMockGroupId.GroupB);
		
		context.RequestSwitchGroup((int)EMockGroupId.GroupA);
		
		Assert.AreEqual(null, context.ActiveGroupId);
		
		context.Run();
		
		Assert.AreEqual((int)EMockGroupId.GroupA, context.ActiveGroupId);
	}

	[Test]
	public void Test_SwitchToB_BBecomesActive()
	{
		var context = new BlahSystemsContext(null);
		context.AddGroup((int)EMockGroupId.GroupA);
		context.AddGroup((int)EMockGroupId.GroupB);
		
		context.RequestSwitchGroup((int)EMockGroupId.GroupB);
		
		Assert.AreEqual(null, context.ActiveGroupId);
		
		context.Run();
		
		Assert.AreEqual((int)EMockGroupId.GroupB, context.ActiveGroupId);
	}

	[Test]
	public void Test_SwitchToBAndRun_BIsActive()
	{
		var context = new BlahSystemsContext(null);
		context.AddGroup((int)EMockGroupId.GroupA);
		context.AddGroup((int)EMockGroupId.GroupB);
		
		context.RequestSwitchGroup((int)EMockGroupId.GroupB);
		
		Assert.AreEqual(null, context.ActiveGroupId);
		
		context.Run();
		
		Assert.AreEqual((int)EMockGroupId.GroupB, context.ActiveGroupId);
		
		context.Run();
	}

	[Test]
	public void Test_SwitchToBThenToAThenRun_ABecomesActive()
	{
		var context = new BlahSystemsContext(null);
		context.AddGroup((int)EMockGroupId.GroupA);
		context.AddGroup((int)EMockGroupId.GroupB);
		
		context.RequestSwitchGroup((int)EMockGroupId.GroupB);
		context.Run();
		context.RequestSwitchGroup((int)EMockGroupId.GroupA);
		context.Run();
		
		Assert.AreEqual((int)EMockGroupId.GroupA, context.ActiveGroupId);
	}

	[Test]
	public void Test_SwitchToBThenToAThenToNo_NoIsActive()
	{
		var context = new BlahSystemsContext(null);
		context.AddGroup((int)EMockGroupId.GroupA);
		context.AddGroup((int)EMockGroupId.GroupB);
		
		context.RequestSwitchGroup((int)EMockGroupId.GroupB);
		context.Run();
		context.RequestSwitchGroup((int)EMockGroupId.GroupA);
		context.Run();
		context.RequestSwitchGroup(null);
		context.Run();
		
		Assert.Null(context.ActiveGroupId);
		
		context.Run();
		
		Assert.Null(context.ActiveGroupId);
	}
}
}
using System;
using NUnit.Framework;

namespace Blah.Systems.Tests
{
internal class TestNoGroups
{
	[Test]
	public void Test_Create_NoGroupActive()
	{
		var context = new BlahSystemsContext(null, null);
		Assert.Null(context.ActiveGroupId);
	}
	
	[Test]
	public void Test_SwitchToNo_NoGroupActive()
	{
		var context = new BlahSystemsContext(null, null);
		context.RequestSwitchGroup(null);
		context.Run();
		Assert.Null(context.ActiveGroupId);
	}

	[Test]
	public void Test_Create_Run()
	{
		var context = new BlahSystemsContext(null, null);
		context.Run();
		Assert.Null(context.ActiveGroupId);
	}

	[Test]
	public void Test_SwitchToA_Exception()
	{
		var context = new BlahSystemsContext(null, null);
		try
		{
			context.RequestSwitchGroup((int)EMockGroupId.GroupA);
			Assert.Fail();
		}
		catch (Exception)
		{
			// ignored
		}
	}
}
}
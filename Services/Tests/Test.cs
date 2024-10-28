using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Blah.Services.Tests
{
internal class Test
{
	[Test]
	public void Test_NoServices()
	{
		var services = new BlahServicesContext(null);
		services.FinalizeInit();
	}

	[Test]
	public void Test_Get_Inited()
	{
		var services = new BlahServicesContext(null);

		Assert.IsNull(services.TestsRawGet<ServiceA>());
		Assert.IsNull(services.TestsRawGet<ServiceBDependsOnA>());
		Assert.IsNull(services.TestsRawGet<ServiceCDependsOnB>());
		Assert.IsNull(services.TestsRawGet<ServiceDDependsOnBAndA>());

		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceCDependsOnB>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceDDependsOnBAndA>().InitsCount);
	}

	[Test]
	public void Test_GetAndFinalize_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		
		Assert.IsNull(services.TestsRawGet<ServiceA>());
		
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		
		services.FinalizeInit();
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
	}

	[Test]
	public void Test_BonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		var b = services.Get<ServiceBDependsOnA>();
		Assert.AreEqual(0, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		
		b.InvokeA();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
	}

	[Test]
	public void Test_ConBonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		var c = services.Get<ServiceCDependsOnB>();
		Assert.IsNull(services.TestsRawGet<ServiceA>());
		Assert.AreEqual(0, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceCDependsOnB>().InitsCount);
		
		c.InvokeBWhichInvokesA();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceCDependsOnB>().InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceCDependsOnB>().InitsCount);
	}

	[Test]
	public void Test_All_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		var d = services.Get<ServiceDDependsOnBAndA>();
		Assert.AreEqual(0, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(0, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceDDependsOnBAndA>().InitsCount);
		
		d.InvokeBWithInvokesA_InvokeA();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceDDependsOnBAndA>().InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceDDependsOnBAndA>().InitsCount);
	}

	[Test]
	public void Test_DependencyInInit_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		var e = services.Get<ServiceEDependsOnFInInit>();
		Assert.AreEqual(1, services.TestsRawGet<ServiceEDependsOnFInInit>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceFDependsOnE>().InitsCount);

		services.FinalizeInit();
		
		Assert.AreEqual(1, services.TestsRawGet<ServiceEDependsOnFInInit>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceFDependsOnE>().InitsCount);
	}

	[Test]
	public void Test_LazyAndFinalizeInit_Inited()
	{
		var services = new BlahServicesContext(null);

		var b = services.Get<ServiceBDependsOnA>();
		Assert.AreEqual(0, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);

		services.FinalizeInit();

		Assert.AreEqual(1, services.TestsRawGet<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.TestsRawGet<ServiceBDependsOnA>().InitsCount);
	}
}
}
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
	public void Test_AddAAndFinalize_Inited()
	{	
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		
		Assert.AreEqual(0, AssertHelper.GetService<ServiceA>(services).InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
	}

	[Test]
	public void Test_AddAAndDoAndFinalize_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		
		Assert.AreEqual(0, AssertHelper.GetService<ServiceA>(services).InitsCount);
		
		services.Get<ServiceA>().Do();
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
	}

	[Test]
	public void Test_AllFinalizeWithoutDo_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		services.Add<ServiceBDependsOnA>();
		services.Add<ServiceCDependsOnB>();
		services.Add<ServiceDDependsOnBAndA>();
		
		Assert.AreEqual(0, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(0, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
		Assert.AreEqual(0, AssertHelper.GetService<ServiceCDependsOnB>(services).InitsCount);
		Assert.AreEqual(0, AssertHelper.GetService<ServiceDDependsOnBAndA>(services).InitsCount);
		
		services.FinalizeInit();

		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceCDependsOnB>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceDDependsOnBAndA>(services).InitsCount);
	}

	[Test]
	public void Test_BonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		services.Add<ServiceBDependsOnA>();
		services.Get<ServiceBDependsOnA>().DoA();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
	}

	[Test]
	public void Test_ConBonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		services.Add<ServiceBDependsOnA>();
		services.Add<ServiceCDependsOnB>();
		
		services.Get<ServiceCDependsOnB>().DoB();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceCDependsOnB>(services).InitsCount);
	}

	[Test]
	public void Test_All_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceA>();
		services.Add<ServiceBDependsOnA>();
		services.Add<ServiceCDependsOnB>();
		services.Add<ServiceDDependsOnBAndA>();
		
		services.Get<ServiceDDependsOnBAndA>().DoBAndA();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
		Assert.AreEqual(0, AssertHelper.GetService<ServiceCDependsOnB>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceDDependsOnBAndA>(services).InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceBDependsOnA>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceCDependsOnB>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceDDependsOnBAndA>(services).InitsCount);	
	}

	[Test]
	public void Test_CyclicEAndF_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		services.Add<ServiceEDependsOnFInInit>();
		services.Add<ServiceFDependsOnE>();
		
		Assert.AreEqual(0, AssertHelper.GetService<ServiceEDependsOnFInInit>(services).InitsCount);
		Assert.AreEqual(0, AssertHelper.GetService<ServiceFDependsOnE>(services).InitsCount);
		
		services.Get<ServiceEDependsOnFInInit>().DoEmpty();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceEDependsOnFInInit>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceFDependsOnE>(services).InitsCount);
		
		services.Get<ServiceFDependsOnE>().DoE();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceEDependsOnFInInit>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceFDependsOnE>(services).InitsCount);
		
		services.FinalizeInit();
		
		Assert.AreEqual(1, AssertHelper.GetService<ServiceEDependsOnFInInit>(services).InitsCount);
		Assert.AreEqual(1, AssertHelper.GetService<ServiceFDependsOnE>(services).InitsCount);
	}
}
}
using System;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Blah.Services.Tests
{
internal class Test
{
	[Test]
	public void Test_GetA_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		Assert.IsNull(services.TestsGetWithoutInit<ServiceA>());
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
	}

	[Test]
	public void Test_GetMany_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		
		Assert.IsNull(services.TestsGetWithoutInit<ServiceA>());
		Assert.IsNull(services.TestsGetWithoutInit<ServiceBDependsOnA>());
		Assert.IsNull(services.TestsGetWithoutInit<ServiceCDependsOnB>());
		Assert.IsNull(services.TestsGetWithoutInit<ServiceDDependsOnBAndA>());
		
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceCDependsOnB>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceCDependsOnB>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceDDependsOnBAndA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceDDependsOnBAndA>().InitsCount);
	}

	[Test]
	public void Test_BonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);
		
		Assert.AreEqual(1, services.Get<ServiceBDependsOnA>().InitsCount);
		Assert.IsNull(services.TestsGetWithoutInit<ServiceA>());
		
		services.Get<ServiceBDependsOnA>().DoA();
		
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceA>().InitsCount);
		Assert.AreEqual(1, services.Get<ServiceA>().InitsCount);
	}

	[Test]
	public void Test_ConBonA_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		services.Get<ServiceCDependsOnB>().DoB();
		
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceA>().InitsCount);
	}

	[Test]
	public void Test_All_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		services.Get<ServiceDDependsOnBAndA>().DoBAndA();
		
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceBDependsOnA>().InitsCount);
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceA>().InitsCount);
		Assert.IsNull(services.TestsGetWithoutInit<ServiceCDependsOnB>());
	}

	[Test]
	public void Test_CyclicEAndF_InitedOnce()
	{
		var services = new BlahServicesContext(null);

		services.Get<ServiceEDependsOnFInInit>().DoEmpty();
		
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceFDependsOnE>().InitsCount);

		services.Get<ServiceFDependsOnE>().DoE();

		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceEDependsOnFInInit>().InitsCount);
		Assert.AreEqual(1, services.TestsGetWithoutInit<ServiceFDependsOnE>().InitsCount);
	}


	private class ServiceA : BlahServiceBase
	{
		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;
		}

		public void Do()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");
		}
	}

	private class ServiceBDependsOnA : BlahServiceBase
	{
		private BlahServiceLazy<ServiceA> _serviceA;

		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;

			_serviceA = services.GetLazy<ServiceA>();
		}

		public void DoA()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");

			_serviceA.Get.Do();
		}
	}

	private class ServiceCDependsOnB : BlahServiceBase
	{
		private BlahServiceLazy<ServiceBDependsOnA> _serviceB;

		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;

			_serviceB = services.GetLazy<ServiceBDependsOnA>();
		}

		public void DoB()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");

			_serviceB.Get.DoA();
		}
	}

	private class ServiceDDependsOnBAndA : BlahServiceBase
	{
		private BlahServiceLazy<ServiceBDependsOnA> _serviceB;
		private BlahServiceLazy<ServiceA>           _serviceA;

		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;

			_serviceA = services.GetLazy<ServiceA>();
			_serviceB = services.GetLazy<ServiceBDependsOnA>();
		}

		public void DoBAndA()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");

			_serviceB.Get.DoA();
			_serviceA.Get.Do();
		}
	}

	private class ServiceEDependsOnFInInit : BlahServiceBase
	{
		private BlahServiceLazy<ServiceFDependsOnE> _serviceF;

		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;

			_serviceF = services.GetLazy<ServiceFDependsOnE>();
			_serviceF.Get.DoEmpty();
		}

		public void DoEmpty()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");
		}
	}

	private class ServiceFDependsOnE : BlahServiceBase
	{
		private BlahServiceLazy<ServiceEDependsOnFInInit> _serviceE;

		public int InitsCount { get; private set; }

		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services)
		{
			InitsCount += 1;

			_serviceE = services.GetLazy<ServiceEDependsOnFInInit>();
		}

		public void DoEmpty()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");
		}

		public void DoE()
		{
			if (InitsCount == 0)
				throw new Exception("not inited");
			_serviceE.Get.DoEmpty();
		}
	}
}
}
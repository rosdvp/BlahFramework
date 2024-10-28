using System;
using System.Reflection;

namespace Blah.Services.Tests
{
internal class ServiceA : BlahServiceBase
{
	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;
	}

	public void Do()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
	}
}

internal class ServiceBDependsOnA : BlahServiceBase
{
	private BlahServiceLazy<ServiceA> _serviceA;

	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;

		_serviceA = services.GetLazy<ServiceA>();
	}

	public void InvokeA()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
		
		_serviceA.Get.Do();
	}
}

internal class ServiceCDependsOnB : BlahServiceBase
{
	private BlahServiceLazy<ServiceBDependsOnA> _serviceB;

	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;

		_serviceB = services.GetLazy<ServiceBDependsOnA>();
	}

	public void InvokeBWhichInvokesA()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
		
		_serviceB.Get.InvokeA();
	}
}

internal class ServiceDDependsOnBAndA : BlahServiceBase
{
	private BlahServiceLazy<ServiceBDependsOnA> _serviceB;
	private BlahServiceLazy<ServiceA>           _serviceA;

	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;

		_serviceA = services.GetLazy<ServiceA>();
		_serviceB = services.GetLazy<ServiceBDependsOnA>();
	}

	public void InvokeBWithInvokesA_InvokeA()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
		
		_serviceB.Get.InvokeA();
		_serviceA.Get.Do();
	}
}

internal class ServiceEDependsOnFInInit : BlahServiceBase
{
	private BlahServiceLazy<ServiceFDependsOnE> _serviceF;

	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;

		_serviceF = services.GetLazy<ServiceFDependsOnE>();
		_serviceF.Get.InvokeEmpty();
	}

	public void InvokeEmpty()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
	}
}

internal class ServiceFDependsOnE : BlahServiceBase
{
	private BlahServiceLazy<ServiceEDependsOnFInInit> _serviceE;

	public int InitsCount { get; private set; }

	protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContext services)
	{
		InitsCount += 1;

		_serviceE = services.GetLazy<ServiceEDependsOnFInInit>();
	}

	public void InvokeEmpty()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
	}

	public void InvokeE()
	{
		if (InitsCount == 0)
			throw new Exception("not inited");
		_serviceE.Get.InvokeEmpty();
	}
}
}
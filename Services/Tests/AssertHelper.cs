using System;
using System.Reflection;

namespace Blah.Services.Tests
{
internal static class AssertHelper
{
	public static T GetService<T>(BlahServicesContext context)
	{
		var type       = typeof(BlahServicesContext);
		var baseMethod = type.GetMethod("TestGet", BindingFlags.Instance | BindingFlags.NonPublic);
		var method     = baseMethod.MakeGenericMethod(typeof(T));
		object result = method.Invoke(context, null);
		return (T)result;
	}
}

internal class ServiceA : BlahServiceBase
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

internal class ServiceBDependsOnA : BlahServiceBase
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

internal class ServiceCDependsOnB : BlahServiceBase
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

internal class ServiceDDependsOnBAndA : BlahServiceBase
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

internal class ServiceEDependsOnFInInit : BlahServiceBase
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

internal class ServiceFDependsOnE : BlahServiceBase
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
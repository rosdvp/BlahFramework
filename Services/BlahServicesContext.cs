using System;
using System.Collections.Generic;

namespace Blah.Services
{
public class BlahServicesContext : IBlahServicesContainerLazy
{
	private readonly IBlahServicesInitData _initData;

	private readonly Dictionary<Type, BlahServiceBase> _typeToService = new();

	public BlahServicesContext(IBlahServicesInitData initData)
	{
		_initData = initData;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public T Get<T>() where T : BlahServiceBase, new()
	{
		if (_typeToService.TryGetValue(typeof(T), out var service))
			return (T)service;
		service = new T();
		service.TryInit(_initData, this);
		_typeToService[typeof(T)] = service;
		return (T)service;
	}

	BlahServiceLazy<T> IBlahServicesContainerLazy.GetLazy<T>()
	{
		return new BlahServiceLazy<T>(this);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public T TestsGetWithoutInit<T>() where T : BlahServiceBase 
		=> _typeToService.TryGetValue(typeof(T), out var service) ? (T)service : null;
#endif
}
}
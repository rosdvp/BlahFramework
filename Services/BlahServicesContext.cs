using System;
using System.Collections.Generic;

namespace Blah.Services
{
public class BlahServicesContext : IBlahServicesContext
{
	private readonly IBlahServicesInitData _initData;

	private readonly Dictionary<Type, BlahServiceBase> _typeToService = new();

	public BlahServicesContext(IBlahServicesInitData initData)
	{
		_initData = initData;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public void FinalizeInit()
	{
		foreach (var (_, service) in _typeToService)
			if (service.State != BlahServiceBase.EState.Inited)
				service.Init(_initData, this);
	}

	public T Get<T>() where T : BlahServiceBase, new()
	{
		if (!_typeToService.TryGetValue(typeof(T), out var service))
		{
			service                   = new T();
			_typeToService[typeof(T)] = service;
		}
		if (service.State != BlahServiceBase.EState.Inited)
			service.Init(_initData, this);
		return (T)service;
	}

	public BlahServiceLazy<T> GetLazy<T>() where T : BlahServiceBase, new()
	{
		if (!_typeToService.ContainsKey(typeof(T)))
			_typeToService.Add(typeof(T), new T());
		return new BlahServiceLazy<T>(this);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
#if BLAH_TESTS
	public T TestsRawGet<T>() where T : BlahServiceBase
		=> _typeToService.TryGetValue(typeof(T), out var service) ? (T)service : null;
#endif
}

public interface IBlahServicesContext
{
	public BlahServiceLazy<T> GetLazy<T>() where T : BlahServiceBase, new();
}
}
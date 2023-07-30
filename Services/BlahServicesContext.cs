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
	public void Add<T>() where T: BlahServiceBase, new()
	{
		if (!_typeToService.TryAdd(typeof(T), new T()))
			throw new Exception($"service {typeof(T)} is already added");
	}

	public void TryAdd(Type type, BlahServiceBase service) 
		=> _typeToService.TryAdd(type, service);

	public T Get<T>() where T : BlahServiceBase
	{
		if (_typeToService.TryGetValue(typeof(T), out var service))
		{
			if (service.State != BlahServiceBase.EState.Inited)
				service.Init(_initData, this);
			return (T)service;
		}
		throw new Exception($"service {typeof(T)} is not added");
	}

	BlahServiceLazy<T> IBlahServicesContainerLazy.GetLazy<T>()
	{
		return new BlahServiceLazy<T>(this);
	}

	public void FinalizeInit()
	{
		foreach (var pair in _typeToService)
			if (pair.Value.State != BlahServiceBase.EState.Inited)
				pair.Value.Init(_initData, this);
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private T TestGet<T>() where T: BlahServiceBase => (T)_typeToService[typeof(T)];
}
}
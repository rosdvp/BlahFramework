using System;

namespace Blah.Services
{
public abstract class BlahServiceBase
{
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private EState _state;

	internal void TryInit(IBlahServicesInitData initData, IBlahServicesContainerLazy container)
	{
		if (_state == EState.Inited)
			return;
		
		if (_state == EState.Initing)
			throw new Exception($"{GetType().Name} has cycling dependencies in init");

		_state = EState.Initing;
		InitImpl(initData, container);
		_state = EState.Inited;
	}

	protected abstract void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services);
	

	private enum EState
	{
		None,
		Initing,
		Inited
	}
}
}
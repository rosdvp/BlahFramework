using System;

namespace Blah.Services
{
public abstract class BlahServiceBase
{
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	internal EState State { get; private set; }

	internal void Init(IBlahServicesInitData initData, IBlahServicesContainerLazy container)
	{
		if (State == EState.Initing)
			throw new Exception($"services {GetType()} is already initing, " +
			                    $"perhaps, there is some cycling dependencies in init"
			);

		if (State == EState.Inited)
			throw new Exception($"service {GetType()} is already inited");

		State = EState.Initing;
		InitImpl(initData, container);
		State = EState.Inited;
	}

	protected abstract void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services);
	

	internal enum EState
	{
		None,
		Initing,
		Inited
	}
}
}
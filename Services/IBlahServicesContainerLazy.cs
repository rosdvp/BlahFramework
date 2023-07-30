namespace Blah.Services
{
public interface IBlahServicesContainerLazy
{
	public BlahServiceLazy<T> GetLazy<T>() where T : BlahServiceBase;
}
}
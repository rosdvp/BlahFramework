namespace Blah.Services
{
public class BlahServiceLazy<T> where T: BlahServiceBase
{
	private readonly BlahServicesContext _context;

	internal BlahServiceLazy(BlahServicesContext context)
	{
		_context = context;
	}
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private T _service;

	public T Get => _service ??= _context.Get<T>();
}
}
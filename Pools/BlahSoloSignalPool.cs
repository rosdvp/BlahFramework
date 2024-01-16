using System;

namespace Blah.Pools
{
public interface IBlahEntrySoloSignal { }

public interface IBlahSoloSignalProducer<T> where T: IBlahEntrySoloSignal
{
	public ref T Add();
}

public interface IBlahSoloSignalConsumer<T> where T: IBlahEntrySoloSignal
{
	public bool IsExists { get; }
	public bool IsEmpty  { get; }

	public ref T Get();
	public void Remove();
}

public class BlahSoloSignalPool<T> : IBlahPoolInternal,
	IBlahSoloSignalProducer<T>,
	IBlahSoloSignalConsumer<T> where T: IBlahEntrySoloSignal
{
	private bool _isExists;
	private T    _value;
    
	public ref T Add()
	{
		if (_isExists)
			throw new Exception($"signal already exists: {_value.ToString()}");
		_isExists = true;
		_value    = default;
		return ref _value;
	}

	public bool IsExists => _isExists;
	public bool IsEmpty  => !_isExists;
	
	public ref T Get()
	{
		if (!_isExists)
			throw new Exception("no signal exists");
		return ref _value;
	}

	public void Remove()
	{
		_isExists = false;
	}
	
	
	public void OnNextFrame()
	{
		_isExists = false;
	}

	public void Clear()
	{
		_isExists = false;
	}
}
}
using System;
using System.Collections.Generic;

namespace Blah.Injection.Tests
{
internal class MockInhItemBase { }
internal class MockInhItem1 : MockInhItemBase { }
internal class MockInhItem2 : MockInhItemBase { }

internal class MockGenItemA<T> { }
internal class MockGenArgA1 { }
internal class MockGenArgA2 { }

internal class MockGenItemB<T> { }
internal class MockGenArgB1 { }
internal class MockGenArgB2 { }

internal class MockSource
{
	private Dictionary<Type, object> _typeToObj = new();


	public T GetInhItem<T>() where T : MockInhItemBase, new()
	{
		if (!_typeToObj.TryGetValue(typeof(T), out object obj))
		{
			obj                   = new T();
			_typeToObj[typeof(T)] = obj;
		}
		return (T)obj;
	}
	
	public MockGenItemA<T> GetGenItemA<T>()
	{
		var type = typeof(MockGenItemA<T>);
		if (!_typeToObj.TryGetValue(type, out object obj))
		{
			obj                   = new MockGenItemA<T>();
			_typeToObj[type] = obj;
		}
		return (MockGenItemA<T>)obj;
	}
	
	public MockGenItemB<T> GetGenItemB<T>()
	{
		var type = typeof(MockGenItemB<T>);
		if (!_typeToObj.TryGetValue(type, out object obj))
		{
			obj              = new MockGenItemB<T>();
			_typeToObj[type] = obj;
		}
		return (MockGenItemB<T>)obj;
	}
}
}
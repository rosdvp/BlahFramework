using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blah.Injection
{
public class BlahInjector
{
	private readonly Dictionary<Type, Source> _injectableFieldTypeToSource = new();
	private readonly Dictionary<Type, object> _fieldTypeToObject           = new();

	private readonly object[] _tempParams1 = new object[1];

	public void AddSource(
		object  obj,
		Type    fieldBaseType,
		string  methodBaseName,
		EMethodType methodType)
	{
		_injectableFieldTypeToSource.TryAdd(
			fieldBaseType,
			new Source
			{
				Obj            = obj,
				MethodBaseName = methodBaseName,
				MethodType = methodType
			}
		);
	}

	public void InjectInto(object target)
	{
		var targetType = target.GetType();

		while (targetType?.Namespace?.StartsWith("System") == false)
		{
			var fields = targetType.GetFields(
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			foreach (var field in fields)
				TryInjectIntoField(target, field);

			targetType = targetType.BaseType;
		}
	}

	private void TryInjectIntoField(object target, FieldInfo field)
	{
		var fieldType = field.FieldType;
		if (_fieldTypeToObject.TryGetValue(fieldType, out object obj))
		{
			field.SetValue(target, obj);	
			return;
		}

		var source = TryFindSource(fieldType);
		if (source == null)
			return;

		obj = RetrieveFromSource(source, field);
		field.SetValue(target, obj);
		
		_fieldTypeToObject[fieldType] = obj;
	}

	private Source TryFindSource(Type fieldType)
	{
		Source source = null;
		
		if (_injectableFieldTypeToSource.TryGetValue(fieldType, out source))
			return source;

		if (fieldType.IsGenericType &&
		    _injectableFieldTypeToSource.TryGetValue(fieldType.GetGenericTypeDefinition(), out source))
			return source;

		var baseType = fieldType.BaseType;
		while (baseType != null)
		{
			if (_injectableFieldTypeToSource.TryGetValue(baseType, out source))
				return source;
			baseType = baseType.BaseType;
		}
		
		return null;
	}

	private object RetrieveFromSource(Source source, FieldInfo field)
	{
		var method = source.Obj.GetType().GetMethod(source.MethodBaseName);
		if (method == null)
			throw new Exception($"source {source.Obj.GetType().Name} does not have " +
			                    $"base method with name {source.MethodBaseName}");

		if (source.MethodType == EMethodType.GenericAcceptFieldType)
		{
			method = method.MakeGenericMethod(field.FieldType);
		}
		else if (source.MethodType == EMethodType.GenericAcceptGenericArgument)
		{
			method = method.MakeGenericMethod(field.FieldType.GenericTypeArguments[0]);
		}

		if (source.MethodType == EMethodType.SimpleAcceptFieldInfo)
		{
			_tempParams1[0] = field;
			return method.Invoke(source.Obj, _tempParams1);
		}
		
		return method.Invoke(source.Obj, null);
	}

	private class Source
	{
		public object  Obj;
		public string  MethodBaseName;
		public EMethodType MethodType;
	}

	public enum EMethodType
	{
		Simple,
		GenericAcceptFieldType,
		GenericAcceptGenericArgument,
		SimpleAcceptFieldInfo,
	}
}
}
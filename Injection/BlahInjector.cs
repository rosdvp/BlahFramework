using System;
using System.Collections.Generic;
using System.Reflection;

namespace Blah.Injection
{
public class BlahInjector
{
	private readonly HashSet<Type>            _injectableFieldBaseTypes = new();
	private readonly Dictionary<Type, Source> _fieldBaseTypeToSource    = new();
	private readonly Dictionary<Type, object> _fieldTypeToObject        = new();

	public void AddSource(
		object obj, 
		Type fieldBaseType, 
		string methodBaseName)
	{
		_injectableFieldBaseTypes.Add(fieldBaseType);
		_fieldBaseTypeToSource.TryAdd(fieldBaseType,
		                              new Source
		                              {
			                              Obj            = obj,
			                              MethodBaseName = methodBaseName
		                              }
		);
	}

	public void InjectInto(object target)
	{
		var targetType = target.GetType();

		while (targetType?.Namespace?.StartsWith("System.") == false)
		{
			var fields = targetType.GetFields(
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			foreach (var field in fields)
			{
				var fieldType = field.FieldType;
				var fieldBaseType = fieldType.IsGenericType
					? fieldType.GetGenericTypeDefinition()
					: fieldType.BaseType;

				if (!_injectableFieldBaseTypes.Contains(fieldBaseType))
					continue;

				if (!_fieldTypeToObject.TryGetValue(fieldType, out object obj))
				{
					obj = RetrieveFromSource(
						fieldBaseType,
						fieldType.IsGenericType
							? fieldType.GenericTypeArguments[0]
							: fieldType
					);
					_fieldTypeToObject[fieldType] = obj;
				}
				field.SetValue(target, obj);
			}

			targetType = targetType.BaseType;
		}
	}

	private object RetrieveFromSource(Type fieldBaseType, Type methodGenericType)
	{
		if (!_fieldBaseTypeToSource.TryGetValue(fieldBaseType, out var source))
			throw new Exception($"source for field base type {fieldBaseType.Name} is not added");

		var method = source.Obj.GetType().GetMethod(source.MethodBaseName);
		if (method == null)
			throw new Exception($"source {source.Obj.GetType().Name} does not have " +
			                    $"base method with name {source.MethodBaseName}");
		
		method = method.MakeGenericMethod(methodGenericType);

		return method.Invoke(source.Obj, null);
	}

	private class Source
	{
		public object Obj;
		public string MethodBaseName;
	}
}
}
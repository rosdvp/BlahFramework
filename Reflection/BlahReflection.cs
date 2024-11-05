using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Pools;
using Blah.Services;
using UnityEditor;

namespace Blah.Reflection
{
public static class BlahReflection
{
	/// <summary>
	/// Including base system.
	/// </summary>
	public static IEnumerable<(EKind, Type)> EnumerateSystemFields(Type system)
	{
		while (system?.Namespace?.StartsWith("System") == false)
		{
			var fields = system.GetFields(
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			foreach (var field in fields)
			{
				var fieldType = field.FieldType;

				if (typeof(BlahServiceBase).IsAssignableFrom(fieldType))
				{
					yield return (EKind.Service, fieldType);
					continue;
				}

				if (!fieldType.IsGenericType)
					continue;
				var genBaseType = fieldType.GetGenericTypeDefinition();
				var genArgType  = fieldType.GenericTypeArguments[0];

				if (genBaseType == typeof(IBlahSignalRead<>))
					yield return (EKind.SignalRead, genArgType);
				else if (genBaseType == typeof(IBlahSignalWrite<>))
					yield return (EKind.SignalWrite, genArgType);
				else if (genBaseType == typeof(IBlahNfSignalRead<>))
					yield return (EKind.NfSignalRead, genArgType);
				else if (genBaseType == typeof(IBlahNfSignalWrite<>))
					yield return (EKind.NfSignalWrite, genArgType);
				else if (genBaseType == typeof(IBlahDataGet<>))
					yield return (EKind.DataGet, genArgType);
				else if (genBaseType == typeof(IBlahDataFull<>))
					yield return (EKind.DataFull, genArgType);
			}

			system = system.BaseType;
		}
	}
	
	public static IEnumerable<Attribute> EnumerateAttributes(Type type)
	{
		while (type?.Namespace?.StartsWith("System") == false)
		{
			foreach (var attr in type.GetCustomAttributes())
				yield return attr;
			type = type.BaseType;
		}
	}
	
	public static IEnumerable<Type> EnumerateGameTypes()
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			if (!assembly.FullName.StartsWith("Unity") &&
			    !assembly.FullName.StartsWith("System") &&
			    !assembly.FullName.StartsWith("Blah"))
			{
				foreach (var type in assembly.GetTypes())
					yield return type;
			}
		}
	}

	public static T InstantiateGameTypeWithBaseType<T>() where T : class
	{
		var baseType = typeof(T);
		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == baseType)
				return (T)Activator.CreateInstance(type);
		return null;
	}


	public enum EKind
	{
		Service,
		SignalRead,
		SignalWrite,
		NfSignalRead,
		NfSignalWrite,
		DataGet,
		DataFull,
	}
}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Pools;
using Blah.Services;

namespace Blah.Reflection
{
public static class BlahReflection
{
	/// <summary>
	/// Including base system.
	/// </summary>
	public static IEnumerable<(Type signalType, bool isWrite)> EnumerateSystemSignals(Type system)
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
				if (!fieldType.IsGenericType)
					continue;
				var genBaseType = fieldType.GetGenericTypeDefinition();
				if (genBaseType == typeof(IBlahSignalRead<>))
					yield return (fieldType.GenericTypeArguments[0], false);
				else if (genBaseType == typeof(IBlahSignalWrite<>))
					yield return (fieldType.GenericTypeArguments[0], true);
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
}
}
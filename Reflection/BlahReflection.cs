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

				if (fieldType.BaseType == typeof(BlahServiceBase))
				{
					yield return (EKind.Service, fieldType);
					continue;
				}

				if (!fieldType.IsGenericType)
					continue;
				var genBaseType = fieldType.GetGenericTypeDefinition();
				var genArgType  = fieldType.GenericTypeArguments[0];

				if (genBaseType == typeof(IBlahSignalConsumer<>))
					yield return (EKind.SignalConsumer, genArgType);
				else if (genBaseType == typeof(IBlahSignalProducer<>))
					yield return (EKind.SignalProducer, genArgType);
				else if (genBaseType == typeof(IBlahDataConsumer<>))
					yield return (EKind.DataConsumer, genArgType);
				else if (genBaseType == typeof(IBlahDataProducer<>))
					yield return (EKind.DataProducer, genArgType);
			}

			system = system.BaseType;
		}
	}
	
	
	public static IReadOnlyList<Type> GetFeatureSystems(object feature)
	{
		var prop = feature.GetType().GetProperty(
			"Systems",
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic
		);
		return (IReadOnlyList<Type>)prop?.GetValue(feature);
	}


	/// <summary>
	/// Finds actual dependencies, not those which are set in feature file.
	/// </summary>
	public static void FindFeatureDependencies(
		object feature,
		HashSet<Type>   services,
		HashSet<Type>   consumers,
		HashSet<Type>   producers,
		bool            shouldClearHashSets)
	{
		if (shouldClearHashSets)
		{
			services.Clear();
			consumers.Clear();
			producers.Clear();
		}
		foreach (var system in GetFeatureSystems(feature))
			FindSystemDependencies(system, services, consumers, producers, false);
	}

	public static void FindSystemDependencies(
		Type          system,
		HashSet<Type> services,
		HashSet<Type> consumers,
		HashSet<Type> producers,
		bool shouldClearHashSets)
	{
		if (shouldClearHashSets)
		{
			services.Clear();
			consumers.Clear();
			producers.Clear();
		}
		
		foreach (var (kind, type) in EnumerateSystemFields(system))
			switch (kind)
			{
				case EKind.Service:
					services.Add(type);
					break;
				case EKind.SignalConsumer:
				case EKind.DataConsumer:
					consumers.Add(type);
					break;
				case EKind.SignalProducer:
				case EKind.DataProducer:
					producers.Add(type);
					break;
				default:
					throw new ArgumentOutOfRangeException();
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



	public enum EKind
	{
		Service,
		SignalConsumer,
		SignalProducer,
		DataConsumer,
		DataProducer
	}
}
}
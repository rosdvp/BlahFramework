using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Pools;
using Blah.Services;
using UnityEditor;

namespace Blah.Features
{
public class BlahFeatureValidatorException : Exception
{
	public readonly BlahFeatureBase Feature;
	public readonly Type            SystemType;
	public readonly Type            InvalidType;

	public BlahFeatureValidatorException(BlahFeatureBase feature, Type systemType, Type invalidType)
		: base($"in feature {feature.GetType().Name}, " +
		       (systemType == null
			       ? $"no system uses {invalidType.Name}"
			       : $"system {systemType.Name} not allowed to use {invalidType.Name}"))
	{
		Feature     = feature;
		SystemType  = systemType;
		InvalidType = invalidType;
	}
}

public static class BlahFeaturesValidator
{
	public static void Validate(BlahFeatureBase feature)
	{
		var usedService   = feature.Services == null ? null : new HashSet<Type>();
		var usedConsumers = feature.Consumers == null ? null : new HashSet<Type>();
		var usedProducers = feature.Producers == null ? null : new HashSet<Type>();

		foreach (var rawSystemType in feature.Systems)
		{
			var systemType = rawSystemType;
			while (systemType?.Namespace?.StartsWith("System") == false)
			{
				var fields = systemType.GetFields(
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic
				);
				foreach (var field in fields)
				{
					var fieldType = field.FieldType;

					if (fieldType.BaseType == typeof(BlahServiceBase))
					{
						usedService?.Add(fieldType);
						if (feature.Services?.Contains(fieldType) == false)
							throw new BlahFeatureValidatorException(
								feature,
								systemType,
								fieldType
							);
					}

					if (!fieldType.IsGenericType)
						continue;
					var genBaseType = fieldType.GetGenericTypeDefinition();
					var genArgType  = fieldType.GenericTypeArguments[0];

					if (genBaseType == typeof(IBlahSignalConsumer<>) ||
					    genBaseType == typeof(IBlahDataConsumer<>))
					{
						usedConsumers?.Add(genArgType);
						if (feature.Consumers?.Contains(genArgType) != true)
							throw new BlahFeatureValidatorException(
								feature,
								systemType,
								genArgType
							);
					}

					if (genBaseType == typeof(IBlahSignalProducer<>) ||
					    genBaseType == typeof(IBlahDataProducer<>))
					{
						usedProducers?.Add(genArgType);
						if (feature.Producers?.Contains(genArgType) != true)
							throw new BlahFeatureValidatorException(
								feature,
								systemType,
								genArgType
							);
					}
				}
				systemType = systemType.BaseType;
			}
		}

		if (usedService != null && feature.Services != null)
		{
			foreach (var service in feature.Services)
				if (!usedService.Contains(service))
					throw new BlahFeatureValidatorException(feature, null, service);
		}
		if (usedConsumers != null && feature.Consumers != null)
		{
			foreach (var consumer in feature.Consumers)
				if (!usedConsumers.Contains(consumer))
					throw new BlahFeatureValidatorException(feature, null, consumer);
		}
		if (usedProducers != null && feature.Producers != null)
		{
			foreach (var producer in feature.Producers)
				if (!usedProducers.Contains(producer))
					throw new BlahFeatureValidatorException(feature, null, producer);
		}
	}
}
}
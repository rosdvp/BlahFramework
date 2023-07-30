using System;
using System.Reflection;
using Blah.Pools;
using Blah.Services;

namespace Blah.Features
{
public class BlahFeatureValidatorException : Exception
{
	public readonly BlahFeatureBase Feature;
	public readonly Type            SystemType;
	public readonly Type            InvalidType;

	public BlahFeatureValidatorException(BlahFeatureBase feature, Type systemType, Type invalidType)
		: base($"in feature {feature.GetType().Name}, " +
		       $"system {systemType.Name}, " +
		       $"type {invalidType} is not allowed."
		)
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
		foreach (var rawSystemType in feature.Systems)
		{
			var systemType = rawSystemType;
			while (systemType?.Namespace?.StartsWith("System.") == false)
			{
				var fields = systemType.GetFields(
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic
				);
				foreach (var field in fields)
				{
					var fieldType = field.FieldType;

					if (fieldType.BaseType == typeof(BlahServiceBase) && feature.Services?.Contains(fieldType) == false)
						throw new BlahFeatureValidatorException(feature, systemType, fieldType);

					if (!fieldType.IsGenericType)
						continue;
					var genBaseType = fieldType.GetGenericTypeDefinition();
					var genArgType  = fieldType.GenericTypeArguments[0];

					if (genBaseType == typeof(IBlahSignalConsumer<>) &&
					    feature.Consumers?.Contains(genArgType) != true)
						throw new BlahFeatureValidatorException(feature, systemType, genArgType);

					if (genBaseType == typeof(IBlahSignalProducer<>) &&
					    feature.Producers?.Contains(genArgType) != true)
						throw new BlahFeatureValidatorException(feature, systemType, genArgType);

					if (genBaseType == typeof(IBlahDataConsumer<>) &&
					    feature.Consumers?.Contains(genArgType) != true)
						throw new BlahFeatureValidatorException(feature, systemType, genArgType);

					if (genBaseType == typeof(IBlahDataProducer<>) &&
					    feature.Producers?.Contains(genArgType) != true)
						throw new BlahFeatureValidatorException(feature, systemType, genArgType);
				}
				systemType = systemType.BaseType;
			}
		}
	}
}
}
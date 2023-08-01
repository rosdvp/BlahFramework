using System;
using System.Collections.Generic;
using Blah.Reflection;

namespace Blah.Features
{
public static class BlahFeaturesValidator
{
	private static readonly HashSet<Type> _usedService   = new();
	private static readonly HashSet<Type> _usedConsumers = new();
	private static readonly HashSet<Type> _usedProducers = new();

	public static void Validate(BlahFeatureBase feature)
	{
		if (feature.Systems == null)
			return;
		
		_usedService.Clear();
		_usedConsumers.Clear();
		_usedProducers.Clear();

		foreach (var system in feature.Systems)
		foreach (var (kind, type) in BlahReflection.EnumerateSystemFields(system))
			if (kind is BlahReflection.EKind.Service)
			{
				_usedService.Add(type);
				if (feature.Services?.Contains(type) != true)
					throw new BlahFeatureValidatorException(feature, system, type);
			}
			else if (kind is BlahReflection.EKind.SignalConsumer or BlahReflection.EKind.DataConsumer)
			{
				_usedConsumers.Add(type);
				if (feature.Consumers?.Contains(type) != true)
					throw new BlahFeatureValidatorException(feature, system, type);
			}
			else if (kind is BlahReflection.EKind.SignalProducer or BlahReflection.EKind.DataProducer)
			{
				_usedProducers.Add(type);
				if (feature.Producers?.Contains(type) != true)
					throw new BlahFeatureValidatorException(feature, system, type);
			}
		
		if (feature.Services != null)
			foreach (var service in feature.Services)
				if (!_usedService.Contains(service))
					throw new BlahFeatureValidatorException(feature, null, service);
		if (feature.Consumers != null)
			foreach (var consumer in feature.Consumers)
				if (!_usedConsumers.Contains(consumer))
					throw new BlahFeatureValidatorException(feature, null, consumer);
		if (feature.Producers != null)
			foreach (var producer in feature.Producers)
				if (!_usedProducers.Contains(producer))
					throw new BlahFeatureValidatorException(feature, null, producer);
	}
}


public class BlahFeatureValidatorException : Exception
{
	public readonly BlahFeatureBase Feature;
	public readonly Type            SystemType;
	public readonly Type            InvalidType;

	internal BlahFeatureValidatorException(BlahFeatureBase feature, Type systemType, Type invalidType)
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
}
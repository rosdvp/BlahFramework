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

		BlahReflection.FindFeatureDependencies(
			feature,
			_usedService,
			_usedConsumers,
			_usedProducers,
			true
		);

		if (feature.Services == null || !_usedService.SetEquals(feature.Services))
		{
			if (_usedService != null)
				foreach (var service in _usedService)
					if (feature.Services?.Contains(service) != true)
						throw new BlahFeatureValidatorException(feature, service, true);
			if (feature.Services != null)
				foreach (var service in feature.Services)
					if (!_usedService.Contains(service))
						throw new BlahFeatureValidatorException(feature, service, false);
		}

		_usedConsumers.ExceptWith(_usedProducers);
		if (feature.ConsumingFromOutside == null || !_usedConsumers.SetEquals(feature.ConsumingFromOutside))
		{
			foreach (var consumer in _usedConsumers)
				if (feature.ConsumingFromOutside?.Contains(consumer) != true)
					throw new BlahFeatureValidatorException(feature, consumer, true);
			if (feature.ConsumingFromOutside != null)
				foreach (var consumer in feature.ConsumingFromOutside)
					if (!_usedConsumers.Contains(consumer))
						throw new BlahFeatureValidatorException(feature, consumer, false);
		}

		if (feature.Producing == null || !_usedProducers.SetEquals(feature.Producing))
		{
			foreach (var producer in _usedProducers)
				if (feature.Producing?.Contains(producer) != true)
					throw new BlahFeatureValidatorException(feature, producer, true);
			if (feature.Producing != null)
				foreach (var producer in feature.Producing)
					if (!_usedProducers.Contains(producer))
						throw new BlahFeatureValidatorException(feature, producer, false);
		}
	}
}


public class BlahFeatureValidatorException : Exception
{
	public readonly BlahFeatureBase Feature;
	public readonly Type            InvalidType;

	internal BlahFeatureValidatorException(BlahFeatureBase feature, Type invalidType, bool isNotUsed)
		: base($"in feature {feature.GetType().Name}, " +
		       (isNotUsed 
			       ? $"no system uses {invalidType.Name}"
			       : $"usage of {invalidType.Name} is not allowed"))
	{
		Feature     = feature;
		InvalidType = invalidType;
	}
}
}
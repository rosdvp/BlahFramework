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

		if (!IsSame(_usedService, feature.Services))
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
		if (!IsSame(_usedConsumers, feature.ConsumingFromOutside))
		{
			foreach (var consumer in _usedConsumers)
				if (feature.ConsumingFromOutside?.Contains(consumer) != true)
					throw new BlahFeatureValidatorException(feature, consumer, true);
			if (feature.ConsumingFromOutside != null)
				foreach (var consumer in feature.ConsumingFromOutside)
					if (!_usedConsumers.Contains(consumer))
						throw new BlahFeatureValidatorException(feature, consumer, false);
		}

		if (!IsSame(_usedProducers, feature.Producing))
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

	private static bool IsSame(HashSet<Type> a, HashSet<Type> b) =>
		a == null && b == null ||
		a is { Count: 0 } && b == null ||
		a == null && b is { Count: 0 } ||
		a != null && b != null && a.SetEquals(b);
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
using System;
using System.Collections.Generic;
using System.Reflection;
using Blah.Ordering.Attributes;
using Blah.Pools;

namespace Blah.Ordering
{
public static class BlahOrderer
{
	public static void Order(ref List<Type> systems)
	{
		if (systems == null || systems.Count == 1)
			return;
		
		var cache = new Cache();

		foreach (var system in systems)
			FillConsumersProducers(cache, system);

		foreach (var system in systems)
		{
			FillAfterSystemsByConsumersProducers(cache, system);
			FillAfterSystemsByAttributes(cache, system);
		}

		systems = BlahOrdererTopologicalSort.Sort(systems, cache.GetCopySystemToSystemsGoingBefore());
	}

	private static void FillConsumersProducers(Cache cache, Type system)
	{
		var fields = system.GetFields(
			BindingFlags.Instance |
			BindingFlags.NonPublic |
			BindingFlags.Public
		);
		foreach (var field in fields)
		{
			var fieldType = field.FieldType;
			if (fieldType.IsGenericType)
			{
				var fieldGenType = fieldType.GetGenericTypeDefinition();
				if (fieldGenType == typeof(IBlahSignalConsumer<>))
					cache.AddConsumingSystem(system, fieldType.GenericTypeArguments[0]);
				else if (fieldGenType == typeof(IBlahSignalProducer<>))
					cache.AddProducingSystem(system, fieldType.GenericTypeArguments[0]);
			}
		}
	}

	private static void FillAfterSystemsByAttributes(Cache cache, Type system)
	{
		foreach (var attr in system.GetCustomAttributes())
			if (attr is BlahAfterAttribute afterAttr)
			{
				cache.AddSystemsDependency(afterAttr.SystemGoingBefore, system);
			}
			else if (attr is BlahBeforeAttribute beforeAttr)
			{
				cache.AddSystemsDependency(system, beforeAttr.SystemGoingAfter);
			}
	}

	private static void FillAfterSystemsByConsumersProducers(Cache cache, Type system)
	{
		var consumingTypes = cache.GetConsumingTypesOfSystem(system);
		if (consumingTypes == null)
			return;

		foreach (var type in consumingTypes)
		{
			var producingSystems = cache.GetProducingSystems(type);
			if (producingSystems != null)
				cache.AddSystemsDependency(producingSystems, system);
		}
	}
	

	private class Cache
	{
		private Dictionary<Type, HashSet<Type>> _systemToConsumingTypes  = new();
		private Dictionary<Type, HashSet<Type>> _consumingTypeToSystems = new();
		
		private Dictionary<Type, HashSet<Type>> _systemToProducingTypes = new();
		private Dictionary<Type, HashSet<Type>> _producingTypeToSystems = new();
	
		private Dictionary<Type, List<Type>> _systemToSystemsGoingBefore = new();
		

		public void AddConsumingSystem(Type system, Type type)
		{
			if (_systemToConsumingTypes.TryGetValue(system, out var consumingTypes))
				consumingTypes.Add(type);
			else
				_systemToConsumingTypes[system] = new HashSet<Type> { type };

			if (_consumingTypeToSystems.TryGetValue(type, out var consumingSystems))
				consumingSystems.Add(system);
			else
				_consumingTypeToSystems[type] = new HashSet<Type> { system };
		}
		
		public HashSet<Type> GetConsumingTypesOfSystem(Type system)
		{
			return _systemToConsumingTypes.TryGetValue(system, out var consumingTypes)
				? consumingTypes
				: null;
		}
		

		public void AddProducingSystem(Type system, Type type)
		{
			if (_systemToProducingTypes.TryGetValue(system, out var producingTypes))
				producingTypes.Add(type);
			else
				_systemToProducingTypes[system] = new HashSet<Type> { type };

			if (_producingTypeToSystems.TryGetValue(type, out var producingSystems))
				producingSystems.Add(system);
			else
				_producingTypeToSystems[type] = new HashSet<Type> { system };
		}

		public HashSet<Type> GetProducingSystems(Type producingType)
		{
			return _producingTypeToSystems.TryGetValue(producingType, out var producingSystems)
				? producingSystems
				: null;
		}


		public void AddSystemsDependency(Type beforeSystem, Type afterSystem)
		{
			if (_systemToSystemsGoingBefore.TryGetValue(afterSystem, out var systemsGoingBefore))
				systemsGoingBefore.Add(beforeSystem);
			else
				_systemToSystemsGoingBefore[afterSystem] = new List<Type> { beforeSystem };
		}

		public void AddSystemsDependency(IReadOnlyCollection<Type> beforeSystems, Type afterSystem)
		{
			if (_systemToSystemsGoingBefore.TryGetValue(afterSystem, out var systemsGoingBefore))
				systemsGoingBefore.AddRange(beforeSystems);
			else
				_systemToSystemsGoingBefore[afterSystem] = new List<Type>(beforeSystems);
		}

		public Dictionary<Type, List<Type>> GetCopySystemToSystemsGoingBefore()
		{
			var dict = new Dictionary<Type, List<Type>>();
			foreach (var pair in _systemToSystemsGoingBefore)
				dict[pair.Key] = new List<Type>(pair.Value);
			return dict;
		}
	}
}
}
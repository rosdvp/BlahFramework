using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Reflection;

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
			foreach (var (kind, type) in BlahReflection.EnumerateSystemFields(system))
				if (kind == BlahReflection.EKind.SignalConsumer)
					cache.AddConsumingSystem(system, type);
				else if (kind == BlahReflection.EKind.SignalProducer)
					cache.AddProducingSystem(system, type);

		foreach (var system in systems)
		{
			foreach (var attr in BlahReflection.EnumerateAttributes(system))
				if (attr is BlahAfterAttribute afterAttr)
					cache.AddSystemsDependency(afterAttr.PrevSystem, system);
				else if (attr is BlahBeforeAttribute beforeAttr)
					cache.AddSystemsDependency(system, beforeAttr.NextSystem);
			
			var consumingTypes = cache.GetConsumingTypesOfSystem(system);
			if (consumingTypes != null)
				foreach (var type in consumingTypes)
				{
					var producingSystems = cache.GetProducingSystems(type);
					if (producingSystems != null)
						cache.AddSystemsDependency(producingSystems, system);
				}
		}

		systems = BlahOrdererSort.Sort(systems, cache.GetSystemToPrevSystemsMap());
	}


	private class Cache
	{
		private Dictionary<Type, HashSet<Type>> _systemToConsumingTypes  = new();
		private Dictionary<Type, HashSet<Type>> _consumingTypeToSystems = new();
		
		private Dictionary<Type, HashSet<Type>> _systemToProducingTypes = new();
		private Dictionary<Type, HashSet<Type>> _producingTypeToSystems = new();

		private Dictionary<Type, List<Type>> _systemToPrevSystems = new();
		

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


		public void AddSystemsDependency(Type prevSystem, Type nextSystem)
		{
			if (_systemToPrevSystems.TryGetValue(nextSystem, out var prevSystems))
				prevSystems.Add(prevSystem);
			else
				_systemToPrevSystems[nextSystem] = new List<Type> { prevSystem };
		}

		public void AddSystemsDependency(IReadOnlyCollection<Type> prevSystems, Type nextSystem)
		{
			if (_systemToPrevSystems.TryGetValue(nextSystem, out var cachedPrevSystems))
				cachedPrevSystems.AddRange(prevSystems);
			else
				_systemToPrevSystems[nextSystem] = new List<Type>(prevSystems);
		}

		public Dictionary<Type, List<Type>> GetSystemToPrevSystemsMap()
			=> _systemToPrevSystems;
	}
}
}
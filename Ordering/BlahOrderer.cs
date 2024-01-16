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
				if (kind is BlahReflection.EKind.SignalConsumer or BlahReflection.EKind.SoloSignalConsumer)
					cache.AddConsumingSystem(system, type);
				else if (kind is BlahReflection.EKind.SignalProducer or BlahReflection.EKind.SoloSignalProducer)
					cache.AddProducingSystem(system, type);

		foreach (var system in systems)
		{
			foreach (var attr in BlahReflection.EnumerateAttributes(system))
				if (attr is BlahAfterAttribute afterAttr)
					cache.AddSystemsDependency(afterAttr.PrevSystem, system);
				else if (attr is BlahBeforeAttribute beforeAttr)
					cache.AddSystemsDependency(system, beforeAttr.NextSystem);
				else if (attr is BlahAfterAllAttribute afterAllAttr)
					cache.SetSystemPriority(system, -afterAllAttr.Priority - 1);
				else if (attr is BlahBeforeAllAttribute beforeAllAttr)
					cache.SetSystemPriority(system, beforeAllAttr.Priority + 1);
			
			var consumingTypes = cache.GetConsumingTypesOfSystem(system);
			if (consumingTypes != null)
				foreach (var type in consumingTypes)
				{
					var producingSystems = cache.GetProducingSystems(type);
					if (producingSystems != null)
						cache.AddSystemsDependency(producingSystems, system);
				}
		}

		foreach (var systemA in systems)
			if (cache.TryGetSystemPriority(systemA, out int priorityA))
			{
				foreach (var systemB in systems)
					if (systemA != systemB)
					{
						if (cache.TryGetSystemPriority(systemB, out int priorityB))
						{
							if (priorityA < priorityB)
								cache.AddSystemsDependency(systemB, systemA);
							// dependency systemB -> systemA will be added during next foreach step
						}
						else
						{
							if (priorityA > 0)
								cache.AddSystemsDependency(systemA, systemB);
							else
								cache.AddSystemsDependency(systemB, systemA);
						}
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

		// >0 - before all, <0 - after all 
		private Dictionary<Type, int> _systemToPriority = new(); 
		
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


		
		public void SetSystemPriority(Type system, int priority)
		{
			_systemToPriority[system] = priority;
		}

		public bool TryGetSystemPriority(Type system, out int priority)
		{
			return _systemToPriority.TryGetValue(system, out priority);
		}
		
		
			
		public Dictionary<Type, List<Type>> GetSystemToPrevSystemsMap()
			=> _systemToPrevSystems;
	}
}
}
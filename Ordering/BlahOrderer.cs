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

		// 1. collect consumings/producings
		foreach (var system in systems)
			foreach (var (kind, type) in BlahReflection.EnumerateSystemFields(system))
				if (kind is BlahReflection.EKind.SignalConsumer or BlahReflection.EKind.SoloSignalConsumer)
					cache.AddSystemConsumingType(system, type);
				else if (kind is BlahReflection.EKind.SignalProducer or BlahReflection.EKind.SoloSignalProducer)
					cache.AddSystemProducingType(system, type);

		// 2. add dependencies from consumings/producings
		foreach (var system in systems)
			if (cache.TryGetConsumingTypesOfSystem(system, out var consumingTypes))
				foreach (var type in consumingTypes)
					if (cache.TryGetSystemsThatProduceType(type, out var producingSystems))
						cache.AddSystemsDependency(producingSystems, system);

		// 3. add dependencies from specified attributes
		foreach (var system in systems)
		foreach (var attr in BlahReflection.EnumerateAttributes(system))
			if (attr is BlahAfterAttribute afterAttr)
				cache.AddSystemsDependency(afterAttr.PrevSystem, system);
			else if (attr is BlahBeforeAttribute beforeAttr)
				cache.AddSystemsDependency(system, beforeAttr.NextSystem);

		// 4. collect BlahAfterAll/BeforeAll priorities and propagate them
		foreach (var system in systems)
		foreach (var attr in BlahReflection.EnumerateAttributes(system))
			if (attr is BlahAfterAllAttribute afterAllAttr)
			{
				int priority = -afterAllAttr.Priority - 1;
				cache.SetSystemPriority(system, priority);

				foreach (var nextSystem in cache.EnumerateAllNextSystems(system))
				{
					if (cache.TryGetSystemPriority(nextSystem, out int nextSystemPriority))
					{
						if (nextSystemPriority <= priority)
							continue;
						if (nextSystemPriority > 0)
							throw new Exception(
								$"system {nextSystem} is BlahBeforeAll({nextSystemPriority}), " +
								$"but it depends on {system} BlahAfterAll({-priority})"
							);
					}
					cache.SetSystemPriority(nextSystem, priority);
				}
			}
			else if (attr is BlahBeforeAllAttribute beforeAllAttr)
			{
				int priority = beforeAllAttr.Priority + 1;
				cache.SetSystemPriority(system, priority);

				foreach (var prevSystem in cache.EnumerateAllPrevSystems(system))
				{
					if (cache.TryGetSystemPriority(prevSystem, out int prevSystemPriority))
					{
						if (prevSystemPriority >= priority)
							continue;
						if (prevSystemPriority < 0)
							throw new Exception(
								$"system {prevSystem} is BlahAfterAll({-prevSystemPriority}), " +
								$"but it depends on {system} BlahBeforeAll({priority})"
							);
					}
					cache.SetSystemPriority(prevSystem, priority);
				}
			}

		// 4. add dependencies from priorities
		foreach (var systemA in systems)
		{
			if (!cache.TryGetSystemPriority(systemA, out int priorityA))
				continue;
			foreach (var systemB in systems)
			{
				if (systemA == systemB)
					continue;
				if (cache.TryGetSystemPriority(systemB, out int priorityB))
				{
					if (priorityA < priorityB)
						cache.AddSystemsDependency(systemB, systemA);
					// dependency systemA -> systemB will be added during next foreach step
				}
				else
				{
					if (priorityA > 0) // system a before all
					{
						if (!cache.IsDependencyExists(systemB, systemA))
							cache.AddSystemsDependency(systemA, systemB);
					}
					else // system a after all
					{
						if (!cache.IsDependencyExists(systemA, systemB))
							cache.AddSystemsDependency(systemB, systemA);
					}
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
		
		private Dictionary<Type, HashSet<Type>> _systemToPrevSystems = new();
		private Dictionary<Type, HashSet<Type>> _systemToNextSystems = new();
		

		public void AddSystemConsumingType(Type system, Type type)
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

		public bool TryGetConsumingTypesOfSystem(Type system, out HashSet<Type> consumingTypes)
		{
			return _systemToConsumingTypes.TryGetValue(system, out consumingTypes);
		}
        

		public void AddSystemProducingType(Type system, Type type)
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

		public bool TryGetSystemsThatProduceType(Type producingType, out HashSet<Type> producingSystems)
		{
			return _producingTypeToSystems.TryGetValue(producingType, out producingSystems);
		}

        
		public void AddSystemsDependency(Type prevSystem, Type nextSystem)
		{
			if (_systemToPrevSystems.TryGetValue(nextSystem, out var prevSystems))
				prevSystems.Add(prevSystem);
			else
				_systemToPrevSystems[nextSystem] = new HashSet<Type> { prevSystem };

			if (_systemToNextSystems.TryGetValue(prevSystem, out var nextSystems))
				nextSystems.Add(nextSystem);
			else
				_systemToNextSystems[prevSystem] = new HashSet<Type> { nextSystem };
		}

		public void AddSystemsDependency(IReadOnlyCollection<Type> prevSystems, Type nextSystem)
		{
			if (_systemToPrevSystems.TryGetValue(nextSystem, out var cachedPrevSystems))
				cachedPrevSystems.UnionWith(prevSystems);
			else
				_systemToPrevSystems[nextSystem] = new HashSet<Type>(prevSystems);

			foreach (var prevSystem in prevSystems)
				if (_systemToNextSystems.TryGetValue(prevSystem, out var nextSystems))
					nextSystems.Add(nextSystem);
				else
					_systemToNextSystems[prevSystem] = new HashSet<Type> { nextSystem };
		}

		public bool IsDependencyExists(Type prevSystem, Type nextSystem)
		{
			return _systemToPrevSystems.TryGetValue(nextSystem, out var prevSystems) &&
			       prevSystems.Contains(prevSystem);
		}


		public void SetSystemPriority(Type system, int priority)
		{
			_systemToPriority[system] = priority;
		}

		public bool TryGetSystemPriority(Type system, out int priority)
		{
			return _systemToPriority.TryGetValue(system, out priority);
		}
		
		public IEnumerable<Type> EnumerateAllPrevSystems(Type system)
		{
			if (!_systemToPrevSystems.TryGetValue(system, out var prevSystems))
				yield break;
			foreach (var prevSystem in prevSystems)
			{
				yield return prevSystem;
				foreach (var s in EnumerateAllPrevSystems(prevSystem))
					yield return s;
			}
		}

		public IEnumerable<Type> EnumerateAllNextSystems(Type system)
		{
			if (!_systemToNextSystems.TryGetValue(system, out var nextSystems))
				yield break;
			foreach (var nextSystem in nextSystems)
			{
				yield return nextSystem;
				foreach (var s in EnumerateAllNextSystems(nextSystem))
					yield return s;
			}
		}
		
		public Dictionary<Type, HashSet<Type>> GetSystemToPrevSystemsMap()
			=> _systemToPrevSystems;
	}
}
}
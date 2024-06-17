using System;
using System.Collections.Generic;
using Blah.Ordering.Attributes;
using Blah.Reflection;

namespace Blah.Ordering
{
public static class BlahOrderer
{
	public static void Order(ref List<Type> systems, bool isVerboseSanitizing = false)
	{
		if (systems == null || systems.Count == 1)
			return;

		var cache = new Cache();
		AddDependenciesFromConsumingsProducings(cache, systems);
		if (isVerboseSanitizing)
			BlahOrdererTpSort.ThrowOnCyclic(
				systems,
				cache.GetSystemToPrevSystemsMap(),
				"sanitize after consuming/producing",
				null
			);
		AddDirectBeforeAfterDependencies(cache, systems);
		if (isVerboseSanitizing)
			BlahOrdererTpSort.ThrowOnCyclic(
				systems,
				cache.GetSystemToPrevSystemsMap(),
				"sanitize after BlahAfter/BlahBefore",
				null
			);
		SplitIntoPriorities(cache, systems);


		var systemToPrevSystems = cache.GetSystemToPrevSystemsMap();
		var systemToVisitState  = new Dictionary<Type, bool>();
		var result              = new List<Type>();

		foreach (var (priority, beforeAllSystems) in cache.EnumerateBeforeAllSystems())
			BlahOrdererTpSort.Sort(
				priority,
				beforeAllSystems,
				systemToPrevSystems,
				systemToVisitState,
				result
			);

		var nonPrioritySystems = new HashSet<Type>();
		foreach (var system in systems)
			if (cache.GetSystemPriority(system) == 0)
				nonPrioritySystems.Add(system);
		BlahOrdererTpSort.Sort(
			0,
			nonPrioritySystems,
			systemToPrevSystems,
			systemToVisitState,
			result
		);

		foreach (var (priority, afterAllSystems) in cache.EnumerateAfterAllSystems())
			BlahOrdererTpSort.Sort(
				priority,
				afterAllSystems,
				systemToPrevSystems,
				systemToVisitState,
				result
			);
		
		BlahOrdererTpSort.ThrowOnFinalCheck(result, systemToPrevSystems);

		systems = result;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private static void AddDependenciesFromConsumingsProducings(Cache cache, List<Type> systems)
	{
		foreach (var system in systems)
		foreach (var (kind, type) in BlahReflection.EnumerateSystemFields(system))
			if (kind is BlahReflection.EKind.SignalConsumer or BlahReflection.EKind.SoloSignalConsumer)
				cache.AddSystemConsumingType(system, type);
			else if (kind is BlahReflection.EKind.SignalProducer or BlahReflection.EKind.SoloSignalProducer)
				cache.AddSystemProducingType(system, type);

		foreach (var system in systems)
			if (cache.TryGetTypesThatSystemConsume(system, out var consumingTypes))
				foreach (var type in consumingTypes)
					if (cache.TryGetSystemsThatProduceType(type, out var producingSystems))
						cache.AddSystemsDependency(producingSystems, system);
	}

	private static void AddDirectBeforeAfterDependencies(Cache cache, List<Type> systems)
	{
		foreach (var system in systems)
		foreach (var attr in BlahReflection.EnumerateAttributes(system))
			if (attr is BlahAfterAttribute afterAttr)
				cache.AddSystemsDependency(afterAttr.PrevSystem, system);
			else if (attr is BlahBeforeAttribute beforeAttr)
				cache.AddSystemsDependency(system, beforeAttr.NextSystem);
	}

	private static void SplitIntoPriorities(Cache cache, List<Type> systems)
	{
		foreach (var system in systems)
		foreach (var attr in BlahReflection.EnumerateAttributes(system))
			if (attr is BlahAfterAllAttribute afterAllAttr)
				cache.SetSystemPriority(system, -(afterAllAttr.Priority + 1));
			else if (attr is BlahBeforeAllAttribute beforeAllAttr)
				cache.SetSystemPriority(system, beforeAllAttr.Priority + 1);

		foreach (var (priority, rootSystem) in cache.EnumeratePrioritySystemPairsFromMaxToMinCopy())
			if (priority > 0)
				foreach (var prevSystem in cache.EnumerateAllPrevSystems(rootSystem))
				{
					int prevSystemPriority = cache.GetSystemPriority(prevSystem);
					if (prevSystemPriority == 0)
						cache.SetSystemPriority(prevSystem, priority);
					else if (prevSystemPriority < priority)
						throw new Exception(
							$"system {prevSystem} should go before {rootSystem} (priority: {priority}),"
							+ $" but {prevSystem} specifies priority {prevSystemPriority}"
						);
				}
			else if (priority < 0)
				foreach (var nextSystem in cache.EnumerateAllNextSystems(rootSystem))
				{
					int nextSystemPriority = cache.GetSystemPriority(nextSystem);
					if (nextSystemPriority == 0)
						cache.SetSystemPriority(nextSystem, priority);
					else if (nextSystemPriority < priority)
						throw new Exception(
							$"system {nextSystem} should go after {rootSystem} (priority: {priority}),"
							+ $" but {nextSystem} specifies priority {nextSystemPriority}"
						);
				}
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private class Cache
	{
		private Dictionary<Type, HashSet<Type>> _systemToConsumingTypes = new();
		private Dictionary<Type, HashSet<Type>> _consumingTypeToSystems = new();

		private Dictionary<Type, HashSet<Type>> _systemToProducingTypes = new();
		private Dictionary<Type, HashSet<Type>> _producingTypeToSystems = new();

		// >0 - before all, <0 - after all 
		private Dictionary<int, HashSet<Type>> _priorityToSystems = new();
		private Dictionary<Type, int>          _systemToPriority  = new();

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

		public bool TryGetTypesThatSystemConsume(Type system, out HashSet<Type> consumingTypes)
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

		public void SetSystemPriority(Type system, int priority)
		{
			if (_systemToPriority.TryGetValue(system, out int oldPriority))
				_priorityToSystems[oldPriority].Remove(system);

			_systemToPriority[system] = priority;

			if (_priorityToSystems.TryGetValue(priority, out var systems))
				systems.Add(system);
			else
				_priorityToSystems[priority] = new HashSet<Type> { system };
		}

		public int GetSystemPriority(Type system)
		{
			return _systemToPriority.GetValueOrDefault(system, 0);
		}

		public IEnumerable<(int, Type)> EnumeratePrioritySystemPairsFromMaxToMinCopy()
		{
			var result = new List<Type>();
			foreach (int priority in GetSortedPriorities())
				if (_priorityToSystems.TryGetValue(priority, out var systems))
				{
					result.AddRange(systems);
					foreach (var r in result)
						yield return (priority, r);
				}
		}

		public IEnumerable<(int, HashSet<Type>)> EnumerateBeforeAllSystems()
		{
			foreach (int priority in GetSortedPriorities())
				if (priority > 0 && _priorityToSystems.TryGetValue(priority, out var systems))
					yield return (priority, systems);
		}

		public IEnumerable<(int, HashSet<Type>)> EnumerateAfterAllSystems()
		{
			foreach (int priority in GetSortedPriorities())
				if (priority < 0 && _priorityToSystems.TryGetValue(priority, out var systems))
					yield return (priority, systems);
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
		{
			return _systemToPrevSystems;
		}


		private List<int> GetSortedPriorities()
		{
			var priorities = new List<int>();
			foreach (int priority in _priorityToSystems.Keys)
				priorities.Add(priority);
			priorities.Sort();
			priorities.Reverse();
			return priorities;
		}
	}
}
}
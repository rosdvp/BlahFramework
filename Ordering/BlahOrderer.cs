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
		AddDependenciesFromSignals(cache, systems);
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
	private static void AddDependenciesFromSignals(Cache cache, List<Type> systems)
	{
		foreach (var system in systems)
		foreach (var (kind, type) in BlahReflection.EnumerateSystemFields(system))
			if (kind is BlahReflection.EKind.SignalRead)
				cache.AddSystemSignalRead(system, type);
			else if (kind is BlahReflection.EKind.SignalWrite)
				cache.AddSystemSignalWrite(system, type);

		foreach (var system in systems)
			if (cache.TryGetSignalsThatSystemRead(system, out var consumingTypes))
				foreach (var type in consumingTypes)
					if (cache.TryGetSystemsThatWriteSignal(type, out var producingSystems))
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
		private Dictionary<Type, HashSet<Type>> _systemToReadSignals = new();
		private Dictionary<Type, HashSet<Type>> _signalToReadSystems = new();

		private Dictionary<Type, HashSet<Type>> _systemToWriteSignals = new();
		private Dictionary<Type, HashSet<Type>> _signalToWriteSystems = new();

		// >0 - before all, <0 - after all 
		private Dictionary<int, HashSet<Type>> _priorityToSystems = new();
		private Dictionary<Type, int>          _systemToPriority  = new();

		private Dictionary<Type, HashSet<Type>> _systemToPrevSystems = new();
		private Dictionary<Type, HashSet<Type>> _systemToNextSystems = new();


		public void AddSystemSignalRead(Type system, Type signal)
		{
			if (_systemToReadSignals.TryGetValue(system, out var signals))
				signals.Add(signal);
			else
				_systemToReadSignals[system] = new HashSet<Type> { signal };

			if (_signalToReadSystems.TryGetValue(signal, out var readSystems))
				readSystems.Add(system);
			else
				_signalToReadSystems[signal] = new HashSet<Type> { system };
		}

		public bool TryGetSignalsThatSystemRead(Type system, out HashSet<Type> signals)
		{
			return _systemToReadSignals.TryGetValue(system, out signals);
		}


		public void AddSystemSignalWrite(Type system, Type signal)
		{
			if (_systemToWriteSignals.TryGetValue(system, out var signals))
				signals.Add(signal);
			else
				_systemToWriteSignals[system] = new HashSet<Type> { signal };

			if (_signalToWriteSystems.TryGetValue(signal, out var writeSystems))
				writeSystems.Add(system);
			else
				_signalToWriteSystems[signal] = new HashSet<Type> { system };
		}

		public bool TryGetSystemsThatWriteSignal(Type signal, out HashSet<Type> writeSystems)
		{
			return _signalToWriteSystems.TryGetValue(signal, out writeSystems);
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
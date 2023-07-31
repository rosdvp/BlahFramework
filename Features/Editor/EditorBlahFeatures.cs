using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal static class EditorBlahFeatures
{
	[MenuItem("Blah/Validate features consumers-producers")]
	public static void EditorValidateFeaturesConsumersProducers()
	{
		var sb = new StringBuilder();
		sb.AppendLine("--- features consumers/producers report ---");
		
		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
			{
				var feature = (BlahFeatureBase)Activator.CreateInstance(type);
				try
				{
					BlahFeaturesValidator.Validate(feature);
				}
				catch (BlahFeatureValidatorException exc)
				{
					sb.AppendLine(exc.Message);
				}
			}

		sb.AppendLine("---------------------------------");
		Debug.Log(sb.ToString());
	}
	
	[MenuItem("Blah/Report unused features")]
	public static void EditorReportUnUsedFeatures()
	{
		Type contextType   = null;
		var  featuresInProject = new HashSet<Type>();
		
		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == typeof(BlahContextBase))
				contextType = type;
			else if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);
		
		object context = Activator.CreateInstance(contextType);
		var prop = typeof(BlahContextBase).GetProperty(
			"FeaturesBySystemsGroups",
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic
		);
		var featuresInContext = (Dictionary<int, List<BlahFeatureBase>>)prop.GetValue(context);
		foreach ((int groupId, var features) in featuresInContext)
		foreach (var feature in features)
			featuresInProject.Remove(feature.GetType());

		var sb = new StringBuilder();
		sb.AppendLine("--- unused features report ---");
		foreach (var feature in featuresInProject)
			sb.AppendLine(feature.Name);
		sb.AppendLine("------------------------------");
		Debug.Log(sb.ToString());
	}

	
	[MenuItem("Blah/Report unused systems")]
	public static void EditorReportUnUsedSystems()
	{
		var featuresInProject = new HashSet<Type>();
		var systemsInProject  = new HashSet<Type>();

		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);
			else if (type.GetInterface("IBlahInitSystem") != null ||
			         type.GetInterface("IBlahRunSystem") != null)
				systemsInProject.Add(type);

		foreach (var feature in featuresInProject)
		{
			object featureObj = Activator.CreateInstance(feature);
			var prop = typeof(BlahFeatureBase).GetProperty(
				"Systems",
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			var systems = (IReadOnlyList<Type>)prop.GetValue(featureObj);
			if (systems != null)
				foreach (var system in systems)
					systemsInProject.Remove(system);
		}

		var sb = new StringBuilder();
		sb.AppendLine("--- unused systems report ---");
		foreach (var system in systemsInProject)
			sb.AppendLine(system.Name);
		sb.AppendLine("------------------------------");
		Debug.Log(sb.ToString());
	}
	
	
	[MenuItem("Blah/Report duplicating systems")]
	public static void EditorReportDuplicatingSystems()
	{
		var systemsInProject  = new HashSet<Type>();
		var systemsDuplicates = new HashSet<Type>();

		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
			{
				object featureObj = Activator.CreateInstance(type);
				var prop = typeof(BlahFeatureBase).GetProperty(
					"Systems",
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic
				);
				var systems = (IReadOnlyList<Type>)prop.GetValue(featureObj);
				if (systems != null)
					foreach (var system in systems)
					{
						if (systemsInProject.Contains(system))
							systemsDuplicates.Add(system);
						else
							systemsInProject.Add(system);
					}
			}

		var sb = new StringBuilder();
		sb.AppendLine("--- duplicating systems report ---");
		foreach (var system in systemsDuplicates)
			sb.AppendLine(system.Name);
		sb.AppendLine("------------------------------");
		Debug.Log(sb.ToString());
	}
	
	

	internal static IEnumerable<Type> EnumerateGameTypes()
	{
		var assemblies   = AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			if (!assembly.FullName.StartsWith("Unity") &&
			    !assembly.FullName.StartsWith("System") &&
			    !assembly.FullName.StartsWith("Blah"))
			{
				foreach (var type in assembly.GetTypes())
					yield return type;
			}
		}
	}
}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Blah.Features;
using Blah.Systems;
using UnityEditor;
using UnityEngine;

namespace Blah.Editor
{
internal static class BlahEditorFeaturesValidation
{
	[MenuItem("Blah/Report/Features issues")]
	public static void ReportFeaturesIssues()
	{
		var sb = new StringBuilder();
		sb.AppendLine("--- features issues report ---");
		foreach (var feature in BlahEditorHelper.EnumerateGameFeatures())
			try
			{
				BlahFeaturesValidator.Validate(feature);
			}
			catch (BlahFeatureValidatorException exc)
			{
				sb.AppendLine(exc.Message);
			}
		sb.AppendLine("---------------------------------");
		Debug.Log(sb.ToString());
	}
	
	[MenuItem("Blah/Report/Unused features")]
	public static void ReportUnUsedFeatures()
	{
		Type contextType   = null;
		var  featuresInProject = new HashSet<Type>();
		
		foreach (var type in BlahEditorHelper.EnumerateGameTypes())
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

	
	[MenuItem("Blah/Report/Unused systems")]
	public static void ReportUnUsedSystems()
	{
		var featuresInProject = new HashSet<Type>();
		var systemsInProject  = new HashSet<Type>();

		foreach (var type in BlahEditorHelper.EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);
			else if (type.GetInterface(nameof(IBlahInitSystem)) != null ||
			         type.GetInterface(nameof(IBlahRunSystem)) != null ||
			         type.GetInterface(nameof(IBlahResumePauseSystem)) != null)
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
	
	
	[MenuItem("Blah/Report/Duplicating systems")]
	public static void ReportDuplicatingSystems()
	{
		var systemsInProject  = new HashSet<Type>();
		var systemsDuplicates = new HashSet<Type>();

		foreach (var type in BlahEditorHelper.EnumerateGameTypes())
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
}
}
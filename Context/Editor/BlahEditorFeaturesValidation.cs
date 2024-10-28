using System;
using System.Collections.Generic;
using System.Text;
using Blah.Reflection;
using Blah.Systems;
using UnityEditor;
using UnityEngine;

namespace Blah.Context.Editor
{
internal static class BlahEditorFeaturesValidation
{
	[MenuItem("Blah/Framework/Report unused features")]
	public static void ReportUnUsedFeatures()
	{
		Type contextType   = null;
		var  featuresInProject = new HashSet<Type>();
		
		foreach (var type in BlahReflection.EnumerateGameTypes())
			if (type.BaseType == typeof(BlahContextBase))
				contextType = type;
			else if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);
		
		var context = (BlahContextBase)Activator.CreateInstance(contextType);
		
		var featuresGroups = context.FeaturesGroups;
		foreach ((int groupId, var features) in featuresGroups)
		foreach (var feature in features)
			featuresInProject.Remove(feature.GetType());

		var bgFeatures = context.BackgroundFeatures;
		if (bgFeatures != null)
			foreach (var bgFeature in bgFeatures)
				featuresInProject.Remove(bgFeature.GetType());

		var sb = new StringBuilder();
		sb.AppendLine("--- unused features report ---");
		foreach (var feature in featuresInProject)
			sb.AppendLine(feature.Name);
		sb.AppendLine("------------------------------");
		Debug.Log(sb.ToString());
	}

	
	[MenuItem("Blah/Framework/Report unused systems")]
	public static void ReportUnUsedSystems()
	{
		var featuresInProject = new HashSet<Type>();
		var systemsInProject  = new HashSet<Type>();
		
		foreach (var type in BlahReflection.EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);
			else if (typeof(IBlahSystem).IsAssignableFrom(type))
				systemsInProject.Add(type);

		foreach (var featureType in featuresInProject)
		{
			var feature = (BlahFeatureBase)Activator.CreateInstance(featureType);
			if (feature.Systems != null)
				foreach (var system in feature.Systems)
					systemsInProject.Remove(system.GetType());
		}

		var sb = new StringBuilder();
		sb.AppendLine("--- unused systems report ---");
		foreach (var system in systemsInProject)
			sb.AppendLine(system.Name);
		sb.AppendLine("------------------------------");
		Debug.Log(sb.ToString());
	}
	
	
	[MenuItem("Blah/Framework/Report duplicating systems")]
	public static void ReportDuplicatingSystems()
	{
		var systemsInProject  = new HashSet<Type>();
		var systemsDuplicates = new HashSet<Type>();

		foreach (var type in BlahReflection.EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
			{
				var feature = (BlahFeatureBase)Activator.CreateInstance(type);
				if (feature.Systems != null)
					foreach (var system in feature.Systems)
						if (!systemsInProject.Add(system.GetType()))
							systemsDuplicates.Add(system.GetType());
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
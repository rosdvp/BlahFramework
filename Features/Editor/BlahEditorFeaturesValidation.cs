using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Blah.Reflection;
using Blah.Systems;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal static class BlahEditorFeaturesValidation
{
	[MenuItem("Blah/Framework/Report unused features")]
	public static void ReportUnUsedFeatures()
	{
		var featuresInProject = new HashSet<Type>();
		
		foreach (var type in BlahReflection.EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
				featuresInProject.Add(type);

		var context = BlahReflection.InstantiateGameTypeWithBaseType<BlahContextBase>();
		
		foreach (var (_, features) in context.FeaturesGroups)
		foreach (var feature in features)
			featuresInProject.Remove(feature.GetType());

		if (context.BackgroundFeatures != null)
			foreach (var bgFeature in context.BackgroundFeatures)
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
			else if (type.GetInterface(nameof(IBlahInitSystem)) != null ||
			         type.GetInterface(nameof(IBlahRunSystem)) != null ||
			         type.GetInterface(nameof(IBlahPauseSystem)) != null ||
			         type.GetInterface(nameof(IBlahResumeSystem)) != null)
				systemsInProject.Add(type);

		foreach (var featureType in featuresInProject)
		{
			var feature = (BlahFeatureBase)Activator.CreateInstance(featureType);
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
				foreach (var system in feature.Systems)
				{
					if (systemsInProject.Contains(system.GetType()))
						systemsDuplicates.Add(system.GetType());
					else
						systemsInProject.Add(system.GetType());
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
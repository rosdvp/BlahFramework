using System;
using System.Collections.Generic;
using System.Text;
using Blah.Ordering;
using Blah.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blah.Context.Editor
{
internal static class BlahEditorSystemsOrdering
{
	[MenuItem("Blah/Framework/Report systems order")]
	public static void ReportSystemsOrder()
	{
		var sb = new StringBuilder();
	
		var context = BlahReflection.InstantiateGameTypesWithBaseType<BlahContextBase>();
		
		var featuresGroups =
			(Dictionary<int, List<BlahFeatureBase>>)BlahReflection.GetContextFeaturesGroups(context);
		var bgFeatures =
			(List<BlahFeatureBase>)BlahReflection.GetContextBackgroundFeatures(context);
		foreach ((int groupId, var features) in featuresGroups)
		{
			var systems = new List<Type>();
			foreach (var feature in features)
				if (feature.Systems != null)
					foreach (var system in feature.Systems)
						systems.Add(system.GetType());
			if (bgFeatures != null)
				foreach (var bgFeature in bgFeatures)
				foreach (var bgSystem in bgFeature.Systems)
					systems.Add(bgSystem.GetType());
			try
			{
				BlahOrderer.Order(ref systems, true);
				sb.Clear();
				sb.AppendLine($"group {groupId}");
				foreach (var system in systems)
					sb.AppendLine(system.Name);
				Debug.Log(sb.ToString());
			}
			catch (BlahOrdererSortingException e)
			{
				Debug.Log($"problem in group {groupId}, {e.GetFullMsg()}");
			}
		}

		sb.AppendLine("-----------------------");
		Debug.Log(sb);
	}
}
}
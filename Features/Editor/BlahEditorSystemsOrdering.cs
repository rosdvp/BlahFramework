using System;
using System.Collections.Generic;
using System.Text;
using Blah.Ordering;
using Blah.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal static class BlahEditorSystemsOrdering
{
	[MenuItem("Blah/Features/Report systems ordering issues")]
	public static void ReportIssues()
	{
		var sb = new StringBuilder();
		sb.AppendLine("--- systems ordering issues ---");

		var context = BlahReflection.InstantiateGameTypesWithBaseType<BlahContextBase>();

		var featuresBySystemsGroups =
			(Dictionary<int, List<BlahFeatureBase>>)BlahReflection.GetContextFeaturesGroups(context);
		foreach ((int groupId, var features) in featuresBySystemsGroups)
		{
			var systems = new List<Type>();
			foreach (var feature in features)
				if (feature.Systems != null)
					foreach (var system in feature.Systems)
						systems.Add(system);

			try
			{
				BlahOrderer.Order(ref systems);
			}
			catch (BlahOrdererSortingException e)
			{
				sb.AppendLine($"group {groupId}, {e.GetFullMsg()}");
			}
		}

		sb.AppendLine("-----------------------");
		Debug.Log(sb);
	}
}
}
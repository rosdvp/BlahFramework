using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal class EditorBlahFeatures
{
	[MenuItem("Blah/Report unused features")]
	public static void EditorReportUnUsedFeatures()
	{
		var blahAssembly = typeof(BlahContext).Assembly;
		
		Type contextType   = null;
		var  featuresInProject = new HashSet<Type>();

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			if (assembly != blahAssembly &&
		     !assembly.FullName.StartsWith("Unity") &&
		     !assembly.FullName.StartsWith("System") &&
		     !assembly.FullName.StartsWith("Blah"))
			{
				foreach (var type in assembly.GetTypes())
					if (type.BaseType == typeof(BlahContext))
						contextType = type;
					else if (type.BaseType == typeof(BlahFeatureBase))
						featuresInProject.Add(type);
			}
		}

		object context = Activator.CreateInstance(contextType);
		var prop = typeof(BlahContext).GetProperty(
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
}
}
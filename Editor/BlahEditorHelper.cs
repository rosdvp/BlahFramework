using System;
using System.Collections.Generic;
using Blah.Features;
using Blah.Reflection;

namespace Blah.Editor
{
internal static class BlahEditorHelper
{
	public static IEnumerable<Type> EnumerateGameTypes()
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
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

	public static IEnumerable<BlahFeatureBase> EnumerateGameFeatures()
	{
		foreach (var type in EnumerateGameTypes())
			if (type.BaseType == typeof(BlahFeatureBase))
				yield return (BlahFeatureBase)Activator.CreateInstance(type);
	}
}
}
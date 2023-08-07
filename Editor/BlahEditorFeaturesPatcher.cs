using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Blah.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blah.Editor
{
internal static class BlahEditorFeaturesPatcher
{
	[MenuItem("Blah/Experimental/Patch features")]
	public static void FillFeatures()
	{
		var typeNameToFilePath = new Dictionary<string, string>();
		FillTypeNameToFilePathMap(Application.dataPath, "Feature", typeNameToFilePath);

		var patchedFeaturesFilesPaths = new List<string>();

		var services  = new HashSet<Type>();
		var consumers = new HashSet<Type>();
		var producers = new HashSet<Type>();

		foreach (var feature in BlahEditorHelper.EnumerateGameFeatures())
		{
			string featureTypeName = feature.GetType().Name;
			
			BlahReflection.FindFeatureDependencies(feature, services, consumers, producers, true);
			
			consumers.ExceptWith(producers);
			
			bool isServicesPatchRequired  = feature.Services?.SetEquals(services) != true;
			bool isConsumersPatchRequired = feature.ConsumingFromOutside?.SetEquals(consumers) != true;
			bool isProducersPatchRequired = feature.Producing?.SetEquals(producers) != true;
			if (!isServicesPatchRequired &&
			    !isConsumersPatchRequired &&
			    !isProducersPatchRequired)
			{
				Debug.Log($"{featureTypeName} is already fine.");
				continue;
			}

			if (!typeNameToFilePath.TryGetValue(featureTypeName, out string filePath))
			{
				Debug.LogWarning($"{featureTypeName} is not patched since file path is not found.");
				continue;
			}

			if (EditorUtility.DisplayDialog(
				    "BlahEditorFeaturesPatcher",
				    $"Patch {featureTypeName} file?\nFile will require manual formatting.",
				    "Yes",
				    "No"
			    ))
			{
				PatchFeature(
					filePath,
					isServicesPatchRequired ? services : null,
					isConsumersPatchRequired ? consumers : null,
					isProducersPatchRequired ? producers : null
				);
				patchedFeaturesFilesPaths.Add(filePath);

				Debug.Log($"{featureTypeName} file patched.");
			}
		}

		foreach (string filePath in patchedFeaturesFilesPaths)
		{
			string[] parts        = filePath.Split("\\");
			int      idx          = Array.IndexOf(parts, "Assets");
			string   relativePath = string.Join("/", parts[idx..]);
			AssetDatabase.ImportAsset(relativePath);
		}
		
		Debug.Log("Patching is done.");
	}


	private static void PatchFeature(
		string        filePath,
		HashSet<Type> services,
		HashSet<Type> consumers,
		HashSet<Type> producers)
	{
		const string servicesPattern  = "(HashSet<Type>.*?Services)(.|\n)*?(public)";
		const string consumersPattern = "(HashSet<Type>.*?ConsumingFromOutside)(.|\n)*?(public)";
		const string producersPattern = "(HashSet<Type>.*?Producing)(.|\n)*?(public)";

		string file = File.ReadAllText(filePath);

		if (services != null)
			file = PatchStr(file, servicesPattern, services);
		if (consumers != null)
			file = PatchStr(file, consumersPattern, consumers);
		if (producers != null)
			file = PatchStr(file, producersPattern, producers);

		File.WriteAllText(filePath, file, Encoding.UTF8);
	}

	private static string PatchStr(string str, string pattern, HashSet<Type> types)
	{
		var sb = new StringBuilder();
		if (types.Count > 0)
		{
			sb.AppendLine("{ get; } = new()");
			sb.AppendLine("{");
			foreach (var type in types)
				sb.AppendLine($"typeof({type.Name}),");
			sb.AppendLine("};");
		}
		else
		{
			sb.AppendLine("{ get; }");
		}
		return Regex.Replace(str, pattern, $"$1{sb}$3", RegexOptions.Multiline);
	}


	private static void FillTypeNameToFilePathMap(
		string                     rootPath,
		string                     searchFileNameStart,
		Dictionary<string, string> typeNameToFilePath)
	{
		rootPath = rootPath.Replace('/', '\\');

		foreach (string filePath in Directory.GetFiles(rootPath))
			if (filePath.EndsWith(".cs"))
			{
				string fileName = filePath.Split("\\")[^1][..^3];
				if (fileName.StartsWith(searchFileNameStart))
					typeNameToFilePath.Add(fileName, filePath);
			}

		foreach (string subDirPath in Directory.GetDirectories(rootPath))
			FillTypeNameToFilePathMap(subDirPath, searchFileNameStart, typeNameToFilePath);
	}
}
}
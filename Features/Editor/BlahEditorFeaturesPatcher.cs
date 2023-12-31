﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Blah.Reflection;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal static class BlahEditorFeaturesPatcher
{
	[MenuItem("Blah/Features/Smart Patch")]
	public static void SmartPatchFeatures() => PatchFeatures(false);

	[MenuItem("Blah/Features/Force Patch")]
	public static void ForcePatchFeatures() => PatchFeatures(true);
	
	private static void PatchFeatures(bool isForced)
	{
		var typeNameToFilePath = new Dictionary<string, string>();
		FillTypeNameToFilePathMap(Application.dataPath, "Feature", typeNameToFilePath);

		var patchedFeaturesFilesPaths = new List<string>();

		var services  = new HashSet<Type>();
		var consumers = new HashSet<Type>();
		var producers = new HashSet<Type>();

		foreach (var feature in BlahReflection.InstantiateGameTypesWithBaseType<BlahFeatureBase>())
		{
			string featureTypeName = feature.GetType().Name;
			
			BlahReflection.FindFeatureDependencies(feature, services, consumers, producers, true);
			
			consumers.ExceptWith(producers);
			
			bool isServicesPatchRequired  = isForced || !IsSame(feature.Services, services);
			bool isConsumersPatchRequired = isForced || !IsSame(feature.ConsumingFromOutside, consumers);
			bool isProducersPatchRequired = isForced || !IsSame(feature.Producing, producers);
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
			file = PatchStr(file, servicesPattern, services, false);
		if (consumers != null)
			file = PatchStr(file, consumersPattern, consumers, true);
		if (producers != null)
			file = PatchStr(file, producersPattern, producers, true);

		File.WriteAllText(filePath, file, Encoding.UTF8);
	}

	private static string PatchStr(string str, string pattern, HashSet<Type> types, bool withBeautify)
	{
		var sb = new StringBuilder();
		if (types.Count > 0)
		{
			sb.AppendLine("{ get; } = new()");
			sb.AppendLine("\t{");

			if (withBeautify)
			{
				var list = new List<Type>(types);
				list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
				for (var i = 0; i < list.Count; i++)
				{
					if (i > 0 && list[i - 1].Name[..2] != list[i].Name[..2])
						sb.AppendLine();
					sb.AppendLine($"\t\ttypeof({list[i].Name}),");
				}
			}
			else
				foreach (var type in types)
					sb.AppendLine($"\ttypeof({type.Name}),");
			
			sb.AppendLine("\t};");
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

	private static bool IsSame(HashSet<Type> a, HashSet<Type> b) =>
		a == null && b == null ||
		a is { Count             : 0 } && b == null ||
		a == null && b is { Count: 0 } ||
		a != null && b != null && a.SetEquals(b);
}
}
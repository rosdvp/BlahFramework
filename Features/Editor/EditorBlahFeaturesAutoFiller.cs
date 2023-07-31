using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Blah.Pools;
using Blah.Services;
using UnityEditor;
using UnityEngine;

namespace Blah.Features.Editor
{
internal static class EditorBlahFeaturesAutoFiller
{
	[MenuItem("Blah/Experimental/Auto-fill features consumers-producers")]
	public static void EditorFillFeatures()
	{
		var typeNameToFilePath = new Dictionary<string, string>();
		FillTypeNameToFilePathMap(Application.dataPath, "Feature", typeNameToFilePath);

		var patchedFeaturesFilesPaths = new List<string>();
		
		foreach (var type in EditorBlahFeatures.EnumerateGameTypes())
		{
			if (type.BaseType != typeof(BlahFeatureBase))
				continue;

			var feature = (BlahFeatureBase)Activator.CreateInstance(type);
			var prop = typeof(BlahFeatureBase).GetProperty(
				"Systems",
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			var systems = (IReadOnlyList<Type>)prop?.GetValue(feature);
			if (systems == null)
				continue;

			var services  = new HashSet<Type>();
			var consumers = new HashSet<Type>();
			var producers = new HashSet<Type>();
			foreach (var system in systems)
				FillSystemDependencies(system, services, consumers, producers);

			if (feature.Services?.SetEquals(services) == true &&
			    feature.Consumers?.SetEquals(consumers) == true &&
			    feature.Producers?.SetEquals(producers) == true)
			{
				Debug.Log($"{type.Name} is already fine.");
				continue;
			}

			if (!typeNameToFilePath.TryGetValue(type.Name, out string filePath))
			{
				Debug.LogWarning($"{type.Name} is not patched since file path is not found.");
				continue;
			}

			if (EditorUtility.DisplayDialog(
				    "EditorBlahFeaturesAutoFiller",
				    $"Patch Feature {type.Name} file?\nFile will require manual formatting.",
				    "Yes",
				    "No"
			    ))
			{
				PatchFeature(filePath, services, consumers, producers);
				patchedFeaturesFilesPaths.Add(filePath);
				
				Debug.Log($"{type.Name} file patched.");
			}
		}

		foreach (string filePath in patchedFeaturesFilesPaths)
		{
			string[] parts        = filePath.Split("\\");
			int      idx          = Array.IndexOf(parts, "Assets");
			string   relativePath = string.Join("/", parts[idx..]);
			AssetDatabase.ImportAsset(relativePath);
		}
		
		Debug.Log("done");
	}


	private static void FillSystemDependencies(
		Type system, 
		HashSet<Type> services, 
		HashSet<Type> consumers,
		HashSet<Type> producers)
	{
		while (system?.Namespace?.StartsWith("System") == false)
		{
			var fields = system.GetFields(
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic
			);
			foreach (var field in fields)
			{
				if (field.FieldType.BaseType == typeof(BlahServiceBase))
					services.Add(field.FieldType);

				if (!field.FieldType.IsGenericType)
					continue;
				var genBaseType = field.FieldType.GetGenericTypeDefinition();
				var genArgType  = field.FieldType.GenericTypeArguments[0];

				if (genBaseType == typeof(IBlahSignalConsumer<>) ||
				    genBaseType == typeof(IBlahDataProducer<>))
				{
					consumers.Add(genArgType);
				}
				else if (genBaseType == typeof(IBlahSignalProducer<>) ||
				         genBaseType == typeof(IBlahDataProducer<>))
				{
					producers.Add(genArgType);
				}
			}
			system = system.BaseType;
		}
	}
	
	private static void PatchFeature(
		string filePath, 
		HashSet<Type> services,
		HashSet<Type> consumers,
		HashSet<Type> producers)
	{
		const string servicesHeader  = "HashSet<Type> Services";
		const string consumersHeader = "HashSet<Type> Consumers";
		const string producersHeader = "HashSet<Type> Producers";
		const string nextHeader      = "public";

		string file = File.ReadAllText(filePath);

		file = PatchStr(file, servicesHeader, nextHeader, services);
		file = PatchStr(file, consumersHeader, nextHeader, consumers);
		file = PatchStr(file, producersHeader, nextHeader, producers);
		
		File.WriteAllText(filePath, file);
	}

	private static string PatchStr(string str, string startHeader, string nextHeader, HashSet<Type> types)
	{
		int startIdx = str.IndexOf(startHeader, StringComparison.Ordinal) + startHeader.Length;
		int endIdx   = str.IndexOf(nextHeader, startIdx, StringComparison.Ordinal) - 1;

		if (startIdx < 0 || endIdx < 0)
			throw new Exception("incorrect feature format");

		var sb = new StringBuilder();
		sb.Append("{ get; } = new()");
		sb.AppendLine();
		sb.AppendLine("{");
		foreach (var type in types)
			sb.AppendLine($"typeof({type.Name}),");
		sb.AppendLine("};");
		
		return str[..startIdx] + sb + str[endIdx..];
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
using System;
using System.IO;
using System.Text;
using Blah.Ecs;
using Blah.Pools;
using Blah.Reflection;
using Blah.Services;
using UnityEditor;

namespace Blah.Aot
{
public static class BlahAot
{
	private const string PATH = "Assets/Blah/BlahAotGenerated.cs";

	[MenuItem("Blah/Aot/Generate")]
	public static void Generate()
	{
		var sb = new StringBuilder();

		foreach (var type in BlahReflection.EnumerateGameTypes())
		{
			var interfaces = type.GetInterfaces();
			if (Array.IndexOf(interfaces, typeof(IBlahEntrySignal)) != -1)
			{
				sb.AppendLine($"pools.GetSignalRead<{type.FullName}>();");
				sb.AppendLine($"pools.GetSignalWrite<{type.FullName}>();");
			}
			if (Array.IndexOf(interfaces, typeof(IBlahEntryData)) != -1)
			{
				sb.AppendLine($"pools.GetDataGetter<{type.FullName}>();");
				sb.AppendLine($"pools.GetDataAdder<{type.FullName}>();");
			}
			if (Array.IndexOf(interfaces, typeof(IBlahEntryNfSignal)) != -1)
			{
				sb.AppendLine($"pools.GetNfSignalRead<{type.FullName}>();");
				sb.AppendLine($"pools.GetNfSignalWrite<{type.FullName}>();");
			}
			if (Array.IndexOf(interfaces, typeof(IBlahEntryEcs)) != -1)
			{
				sb.AppendLine($"ecs.GetCompGetter<{type.FullName}>();");
				sb.AppendLine($"ecs.GetCompFull<{type.FullName}>();");
			}
			if (type.BaseType == typeof(BlahEcsFilter))
			{
				sb.AppendLine($"ecs.CreateFilter<{type.FullName}>");
			}
			if (type.BaseType == typeof(BlahServiceBase))
			{
				sb.AppendLine($"services.Get<{type.FullName}>();");
			}
		}
		string content = TEMPLATE.Replace("[CODEGEN]", sb.ToString());
		CreateDirIfNotExists();
		File.WriteAllText(PATH, content);
		AssetDatabase.ImportAsset(PATH);
	}


	private static void CreateDirIfNotExists()
	{
		const string pathDir = "Assets/Blah";
		if (!Directory.Exists(pathDir))
			Directory.CreateDirectory(pathDir);
	}
	

	private const string TEMPLATE = @"
using Blah.Pools;
using Blah.Services;
using UnityEngine.Scripting;

public static class BlahPoolsAotGenerated 
{
	[Preserve]
	private static void Preserve()
	{
		var pools = new BlahPoolsContext();
		var ecs = new BlahEcs();
		var services = new BlahServicesContext(null);
		[CODEGEN]
	}	
}";
}
}
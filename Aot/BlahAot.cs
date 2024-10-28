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

	[MenuItem("Blah/Framework/Generate AOT")]
	public static void Generate()
	{
		var sb = new StringBuilder();

		foreach (var type in BlahReflection.EnumerateGameTypes())
		{
			if (typeof(IBlahEntrySignal).IsAssignableFrom(type))
			{
				sb.AppendLine($"pools.GetSignalRead<{type.FullName}>();");
				sb.AppendLine($"pools.GetSignalWrite<{type.FullName}>();");
			}
			else if (typeof(IBlahEntryNfSignal).IsAssignableFrom(type))
			{
				sb.AppendLine($"pools.GetNfSignalRead<{type.FullName}>();");
				sb.AppendLine($"pools.GetNfSignalWrite<{type.FullName}>();");
			}
			else if (typeof(IBlahEntryData).IsAssignableFrom(type))
			{
				sb.AppendLine($"pools.GetDataGetter<{type.FullName}>();");
				sb.AppendLine($"pools.GetDataFull<{type.FullName}>();");
			}
			else if (typeof(BlahServiceBase).IsAssignableFrom(type))
			{
				sb.AppendLine($"services.Get<{type.FullName}>();");
			}
			else if (typeof(IBlahEntryComp).IsAssignableFrom(type))
			{
				sb.AppendLine($"ecs.GetCompGetter<{type.FullName}>");
				sb.AppendLine($"ecs.GetCompFull<{type.FullName}>");
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
using Blah.Ecs;
using UnityEngine.Scripting;

public static class BlahPoolsAotGenerated 
{
	[Preserve]
	private static void Preserve()
	{
		var pools = new BlahPoolsContext();
		var services = new BlahServicesContext(null);
		var ecs = new BlahEcs();
		[CODEGEN]
	}	
}";
}
}
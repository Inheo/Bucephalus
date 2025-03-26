#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bucephalus.Preprocessor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bucephalus.Editor
{
    [InitializeOnLoad]
    public class ViewIdCodeGenerator : IPreprocessBuildWithReport
    {
        static ViewIdCodeGenerator() => GenerateCodeAtStartup();

        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report) => Generate();
        private static void GenerateCodeAtStartup() => Generate();

        private static void Generate()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains("Unity"))
                .SelectMany(assembly => assembly.GetTypes()).ToArray();
            
            Debug.Log("Generating code...");
            var path = GetPath();
            var text = GenerateFromAssemblies(types);
            File.WriteAllText(path, text);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"... {path} generated.");
        }

        private static string GenerateFromAssemblies(IEnumerable<Type> types)
        {
            var viewIds = AsmFinder.FindViewIds(types);

            var stringBuilder = new StringBuilder(@"
using System.Collections.Generic;

namespace Bucephalus.Preprocessor
{
	internal static class Generated
	{
		public static IReadOnlyList<string> ViewIds = new[]
		{
");

            foreach (var viewId in viewIds)
            {
                stringBuilder.AppendLine($"\t\t\t\"{viewId}\",");
            }

            stringBuilder.Append(@"
		};
	}
}
");
            return stringBuilder.ToString();
        }
        
        
        private static string GetPath()
        {
            var guids = AssetDatabase.FindAssets($"{nameof(ViewIdCodeGenerator)} t:script");
            string scriptPath = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/Bucephalus/Editor/")) continue;
                scriptPath = Path.GetDirectoryName(path);
                break;
            }
            
            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("Failed to determine the path to GenerateClass script.");
                return null;
            }

            var bucephalusPath = Directory.GetParent(scriptPath)?.FullName;
            if (string.IsNullOrEmpty(bucephalusPath))
            {
                Debug.LogError("Failed to find the Bucephalus folder.");
                return null;
            }
            
            var preprocessorPath = Path.Combine(bucephalusPath, "Preprocessor");

            if (!Directory.Exists(preprocessorPath))
            {
                Directory.CreateDirectory(preprocessorPath);
            }

            return Path.Combine(preprocessorPath, "Generated.cs");
        }
    }
}
#endif
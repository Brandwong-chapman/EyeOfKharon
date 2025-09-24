#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Tzar
{
    public static class LayerEnumerator
    {
        private const string FILE_NAME = "Layers";
        private const string FILE_EXTENSION = ".cs";

        public static void GenerateLayersFile()
        {
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            string classText = GenerateClassText(layers);

            string pathToThis = GetPathToThis();
            if (string.IsNullOrEmpty(pathToThis))
            {
                Debug.LogError("LayerEnumerator: Failed to find script path.");
                return;
            }

            string filePath = Path.Combine(pathToThis, FILE_NAME + FILE_EXTENSION);

            if (File.Exists(filePath))
            {
                string existingText = File.ReadAllText(filePath);
                if (existingText == classText)
                {
                    // No changes, no need to write
                    return;
                }
            }

            File.WriteAllText(filePath, classText);
            AssetDatabase.Refresh();
            Debug.Log("LayerEnumerator: Layers file regenerated.");
        }

        private static string GenerateClassText(string[] layers)
        {
            string members = string.Join("\n", layers.Select((layerName, index) =>
                $"    public const int {SanitizeName(layerName)} = {index};"));

            string functions = @"
    public static int ToLayerMask(int layer)
    {
        return 1 << layer;
    }";

            return
$@"public static class {FILE_NAME}
{{
{members}
{functions}
}}";
        }

        private static string SanitizeName(string str)
        {
            string pattern = "[^a-zA-Z0-9]";
            string replacement = "SC";
            return Regex.Replace(str, pattern, replacement);
        }

        private static string GetPathToThis()
        {
            string thisFileName = "LayerEnumerator.cs";
            string[] results = Directory.GetFiles(Application.dataPath, thisFileName, SearchOption.AllDirectories);

            if (results.Length == 0)
                return null;

            return Path.GetDirectoryName(results[0])?.Replace("\\", "/");
        }
    }

    public class LayerEnumeratorAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith("TagManager.asset"))
                {
                    LayerEnumerator.GenerateLayersFile();
                    break;
                }
            }
        }
    }
}

#endif

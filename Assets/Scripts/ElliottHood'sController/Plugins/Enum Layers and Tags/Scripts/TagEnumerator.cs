#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Tzar
{
    public static class TagEnumerator
    {
        private const string FILE_NAME = "Tags";
        private const string FILE_EXTENSION = ".cs";

        public static void GenerateTagsFile()
        {
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            string classText = GenerateClassText(tags);

            string pathToThis = GetPathToThis();
            if (string.IsNullOrEmpty(pathToThis))
            {
                Debug.LogError("TagEnumerator: Failed to find script path.");
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
            Debug.Log("TagEnumerator: Tags file regenerated.");
        }

        private static string GenerateClassText(string[] tags)
        {
            string members = string.Join("\n", tags.Select(tag =>
                $"    public const string {SanitizeTagName(tag)} = @\"{tag}\";"));

            return $"public static class {FILE_NAME}\n{{\n{members}\n}}";
        }

        private static string SanitizeTagName(string tag)
        {
            string pattern = "[^a-zA-Z0-9]";
            string replacement = "SC";
            return Regex.Replace(tag, pattern, replacement);
        }

        private static string GetPathToThis()
        {
            string thisFileName = "TagEnumerator.cs";
            string[] results = Directory.GetFiles(Application.dataPath, thisFileName, SearchOption.AllDirectories);

            if (results.Length == 0)
                return null;

            return Path.GetDirectoryName(results[0])?.Replace("\\", "/");
        }
    }

    public class TagEnumeratorAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith("TagManager.asset"))
                {
                    // Automatically regenerate whenever Tags or Layers change
                    TagEnumerator.GenerateTagsFile();
                    break;
                }
            }
        }
    }
}

#endif

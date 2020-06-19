using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace PrefabAssetUtility.Editor
{
    public static class PrefabUtils
    {
        private const string PREFAB_TO_GUID_PATH = "PrefabCache/PrefabToGUID.json";
        private const string GUID_TO_PREFAB_PATH = "PrefabCache/GUIDToPrefab.json";

        private static string _basePath;

        private static Dictionary<string, List<string>> _prefabToGUID = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> _GUIDToPrefab = new Dictionary<string, List<string>>();

        private static readonly Regex _regex = new Regex(@"guid: (.*?),",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [InitializeOnLoadMethod]
        private static void Init()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            _basePath = Path.GetFullPath(
                new Uri(Path.Combine(Path.GetDirectoryName(path) ?? "", "../../")).AbsolutePath);

            LoadCache();

            PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
            EditorApplication.quitting += EditorApplicationOnQuitting;
            CompilationPipeline.compilationStarted += CompilationPipelineOnCompilationStarted;
        }

        private static void CompilationPipelineOnCompilationStarted(object obj)
        {
            SaveCache();

            PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
            EditorApplication.quitting -= EditorApplicationOnQuitting;
            CompilationPipeline.compilationStarted -= CompilationPipelineOnCompilationStarted;
        }

        private static void EditorApplicationOnQuitting()
        {
            SaveCache();
        }

        private static List<string> GetAllPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths().Where(s => s.Contains(".prefab")).ToList();
        }

        private static void PrefabInstanceUpdated(GameObject instance)
        {
            string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(instance));
            ProcessCacheForAsset(path);

            if (EditorPrefs.GetBool(PrefabAssetSettings.STORE_CACHE_ON_PREFAB_CHANGE))
            {
                SaveCache();
            }
        }

        internal static void RefreshPrefabCache()
        {
            List<string> allPrefabs = GetAllPrefabs();
            _prefabToGUID.Clear();
            _GUIDToPrefab.Clear();

            int current = 0;
            int total = allPrefabs.Count;

            foreach (string asset in allPrefabs)
            {
                current++;
                EditorUtility.DisplayProgressBar("Processing prefabs", $"Processed {current} out of {total} prefabs",
                    current / (float) total);

                ProcessCacheForAsset(asset);
            }

            EditorUtility.ClearProgressBar();

            SaveCache();
        }

        private static void ProcessCacheForAsset(string asset)
        {
            string path = $"{_basePath}{asset}";
            List<string> GUIDs = new List<string>();

            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("guid"))
                    {
                        GUIDs.Add(_regex.Split(line)[1]);
                    }
                }

                GUIDs = GUIDs.Distinct().ToList();

                if (_prefabToGUID.ContainsKey(asset))
                    _prefabToGUID[asset] = GUIDs;
                else
                    _prefabToGUID.Add(asset, GUIDs);

                foreach (string guid in GUIDs)
                {
                    if (_GUIDToPrefab.ContainsKey(guid))
                    {
                        _GUIDToPrefab[guid].Add(asset);
                    }
                    else
                    {
                        _GUIDToPrefab.Add(guid, new List<string> {asset});
                    }
                }
            }
        }

        private static void LoadCache()
        {
            if (File.Exists(_basePath + PREFAB_TO_GUID_PATH) && File.Exists(_basePath + GUID_TO_PREFAB_PATH))
            {
                using (StreamReader reader = new StreamReader(_basePath + PREFAB_TO_GUID_PATH))
                {
                    _prefabToGUID = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(reader.ReadToEnd());
                }

                using (StreamReader reader = new StreamReader(_basePath + GUID_TO_PREFAB_PATH))
                {
                    _GUIDToPrefab = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(reader.ReadToEnd());
                }
            }
            else
            {
                RefreshPrefabCache();
            }
        }
        
        private static void SaveCache()
        {
            if (!Directory.Exists(_basePath + "PrefabCache"))
            {
                Directory.CreateDirectory(_basePath + "PrefabCache");
            }

            File.Delete(_basePath + PREFAB_TO_GUID_PATH);
            using (StreamWriter writer = new StreamWriter(_basePath + PREFAB_TO_GUID_PATH))
            {
                writer.Write(JsonConvert.SerializeObject(_prefabToGUID));
            }

            File.Delete(_basePath + GUID_TO_PREFAB_PATH);
            using (StreamWriter writer = new StreamWriter(_basePath + GUID_TO_PREFAB_PATH))
            {
                writer.Write(JsonConvert.SerializeObject(_GUIDToPrefab));
            }
        }

        /// <summary>
        /// Get the list of prefab paths that reference the given GUID
        /// </summary>
        /// <param name="GUID">GUID to check for</param>
        /// <returns>List of prefabs using the GUID</returns>
        public static List<string> GetPrefabsForGUID(string GUID)
        {
            return _GUIDToPrefab[GUID];
        }

        /// <summary>
        /// Get the list of GUIDs that the prefab uses
        /// </summary>
        /// <param name="prefabPath">The relative path to the prefab, <see cref="AssetDatabase.GetAssetPath"/></param>
        /// <returns>List of GUIDs that this prefab uses</returns>
        public static List<string> GetGUIDsForPrefab(string prefabPath)
        {
            return _prefabToGUID[prefabPath];
        }
    }
}

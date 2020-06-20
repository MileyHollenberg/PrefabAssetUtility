using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace PrefabAssetUtility.Editor
{
    internal static class PrefabAssetSettings
    {
        public const string STORE_CACHE_ON_PREFAB_CHANGE = "StoreCacheOnPrefabChange";

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/Prefab Asset Cache", SettingsScope.User)
            {
                label = "Prefab Asset Cache",
                guiHandler = searchContext =>
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Save cache on prefab change:");
                    EditorPrefs.SetBool(STORE_CACHE_ON_PREFAB_CHANGE,
                        EditorGUILayout.Toggle(EditorPrefs.GetBool(STORE_CACHE_ON_PREFAB_CHANGE)));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("Cache will always be stored when recompiling or exiting Unity");
                    if (GUILayout.Button("Force refresh cache"))
                    {
                        PrefabUtils.RefreshPrefabCache();
                    }

                    if (GUILayout.Button("Test"))
                    {
                        Debug.Log(JsonConvert.SerializeObject(PrefabUtils.GetComponentsForPrefab("Assets/Prefabs/Preb1.prefab")));
                        Debug.Log(JsonConvert.SerializeObject(PrefabUtils.GetPrefabsWithComponent<MonoBehaviour>()));
                    }
                },
                keywords = new HashSet<string>(new[] {"camera", "aim", "start"})
            };

            return provider;
        }
    }
}

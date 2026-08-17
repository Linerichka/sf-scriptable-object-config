using System;
using System.Collections.Generic;
using SFramework.Configs.Runtime;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class SFConfigAddressablesUtility
{
    private const string ConfigLabel = "config";

    public static void ProcessAllConfigs()
    {
        var paths = new List<string>();

        foreach (Type type in TypeCache.GetTypesDerivedFrom<SFConfig>())
        {
            if (type.IsAbstract) continue;

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                paths.Add(path);
            }
        }
        
        ProcessAssets(paths.ToArray());
    }

    public static void ProcessAssets(string[] assetPaths)
    {
        if (assetPaths == null || assetPaths.Length == 0) return;

        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null) return;

        foreach (var assetPath in assetPaths)
        {
            if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;

            ProcessAsset(assetPath, settings);
        }

        AssetDatabase.SaveAssets();
    }

    public static void ProcessAsset(string assetPath, AddressableAssetSettings settings)
    {
        var config = AssetDatabase.LoadAssetAtPath<SFConfig>(assetPath);

        if (config == null) return;

        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        if (string.IsNullOrEmpty(guid)) return;
        
        var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup,false,false);

        if (entry == null) return;
        
        entry.SetLabel(ConfigLabel,true,true,false);
    }
}
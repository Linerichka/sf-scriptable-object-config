using System;
using System.Collections.Generic;
using System.Linq;
using SFramework.Configs.Runtime;
using UnityEditor;
using UnityEngine;

namespace SFramework.Configs.Editor
{
    public static class SFConfigsEditorUtility
    {
        private static readonly Dictionary<Type, Dictionary<int, List<string>>> _nodePathsByType = new();


        [MenuItem("Tools/SFramework/Refresh Configs")]
        public static void RefreshConfigs()
        {
            _nodePathsByType.Clear();
        }
        
        public static string[] GetNodePaths(Type targetConfigType, int indentLevel)
        {
            if (_nodePathsByType.TryGetValue(targetConfigType, out var nodePathByIndent))
            {
                int maxLevel = nodePathByIndent.Keys.Max();

                if (indentLevel > maxLevel || maxLevel == 0)
                {
                    return null;
                }
                if (indentLevel == -1)
                {
                    return nodePathByIndent.TryGetValue(maxLevel, out var result) ? result.ToArray() : null;
                }
                else
                {
                    return nodePathByIndent.TryGetValue(indentLevel, out var result) ? result.ToArray() : null;
                }
            }
            else
            {
                nodePathByIndent = new Dictionary<int, List<string>>();
            }
            
            if (!typeof(ISFNodesConfig).IsAssignableFrom(targetConfigType))
            {
                Debug.LogError($"Type {targetConfigType.Name} is not a ScriptableObject implementing ISFConfig.");
                return null;
            }

            var guids = AssetDatabase.FindAssets($"t:{targetConfigType.Name}");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning($"No assets found for type {targetConfigType.Name}");
                return null;
            }
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (asset is ISFNodesConfig nodesConfig)
                {
                    nodesConfig.BuildTree();
                    foreach (var configNode in nodesConfig.Children)
                    {
                        GetChildPath(nodePathByIndent, configNode, 1);
                    }
                }
            }

            _nodePathsByType.Add(targetConfigType, nodePathByIndent);
            return GetNodePaths(targetConfigType, indentLevel);
        }

        private static void GetChildPath(Dictionary<int, List<string>> nodePathByIndent, ISFConfigNode node, int indentLevel)
        {
            if (!nodePathByIndent.TryGetValue(indentLevel, out var paths))
            {
                paths = new List<string>();
                nodePathByIndent.Add(indentLevel, paths);
            }
            
            paths.Add(node.FullId);
            
            if (node.Children == null || node.Children.Length == 0) return;
            indentLevel++;
            foreach (var childNode in node.Children)
            {
                GetChildPath(nodePathByIndent, childNode, indentLevel);
            }
        }
    }
}
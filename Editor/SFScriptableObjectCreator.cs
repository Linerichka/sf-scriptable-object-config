using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using SFramework.Configs.Runtime;

namespace SFramework.Configs.Editor
{

    public static class ScriptableObjectCreator
    {
        [MenuItem("Assets/Create/SFramework/Create ScriptableObject configs")]
        private static void ShowMenu(MenuCommand menuCommand)
        {
            GenericMenu menu = new GenericMenu();

            var types = TypeCache.GetTypesDerivedFrom<SFConfig>()
                .Where(t => !t.IsAbstract);


            foreach (var type in types)
            {
                string path =
                    $"SFramework/Configs/{type.FullName}";

                menu.AddItem(
                    new GUIContent(path),
                    false,
                    () => Create(type)
                );
            }

            if (menu.GetItemCount() == 0)
            {
                Debug.LogWarning("No SFConfig types found!");
                return;
            }

            var window = EditorGUIUtility.GetMainWindowPosition();
            var pos = window.center;
            pos.y = 0;
            Rect rect = new Rect(pos, Vector2.zero);
            menu.DropDown(rect);
        }


        private static void Create(Type type)
        {
            var asset = ScriptableObject.CreateInstance(type);

            string folder = GetSelectedFolderPath();
        
            string fullPath = Path.Combine(folder, $"{type.Name}.asset");
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
        }
        
        private static string GetSelectedFolderPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
            {
                return "Assets";
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }
            else
            {
                string directory = Path.GetDirectoryName(path);
                return string.IsNullOrEmpty(directory) ? "Assets" : directory;
            }
        }
    }
}
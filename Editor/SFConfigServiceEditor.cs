using System;
using System.Collections.Generic;
using System.Linq;
using SFramework.Configs.Runtime;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SFramework.Configs.Editor
{
    [InitializeOnLoad]
    public class SFConfigServiceEditor
    {
        private static Dictionary<Type, LinkedList<SFConfig>> _configsByType;
        
        private static AsyncOperationHandle<IList<SFConfig>> _configsHandle;
        
        private static SFConfigServiceEditor _instance;

        public static SFConfigServiceEditor Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SFConfigServiceEditor();

                return _instance;
            }
        }

        static SFConfigServiceEditor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
            EditorApplication.quitting += Dispose;

            Init();
        }

        private static void Init()
        {
            _configsHandle = Addressables.LoadAssetsAsync<SFConfig>("config", null);

            var configs =  _configsHandle.WaitForCompletion();
            
            _configsByType = new(configs.Count+1);
            
            foreach (var config in configs)
            {
                var type = config.GetType();
                
                if (_configsByType.TryGetValue(type, out var configList))
                {
                    configList.AddLast(config);
                }
                else
                {
                    var list = new LinkedList<SFConfig>();
                    list.AddLast(config);
                    _configsByType.Add(type, list);
                }
                
                if (config is ISFNodesConfig nodesConfig)
                {
                    nodesConfig.BuildTree();
                }
            }
        }
        
        public T[] GetConfigs<T>() where T : SFConfig, new()
        {
            if (_configsByType.TryGetValue(typeof(T), out var configList))
            {
                return configList.Cast<T>().ToArray();
            }
            else
            {
                return Array.Empty<T>();
            }
        }

        public T GetConfig<T>() where T : SFConfig, new()
        {
            if (_configsByType.TryGetValue(typeof(T), out var configList))
            {
                return (T)configList.First.Value;
            }
            else
            {
                return null;
            }
        }

        private static void Dispose()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
            EditorApplication.quitting -= Dispose;
            
            _configsHandle.Release();
            _configsByType.Clear();
            _configsByType = null;
            _instance = null;
        }
    }
}
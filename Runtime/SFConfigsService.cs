using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SFramework.Configs.Runtime
{
    public class SFConfigsService : ISFConfigsService
    {
        private readonly Dictionary<Type, LinkedList<SFConfig>> _configsByType = new();
        //for backward compatibility
        private readonly List<ISFConfig> _configs = new ();

        public UniTask Init(CancellationToken cancellationToken)
        {
            var configs = Resources.LoadAll<SFConfig>(string.Empty);
            
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
                
                if (config is ISFConfig isfConfig)
                {
                    _configs.Add(isfConfig);
                }
                
                if (config is ISFNodesConfig nodesConfig)
                {
                    nodesConfig.BuildTree();
                }
            }

            return UniTask.CompletedTask;
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

        
        #region Compat
        public IEnumerable<ISFConfig> Configs => _configs;
        
        public bool TryGetConfigs<T>(out T[] configs) where T : class, ISFConfig, new()
        {
            if (_configsByType.TryGetValue(typeof(T), out var configList))
            {
                configs = configList.Cast<T>().ToArray();
                return true;
            }
            else
            {
                configs = Array.Empty<T>();
                return false;
            }
        }
        public bool TryGetGlobalConfig<T>(out T config) where T : class, ISFGlobalConfig, new()
        {
            if (_configsByType.TryGetValue(typeof(T), out var configList))
            {
                config = configList.First.Value as T;
                return true;
            }

            config = Activator.CreateInstance<T>();
            return false;
        }
        #endregion
        
        public void Dispose()
        {
            _configsByType.Clear();
        }
    }
}

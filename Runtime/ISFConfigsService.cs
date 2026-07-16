using System;
using System.Collections.Generic;
using SFramework.Core.Runtime;
using UnityEngine.Scripting;



namespace SFramework.Configs.Runtime
{
    [Preserve]
    public interface ISFConfigsService : ISFService
    {
        public T[] GetConfigs<T>() where T : SFConfig, new();
        public T GetConfig<T>() where T : SFConfig, new();
        
        
        #region Compat
        [Obsolete]
        public IEnumerable<ISFConfig> Configs { get; }
        
        [Obsolete]
        public bool TryGetConfigs<T>(out T[] configs) where T : class, ISFConfig , new();
        
        [Obsolete]
        public bool TryGetGlobalConfig<T>(out T config) where T : class, ISFGlobalConfig , new();
        #endregion
    }
}
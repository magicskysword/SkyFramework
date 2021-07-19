using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace SkyFramework
{
    public class LuaSystem : SingletonMono<LuaSystem>,IInitialize
    {
        public static List<LuaEnv.CustomLoader> customLoaders = new List<LuaEnv.CustomLoader>();
        public bool AutoUpdateTick { get; set; }
        public LuaEnv GlobalEnv { get; private set; }
        public LuaTable GlobalTable { get; private set; }
         
        private bool isInit;
        private LuaTable envMetaTable;
        
        public IEnumerator Initialize()
        {
            if (isInit)
            {
                Dispose();
            }
            LuaEnvInit();
            isInit = true;
            Log($"初始化成功.");
            yield break;
        }
        
        public LuaTable GetNewEnvTable()
        {
            var table = GlobalEnv.NewTable();
            table.SetMetaTable(envMetaTable);
            return table;
        }
        
        /// <summary>
        /// 按路径加载lua脚本
        /// </summary>
        /// <param name="luaPath"></param>
        /// <param name="env"></param>
        public void Load(string luaPath,LuaTable env = null)
        {
            var chunk = CustomLoader(ref luaPath);
            if(chunk!=null)
            {
                DoString(chunk, luaPath, env);
            }
            else
            {
                throw new FileNotFoundException($"{luaPath} 加载失败。");
            }
        }

        /// <summary>
        /// 执行lua的chunk
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="chunkName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public object[] DoString(string chunk, string chunkName = null,LuaTable env = null)
        {
            return GlobalEnv.DoString(chunk, chunkName, env);
        }
        
        /// <summary>
        /// 执行lua的chunk
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="chunkName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public object[] DoString(byte[] chunk, string chunkName = null,LuaTable env = null)
        {
            return GlobalEnv.DoString(chunk, chunkName, env);
        }

        private void LuaEnvInit()
        {
            GlobalEnv = new LuaEnv();
            GlobalEnv.AddLoader(CustomLoader);
            foreach (var customLoader in customLoaders)
            {
                GlobalEnv.AddLoader(customLoader);
            }
            
            GlobalTable = GlobalEnv.Global;
            envMetaTable = GlobalEnv.NewTable();
            envMetaTable.Set("__index",GlobalTable);
        }

        private byte[] CustomLoader(ref string filepath)
        {
            var curFilePath = filepath.Replace(".", "/");
            foreach (var luaFolderPath in ConfigSystem.GetOrCreateConfig<LuaConfig>().luaPath)
            {
                var getPath = $"{luaFolderPath}/{curFilePath}.lua.txt";
                if (ResourcesSystem.Instance.HaveAsset(getPath))
                {
                    var bytes = ResourcesSystem.Instance.LoadAsset<TextAsset>(getPath).bytes;
                    return bytes;
                }
            }
            
            return null;
        }
        
        public void Tick()
        {
            GlobalEnv.Tick();
        }

        private void Dispose()
        {
            if (GlobalEnv != null)
            {
                GlobalEnv.Dispose();
                GlobalEnv = null;
            }
        }

        private void Start()
        {
            AutoUpdateTick = true;
        }

        private void Update()
        {
            if(!isInit)
                return;
            Tick();
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        private void Log(string str)
        {
            Debug.Log($"<color=cyan>[{nameof(LuaSystem)}]</color>{str}");
        }

        private void LogWarning(string str)
        {
            Debug.LogWarning($"<color=cyan>[{nameof(LuaSystem)}]</color>{str}");
        }
        
        private void LogError(string str)
        {
            Debug.LogError($"<color=cyan>[{nameof(LuaSystem)}]</color>{str}");
        }
    }
}
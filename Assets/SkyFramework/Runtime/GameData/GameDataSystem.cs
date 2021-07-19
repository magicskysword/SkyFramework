using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SkyFramework
{
    public delegate void CustomGameDataLoader(string startPath,Type type);
    
    public class GameDataSystem : SingletonMono<GameDataSystem>,IInitialize
    {
        
        public static List<KeyValuePair<Type,string>> dataRegister = new List<KeyValuePair<Type, string>>();

        public static Dictionary<Type, CustomGameDataLoader> customGameDataLoaders =
            new Dictionary<Type, CustomGameDataLoader>();
        
        
        public Dictionary<Type, Dictionary<string, IGameData>> dataDic;

        public int TotalDataCount { get; private set; } = 0;
        public int LoadedDataCount { get; private set; } = 0;

        public static void RegisterData(Type type, string path)
        {
            dataRegister.Add(new KeyValuePair<Type, string>(type,path));
        }
        
        public static void RegisterLoader(Type type, CustomGameDataLoader loader)
        {
            customGameDataLoaders[type] = loader;
        }
        
        public IEnumerator Initialize()
        {
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            dataDic = new Dictionary<Type, Dictionary<string, IGameData>>();
            yield return StartCoroutine(CacheAllData());

            stopwatch.Stop();
            Log($"初始化成功，耗时 {stopwatch.ElapsedMilliseconds} ms.");
        }

        public virtual IEnumerator CacheAllData()
        {
            foreach (var keyValuePair in dataRegister)
            {
                if (!customGameDataLoaders.TryGetValue(keyValuePair.Key, out var customGameDataLoader))
                {
                    customGameDataLoader = CacheData;
                }
                foreach (var path in ResourcesSystem.Instance.GetAssetsPath(keyValuePair.Value))
                {
                    customGameDataLoader.Invoke(path, keyValuePair.Key);
                }
            }
            yield return StartCoroutine(WaitForLoadComplete());
        }

        protected void CacheData(string startPath,Type type)
        {
            if (!dataDic.TryGetValue(type, out var objDic))
            {
                objDic = new Dictionary<string, IGameData>();
                dataDic.Add(type, objDic);
            }
            
            string[] dataPaths = ResourcesSystem.Instance.GetAssetsPath(startPath);
            foreach (var path in dataPaths)
            {
                TotalDataCount++;
                ResourcesSystem.Instance.LoadAssetAsync<GameDataBase>(path, data =>
                {
                    if (data == null)
                    {
                        LogWarning($"数据 {path} 加载错误，类型可能不为 {type}");
                    }
                    else
                    {
                        objDic[data.DataID] = data;
                    }
                    LoadedDataCount++;
                });
                
            }
            
            Log($"{startPath} 开始缓存");
        }

        protected IEnumerator WaitForLoadComplete()
        {
            while (LoadedDataCount < TotalDataCount)
            {
                yield return null;
            }
        }

        public TData LoadData<TData>(string dataID,bool logErr = true) where TData : class, IGameData
        {
            if (dataDic.TryGetValue(typeof(TData), out var objDic))
            {
                if (objDic.TryGetValue(dataID, out var data))
                {
                    return data as TData;
                }
                else
                {
                    if(logErr)
                        LogError($"加载 {typeof(TData)} {dataID} 失败，键值不存在。");
                }
            }
            else
            {
                if(logErr)
                    LogError($"要加载的类型 {typeof(TData)} 不存在于缓存。");
            }

            return null;
        }
        
        public TData[] LoadAllData<TData>() where TData : class, IGameData
        {
            if (dataDic.TryGetValue(typeof(TData), out var objDic))
            {
                List<TData> list = new List<TData>();
                foreach (var value in objDic.Values)
                {
                    list.Add((TData) value);
                }

                return list.ToArray();
            }
            else
            {
                LogError($"要加载的类型 {typeof(TData)} 不存在于缓存。");
            }

            return null;
        }


        protected void Log(string str)
        {
            Debug.Log($"<color=orange>[GameDataManager]</color>{str}");
        }

        protected void LogWarning(string str)
        {
            Debug.LogWarning($"<color=orange>[GameDataManager]</color>{str}");
        }

        protected void LogError(string str)
        {
            Debug.LogError($"<color=orange>[GameDataManager]</color>{str}");
        }
    }
}

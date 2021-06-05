using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SkyFrameWork
{
    public class GameDataManagerBase<T> : SingletonMono<T> where T : MonoBehaviour
    {
        public Dictionary<Type, Dictionary<string, IGameData>> dataDic;

        public int TotalDataCount { get; private set; } = 0;
        public int LoadedDataCount { get; private set; } = 0;

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
            yield break;
        }

        protected void CacheDataByObject<TData>(string startPath) where TData : UnityEngine.Object, IGameData
        {
            if (!dataDic.TryGetValue(typeof(TData), out var objDic))
            {
                objDic = new Dictionary<string, IGameData>();
                dataDic.Add(typeof(TData), objDic);
            }
            
            string[] dataPaths = ResourceManager.Instance.GetAssetsPath(startPath);
            foreach (var path in dataPaths)
            {
                TotalDataCount++;
                ResourceManager.Instance.LoadAssetAsync<TData>(path, data =>
                {
                    if (data == null)
                    {
                        LogWarning($"数据 {path} 加载错误，类型可能不为 {typeof(TData)}");
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

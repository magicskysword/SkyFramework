using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SkyFrameWork
{
    public class ResourceManager : SingletonMono<ResourceManager>
    {
        private List<string> assetsPathCache = new List<string>();
        private Dictionary<string, IAsset> assetsCache = new Dictionary<string, IAsset>();
        private bool isInit = false;

        public IEnumerator Initialize(bool reinit = false)
        {
            if (isInit && !reinit)
            {
                LogWarning($"ResourceManager已经初始化，无法重复初始化。");
            }
            else
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
#if UNITY_EDITOR
                Log($"正在初始化，当前加载模式为:<color=cyan>编辑器加载</color>");
#else
            Log($"正在初始化，当前加载模式为:<color=cyan>游戏包加载</color>");
#endif

#if !UNITY_EDITOR
            PlayerPrefs.DeleteKey(Addressables.kAddressablesRuntimeDataPath);
#endif

                bool isInitDone = false;
                Addressables.InitializeAsync().Completed += (_) => isInitDone = true;
                while (!isInitDone)
                {
                    yield return null;
                }

                var items = Addressables.ResourceLocators;
                foreach (var item in items)
                {
                    foreach (var key in item.Keys)
                    {
                        if (key is string path && path.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                        {
                            assetsPathCache.Add(path);
                        }
                    }
                }

                stopwatch.Stop();
                Log($"初始化成功，耗时 {stopwatch.ElapsedMilliseconds} ms.");
                
                isInit = true;
            }
        }

        private void CheckInit()
        {
            if (!isInit)
            {
                Debug.LogWarning($"ResourceManager尚未初始化！");
                throw new Exception("ResourceManager isn't Init.");
            }
        }

        

        public string[] GetAssetsPath(string startPath)
        {
            CheckInit();
            List<string> list = new List<string>();
            foreach (var assetPath in assetsPathCache)
            {
                if (assetPath.StartsWith(startPath,StringComparison.OrdinalIgnoreCase))
                    list.Add(assetPath);
            }

            return list.ToArray();
        }

        private void AddAssetToCache(string path, IAsset asset)
        {
            assetsCache[path.ToLower()] = asset;
        }

        private bool GetAssetInCache(string path, out IAsset asset)
        {
            if (assetsCache.TryGetValue(path.ToLower(), out IAsset cache))
            {
                asset = cache;
                return asset != null;
            }
            else
            {
                asset = null;
                return false;
            }
        }
        
        private void RemoveAssetInCache(string path)
        {
            if (assetsCache.TryGetValue(path.ToLower(), out IAsset cache))
            {
                cache.Release();
                assetsCache.Remove(path.ToLower());
            }
        }

        private bool GetAllAssetInCache(string pathStartWith, out IAsset[] assetArray)
        {
            List<IAsset> assets = new List<IAsset>();
            foreach (var item in assetsCache)
            {
                if (item.Key.StartsWith(pathStartWith.ToLower()))
                {
                    assets.Add(item.Value);
                }
            }

            assetArray = assets.ToArray();
            return assetArray.Length > 0;
        }

        public bool TryLoadAsset<T>(string path,out T outAsset) where T : Object
        {
            if (GetAssetInCache(path, out IAsset asset))
            {
                AssetInfo<T> assetInfo = asset as AssetInfo<T>;
                if (assetInfo.IsComplete)
                {
                    outAsset = assetInfo.GetAsset();
                    return true;
                }
            }

            outAsset = null;
            return false;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadAsset<T>(string path) where T : Object
        {
            CheckInit();
            IAsset asset;
            AssetInfo<T> assetInfo;
            if (GetAssetInCache(path, out asset))
            {
                assetInfo = asset as AssetInfo<T>;
                return assetInfo.GetAsset();
            }

            var handle = Addressables.LoadAssetAsync<T>(path);
            assetInfo = new AssetInfo<T>(handle);
            AddAssetToCache(path, assetInfo);
            return assetInfo.GetAsset();
        }

        public T[] LoadAllAssets<T>(string startPath) where T : Object
        {
            CheckInit();
            string[] assetPath = GetAssetsPath(startPath);
            List<T> assetList = new List<T>();
            foreach (var path in assetPath)
            {
                assetList.Add(LoadAsset<T>(path));
            }

            return assetList.ToArray();
        }

        public bool AssetExist(string path)
        {
            GetAllAssetInCache(path,out _);
            return path.Length > 0;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetInfo<T> LoadAssetAsync<T>(string path, Action<T> callback = null) where T : Object
        {
            CheckInit();
            IAsset asset;
            AssetInfo<T> assetInfo;
            if (GetAssetInCache(path, out asset))
            {
                assetInfo = asset as AssetInfo<T>;
                if (assetInfo != null)
                {
                    if (assetInfo.IsComplete)
                    {
                        // 模拟异步
                        if (callback != null)
                            DelayToInvoke.DelayToInvokeByFrame(() => { callback.Invoke(assetInfo.GetAsset()); }, 0);
                    }
                    else
                    {
                        if (callback != null)
                            assetInfo.AssetLoadedCallback += loadedAsset =>
                            {
                                callback.Invoke(loadedAsset);
                            };
                    }

                    return assetInfo;
                }
            }
            var handle = Addressables.LoadAssetAsync<T>(path);
            assetInfo = new AssetInfo<T>(handle);
            handle.Completed += operationHandle => { callback?.Invoke(operationHandle.Result); };
            AddAssetToCache(path, assetInfo);
            return assetInfo;
        }

        public void ReleaseAsset(string path)
        {
            RemoveAssetInCache(path);
        }

        public SceneInstance LoadScene(string path)
        {
            var sceneHandle = Addressables.LoadSceneAsync(path, LoadSceneMode.Single);
            return sceneHandle.WaitForCompletion();
        }
        
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(string path,LoadSceneMode loadSceneMode)
        {
            var sceneHandle = Addressables.LoadSceneAsync(path, loadSceneMode);
            return sceneHandle;
        }

        public void Log(string str)
        {
            Debug.Log($"<color=yellow>[ResourceManager]</color>{str}");
        }

        public void LogWarning(string str)
        {
            Debug.LogWarning($"<color=yellow>[ResourceManager]</color>{str}");
        }
        
        public void LogError(string str)
        {
            Debug.LogError($"<color=yellow>[ResourceManager]</color>{str}");
        }
    }
}
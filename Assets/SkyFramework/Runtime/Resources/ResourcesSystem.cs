using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SkyFramework
{
    public partial class ResourcesSystem : SingletonMono<ResourcesSystem>,IInitialize
    {
        private List<string> assetsPathCache = new List<string>();
        private Dictionary<string, IAsset> assetsCache = new Dictionary<string, IAsset>();
        private bool isInit = false;

        public IEnumerator Initialize()
        {
            isInit = false;
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
            Log($"初始化成功，耗时 {stopwatch.ElapsedMilliseconds} ms 共加载资源数：{assetsPathCache.Count}.");
                
            isInit = true;
        }

        private void CheckInit()
        {
            if (!isInit)
            {
                throw new Exception($"{nameof(ResourcesSystem)}尚未初始化！");
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

        public bool HaveAsset(string path)
        {
            return assetsPathCache.Contains(path);
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
            if (GetAssetInCache(path, out asset))
            {
                return asset.GetObjectAsset() as T;
            }

            if (!HaveAsset(path))
            {
                LogWarning($"不存在资源 {path}");
                return null;
            }
            var handle = Addressables.LoadAssetAsync<T>(path);
            asset = AssetInfo<T>.CreateAssetInfo(handle);
            AddAssetToCache(path, asset);
            return asset.GetObjectAsset() as T;
        }
        
        /// <summary>
        /// 同步加载资源，非泛型
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Object LoadAsset(string path,Type type)
        {
            // 通过反射委托提高效率
            return LoadAssetUtil.GetLoadAssetInvoke(type).Invoke(path);
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
        public IAsset LoadAssetAsync<T>(string path, Action<T> callback = null) where T : Object
        {
            CheckInit();
            if (!HaveAsset(path))
            {
                LogWarning($"不存在资源 {path}");
                return null;
            }
            if (GetAssetInCache(path, out var asset))
            {
                if (asset != null)
                {
                    AssetInfo<T> assetInfo = asset as AssetInfo<T>;
                    if (assetInfo.IsComplete)
                    {
                        // 模拟异步
                        if (callback != null)
                            DelayToInvoke.DelayToInvokeByFrame(
                                () => { callback.Invoke(assetInfo.GetAsset()); }, 0
                                );
                    }
                    else
                    {
                        if (callback != null)
                            assetInfo.AssetLoadedCallback += callback;
                    }

                    return asset;
                }
            }
            var handle = Addressables.LoadAssetAsync<T>(path);
            asset = AssetInfo<T>.CreateAssetInfo(handle);
            handle.Completed += operationHandle => { callback?.Invoke(operationHandle.Result as T); };
            AddAssetToCache(path, asset);
            return asset;
        }

        /// <summary>
        /// 异步加载非泛型方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsset LoadAssetAsync(string path, Type type, Action<Object> callback = null)
        {
            // 通过反射委托提高效率
            return LoadAssetAsyncUtil.GetLoadAssetInvoke(type).Invoke(path,callback);
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

        private void Log(string str)
        {
            Debug.Log($"<color=yellow>[{nameof(ResourcesSystem)}]</color>{str}");
        }

        private void LogWarning(string str)
        {
            Debug.LogWarning($"<color=yellow>[{nameof(ResourcesSystem)}]</color>{str}");
        }
        
        private void LogError(string str)
        {
            Debug.LogError($"<color=yellow>[{nameof(ResourcesSystem)}]</color>{str}");
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SkyFramework
{
    public class AssetInfo<T> : IAsset where T : Object
    {
        private AsyncOperationHandle<T> handle;
        private T asset = null;

        public event Action<T> AssetLoadedCallback;
        public bool IsComplete =>  handle.IsDone;

        private AssetInfo()
        {
            
        }

        public static AssetInfo<T> CreateAssetInfo(AsyncOperationHandle<T> handle)
        {
            AssetInfo<T> assetInfo = new AssetInfo<T>();

            handle.Completed += operationHandle => { assetInfo.OnAssetLoadedCallback(operationHandle.Result); };
            assetInfo.handle = handle;
            return assetInfo;
        }

        public void Release()
        {
            Addressables.Release(handle);
            while (AssetLoadedCallback!=null)
            {
                AssetLoadedCallback -= AssetLoadedCallback;
            }
        }

        public Object GetObjectAsset()
        {
            return GetAsset();
        }
        
        public T GetAsset()
        {
            if (!IsComplete)
            {
                asset = handle.WaitForCompletion();
            }
            else
            {
                asset = handle.Result;
            }

            return asset;
        }

        private void OnAssetLoadedCallback(T obj)
        {
            AssetLoadedCallback?.Invoke(obj);
        }
    }

}
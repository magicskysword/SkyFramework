using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SkyFrameWork
{
    public class AssetInfo<T> : IAsset where T : Object
    {
        private AsyncOperationHandle<T> handle;
        private T asset = null;

        public event Action<T> AssetLoadedCallback;
        public bool IsComplete =>  handle.IsDone;

        public AssetInfo(AsyncOperationHandle<T> handle)
        {
            this.handle = handle;
            this.handle.Completed += operationHandle => { OnAssetLoadedCallback(operationHandle.Result); };
        }

        public void Release()
        {
            Addressables.Release(handle);
            while (AssetLoadedCallback!=null)
            {
                AssetLoadedCallback -= AssetLoadedCallback;
            }
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
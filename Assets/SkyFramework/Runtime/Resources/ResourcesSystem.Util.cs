using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SkyFramework
{
    public partial class ResourcesSystem
    {
        private class LoadAssetUtil
        {
            private delegate TObject LoadAssetDelegate<TObject>(string path) where TObject : Object;

            private static Dictionary<Type, ILoadAssetInvoke> loadAssetInvokes =
                new Dictionary<Type, ILoadAssetInvoke>();

            private static T LoadAsset<T>(string path) where T : Object
            {
                return ResourcesSystem.Instance.LoadAsset<T>(path);
            }
            
            public static ILoadAssetInvoke GetLoadAssetInvoke(Type genericType)
            {
                if (loadAssetInvokes.ContainsKey(genericType))
                    return loadAssetInvokes[genericType];
                MethodInfo mi = typeof(LoadAssetUtil).GetMethod(nameof(LoadAsset),
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo gmi = mi.MakeGenericMethod(genericType);
                Delegate gmd = Delegate.CreateDelegate(typeof(LoadAssetDelegate<>).MakeGenericType(genericType), gmi);
                ILoadAssetInvoke invoke =
                    Activator.CreateInstance(typeof(LoadAssetDelegateClass<>).MakeGenericType(genericType), gmd) as
                        ILoadAssetInvoke;
                loadAssetInvokes.Add(genericType,invoke);
                return invoke;
            }
            
            public interface ILoadAssetInvoke
            {
                Object Invoke(string path);
            }

            private class LoadAssetDelegateClass<TObject> : ILoadAssetInvoke where TObject : Object
            {
                private LoadAssetDelegate<TObject> dM;

                public LoadAssetDelegateClass(LoadAssetDelegate<TObject> gmd)
                {
                    dM = gmd;
                }

                public Object Invoke(string path)
                {
                    return dM.Invoke(path);
                }
            }
        }
        
        private class LoadAssetAsyncUtil
        {
            private delegate IAsset LoadAssetAsyncDelegate<TObject>(string path,Action<Object> callback) where TObject : Object;

            private static Dictionary<Type, ILoadAssetAsyncInvoke> loadAssetAsyncInvokes =
                new Dictionary<Type, ILoadAssetAsyncInvoke>();

            private static IAsset LoadAssetAsync<T>(string path,Action<T> callback) where T : Object
            {
                return ResourcesSystem.Instance.LoadAssetAsync<T>(path,callback);
            }
            
            public static ILoadAssetAsyncInvoke GetLoadAssetInvoke(Type genericType)
            {
                if (loadAssetAsyncInvokes.ContainsKey(genericType))
                    return loadAssetAsyncInvokes[genericType];
                MethodInfo mi = typeof(LoadAssetAsyncUtil).GetMethod(nameof(LoadAssetAsync),
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo gmi = mi.MakeGenericMethod(genericType);
                Delegate gmd = Delegate.CreateDelegate(typeof(LoadAssetAsyncDelegate<>).MakeGenericType(genericType), gmi);
                ILoadAssetAsyncInvoke asyncInvoke =
                    Activator.CreateInstance(typeof(LoadAssetAsyncDelegateClass<>).MakeGenericType(genericType), gmd) as
                        ILoadAssetAsyncInvoke;
                loadAssetAsyncInvokes.Add(genericType,asyncInvoke);
                return asyncInvoke;
            }
            
            public interface ILoadAssetAsyncInvoke
            {
                IAsset Invoke(string path,Action<Object> callback);
            }

            private class LoadAssetAsyncDelegateClass<TObject> : ILoadAssetAsyncInvoke where TObject : Object
            {
                private LoadAssetAsyncDelegate<TObject> dM;

                public LoadAssetAsyncDelegateClass(LoadAssetAsyncDelegate<TObject> gmd)
                {
                    dM = gmd;
                }

                public IAsset Invoke(string path,Action<Object> callback)
                {
                    return dM.Invoke(path,callback);
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SkyFrameWork;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SkyFrameWork
{
    public class ObjectPool : SingletonMono<ObjectPool>
    {
        private Dictionary<string, SubObjectPool> pools = new Dictionary<string, SubObjectPool>();

        public bool CreatePool(string id, ReuseableObject prefab)
        {
            if (pools.ContainsKey(id))
            {
                LogError($"创建对象池失败，对象池 {id} 已存在");
                return false;
            }

            if (prefab == null)
            {
                LogError($"创建对象池失败，对象池 {id} 的 prefab 不能为空");
                return false;
            }
            
            SubObjectPool pool = SubObjectPool.CreatePool(id, prefab);
            pool.transform.parent = transform;
            pools.Add(id, pool);
            ObjectPool.Log($"对象池 {id} 创建成功");
            return true;
        }
        
        public bool CreatePool(string path)
        {
            if (pools.ContainsKey(path))
            {
                LogError($"创建对象池失败，对象池 {path} 已存在");
                return false;
            }

            GameObject go = ResourceManager.Instance.LoadAsset<GameObject>(path);
            if (go == null)
            {
                LogError($"创建对象池失败，资源 {path} 不存在");
            }

            ReuseableObject prefab = go.GetComponent<ReuseableObject>();

            if (prefab == null)
            {
                LogError($"创建对象池失败，对象池 {path} 的 prefab 不能为空");
                return false;
            }
            
            SubObjectPool pool = SubObjectPool.CreatePool(path, prefab);
            pool.transform.parent = transform;
            pools.Add(path, pool);
            ObjectPool.Log($"对象池 {path} 创建成功");
            return true;
        }
        
        public void CreatePoolAsync(string path,Action<SubObjectPool> callback = null)
        {
            if (pools.ContainsKey(path))
            {
                LogError($"创建对象池失败，对象池 {path} 已存在");
                return;
            }

            ResourceManager.Instance.LoadAssetAsync<GameObject>(path, go =>
            {
                if (pools.TryGetValue(path, out var tagPool))
                {
                    callback?.Invoke(tagPool);
                    return;
                }
                
                if (go == null)
                {
                    LogError($"创建对象池失败，资源 {path} 不存在");
                    return;
                }

                ReuseableObject prefab = go.GetComponent<ReuseableObject>();

                if (prefab == null)
                {
                    LogError($"创建对象池失败，对象池 {path} 的 prefab 不能为空");
                    return;
                }
            
                SubObjectPool pool = SubObjectPool.CreatePool(path, prefab);
                pool.transform.parent = transform;
                pools.Add(path, pool);
                Log($"对象池 {path} 创建成功");
                
                callback?.Invoke(pool);
            });
        }

        public bool TryGetPool(string id, out SubObjectPool pool)
        {
            return pools.TryGetValue(id, out pool);
        }


        public bool DestroyPool(string id)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                LogError($"销毁对象池失败，对象池 {id} 不存在");
                return false;
            }

            pool.UnspawnAll();
            Destroy(pool.gameObject);
            pools.Remove(id);
            return true;
        }

        public ReuseableObject Spawn(string id)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                LogError($"创建对象失败，对象池 {id} 不存在");
                return null;
            }

            return pool.Spawn();
        }
        
        /// <summary>
        /// 当对象池不存在时，使用prefab创建对象池
        /// </summary>
        /// <param name="id"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public ReuseableObject Spawn(string id,ReuseableObject prefab)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                if (!CreatePool(id, prefab) || !TryGetPool(id, out pool))
                    return null;
            }
            return pool.Spawn();
        }

        /// <summary>
        /// 当对象池不存在时，使用path读取prefab创建对象池
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReuseableObject SpawnByPath(string path)
        {
            if (!pools.TryGetValue(path, out var pool))
            {
                if (!CreatePool(path) || !TryGetPool(path, out pool))
                    return null;
            }
            return pool.Spawn();
        }
        
        /// <summary>
        /// 当对象池不存在时，使用path读取prefab创建对象池
        /// <br/>该方法为异步方法
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void SpawnByPathAsync(string path,Action<ReuseableObject> callback)
        {
            if (!pools.TryGetValue(path, out var pool))
            {
                CreatePoolAsync(path,pool => pool.SpawnAsync(callback));
            }
            else
            {
                pool.SpawnAsync(callback);
            }
        }
        
        public bool Unspawn(string id, ReuseableObject go)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                LogError($"回收对象失败，对象池 {id} 不存在");
                return false;
            }

            return pool.Unspawn(go);
        }

        public bool UnspawnAll(string id)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                LogWarning($"回收所有对象失败，对象池 {id} 不存在");
                return false;
            }

            pool.UnspawnAll();
            return true;
        }

        public void UnspawnAll()
        {
            foreach (var pool in pools.Values)
            {
                pool.UnspawnAll();
            }
        }

        public bool CleanPool(string id)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                LogError($"清理对象池失败，对象池 {id} 不存在");
                return false;
            }

            if (pool.autoKill)
                DestroyPool(id);
            return true;
        }
        
        public void CleanAllPool()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (var pool in pools.Values)
            {
                pool.CleanPool();
            }
            stopwatch.Stop();
            Log($"清理 {pools.Count} 个对象池完成，耗时 {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// 清除所有对象池，一般在切换场景时调用
        /// </summary>
        /// <param name="force">强制清除，会清除autoKill为false的对象池</param>
        public void AutoKillPool(bool force = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int killPool = 0;
            foreach (var pool in pools.Values.ToArray())
            {
                if (pool.autoKill || force)
                {
                    killPool += 1;
                    DestroyPool(pool.PoolID);
                }
                else
                {
                    pool.CleanPool();
                }
            }
            stopwatch.Stop();
            Log($"总计清理 {pools.Count} 个对象池，删除{killPool}个对象池，耗时 {stopwatch.ElapsedMilliseconds} ms");
        }

        public static void Log(string msg)
        {
            Debug.Log($"<color=cyan>[ObjectPool]</color>{msg}");
        }
        
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=cyan>[ObjectPool]</color>{msg}");
        }
        
        public static void LogError(string msg)
        {
            Debug.LogError($"<color=cyan>[ObjectPool]</color>{msg}");
        }
    }

}
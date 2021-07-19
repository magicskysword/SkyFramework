using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SkyFramework
{
    public class SubObjectPool : MonoBehaviour
    {
        private string poolID;

        /// <summary>
        /// 对象预设体
        /// </summary>
        private IReuseable objectPrefab;

        private int objMinNum = 4;

        public bool autoKill = true;

        public int ObjMinNum
        {
            get => objMinNum;
            set => objMinNum = value;
        }

        /// <summary>
        /// 对象池集合
        /// </summary>
        private List<IReuseable> objects = new List<IReuseable>();

        /// <summary>
        /// 对象池ID
        /// </summary>
        public string PoolID => poolID;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="prefab"></param>
        /// <param name="minNum">对象池清理时最小持有数量</param>
        /// <returns></returns>
        internal static SubObjectPool CreatePool(string id, IReuseable prefab,int minNum = 0)
        {
            GameObject go = new GameObject(id);
            SubObjectPool subObjectPool = go.AddComponent<SubObjectPool>();
            subObjectPool.poolID = id;
            subObjectPool.BindPrefab(prefab);
            subObjectPool.ObjMinNum = minNum;
            return subObjectPool;
        }

        private void BindPrefab(IReuseable prefab)
        {
            if (prefab == null || prefab.ThisGameObject == null)
            {
                ObjectPoolSystem.LogError($"对象池 {poolID} 的预设体不存在");
                throw new NullReferenceException();
            }
            objectPrefab = prefab;
        }
        
        public IReuseable Spawn()
        {
            

            IReuseable go = null;
            foreach (var o in objects)
            {
                if (o.InPool)
                {
                    go = o;
                    break;
                }
            }

            if (go == null)
            {
                go = Instantiate(objectPrefab.ThisGameObject).GetComponent<IReuseable>();
                go.ObjPoolID = poolID;
                objects.Add(go);
            }

            go.InPool = false;
            go.ThisGameObject.SetActive(true);
            go.OnSpawn();
            return go;
        }
        
        public void SpawnAsync(Action<IReuseable> callback)
        {
            StartCoroutine(SpawnAsyncInternal(callback));
        }

        private IEnumerator SpawnAsyncInternal(Action<IReuseable> callback)
        {
            IReuseable go = null;
            yield return null;
            foreach (var o in objects)
            {
                if (o.InPool)
                {
                    go = o;
                    break;
                }
            }
            
            if (go == null)
            {
                go = Instantiate(objectPrefab.ThisGameObject).GetComponent<IReuseable>();
                go.ObjPoolID = poolID;
                objects.Add(go);
            }

            go.InPool = false;
            go.ThisGameObject.SetActive(true);
            go.OnSpawn();
            callback?.Invoke(go);
        }

        public bool Unspawn(IReuseable go)
        {
            if (!Contain(go))
            {
                ObjectPoolSystem.LogError($"回收对象失败，{go.ThisGameObject.name} 不是对象池 {PoolID} 中的对象");
                return false;
            }

            go.OnUnspawn();
            go.ThisGameObject.transform.SetParent(transform,false);
            go.InPool = true;
            go.ThisGameObject.SetActive(false);
            return true;
        }

        public void UnspawnAll()
        {
            foreach (var go in objects)
            {
                if (go.InPool)
                    continue;
                go.OnUnspawn();
                go.InPool = true;
                go.ThisGameObject.transform.SetParent(transform,false);
                go.ThisGameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 填充对象池直到达到num
        /// </summary>
        /// <param name="num"></param>
        public void Fill(int num)
        {
            int totalCount = objects.Count;
            for (int i = totalCount; i < num; i++)
            {
                IReuseable go = Instantiate(objectPrefab.ThisGameObject).GetComponent<IReuseable>();
                objects.Add(go);
                go.InPool = true;
                go.ThisGameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 清除对象池里的未使用Obj，直到对象池总数量小于objMinNum
        /// </summary>
        public void CleanPool()
        {
            int totalCount = objects.Count;
            List<int> deleteList = new List<int>();
            for (int i = 0; i < objects.Count && totalCount > objMinNum; i++)
            {
                var go = objects[i];
                if (go.InPool)
                {
                    deleteList.Add(i);
                    
                }
            }
            for (int i = deleteList.Count-1; i >= 0; i--)
            {
                var go = objects[deleteList[i]];
                objects.RemoveAt(deleteList[i]);
                Destroy(go.ThisGameObject);
            }
        }
        
        public bool Contain(IReuseable go)
        {
            return objects.Contains(go);
        }

        public IReuseable[] GetAllActiveObjects()
        {
            return objects.FindAll(x => x.InPool == false).ToArray();
        }
    }

}
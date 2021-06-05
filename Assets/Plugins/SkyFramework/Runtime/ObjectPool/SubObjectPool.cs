using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SkyFrameWork
{
    public class SubObjectPool : MonoBehaviour
    {
        private string poolID;

        /// <summary>
        /// 对象预设体
        /// </summary>
        private ReuseableObject objectPrefab;
        
        private bool prefabIsNull = true;

        private int objMinNum = 0;

        public bool autoKill = true;

        public int ObjMinNum
        {
            get => objMinNum;
            set => objMinNum = value;
        }

        /// <summary>
        /// 对象池集合
        /// </summary>
        [Disable]
        [SerializeField]
        private List<ReuseableObject> objects = new List<ReuseableObject>();

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
        internal static SubObjectPool CreatePool(string id, ReuseableObject prefab,int minNum = 0)
        {
            GameObject go = new GameObject(id);
            SubObjectPool subObjectPool = go.AddComponent<SubObjectPool>();
            subObjectPool.poolID = id;
            subObjectPool.BindPrefab(prefab);
            subObjectPool.ObjMinNum = minNum;
            return subObjectPool;
        }

        private void BindPrefab(ReuseableObject prefab)
        {
            objectPrefab = prefab;
            prefabIsNull = objectPrefab == null;
        }
        
        public ReuseableObject Spawn()
        {
            if (prefabIsNull)
            {
                ObjectPool.LogError($"对象池 {poolID} 的预设体不存在");
                return null;
            }

            ReuseableObject go = null;
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
                go = Instantiate(objectPrefab.gameObject).GetComponent<ReuseableObject>();
                go.objPoolID = poolID;
                objects.Add(go);
            }

            go.InPool = false;
            go.gameObject.SetActive(true);
            go.OnSpawn();
            return go;
        }
        
        public void SpawnAsync(Action<ReuseableObject> callback)
        {
            StartCoroutine(SpawnAsyncInternal(callback));
        }

        private IEnumerator SpawnAsyncInternal(Action<ReuseableObject> callback)
        {
            if (prefabIsNull)
            {
                ObjectPool.LogError($"对象池 {poolID} 的预设体不存在");
                yield break;
            }
            
            ReuseableObject go = null;
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
                go = Instantiate(objectPrefab.gameObject).GetComponent<ReuseableObject>();
                go.objPoolID = poolID;
                objects.Add(go);
            }

            go.InPool = false;
            go.gameObject.SetActive(true);
            go.OnSpawn();
            callback?.Invoke(go);
        }

        public bool Unspawn(ReuseableObject go)
        {
            if (!Contain(go))
            {
                ObjectPool.LogError($"回收对象失败，{go.name} 不是对象池 {PoolID} 中的对象");
                return false;
            }

            go.OnUnspawn();
            go.transform.SetParent(transform,false);
            go.InPool = true;
            go.gameObject.SetActive(false);
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
                go.transform.SetParent(transform,false);
                go.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 填充对象池直到达到num
        /// </summary>
        /// <param name="num"></param>
        public void Fill(int num)
        {
            if (prefabIsNull)
            {
                ObjectPool.LogError($"对象池 {poolID} 的预设体不存在");
            }

            int totalCount = objects.Count;
            for (int i = totalCount; i < num; i++)
            {
                ReuseableObject go = Instantiate(objectPrefab.gameObject).GetComponent<ReuseableObject>();
                objects.Add(go);
                go.InPool = true;
                go.gameObject.SetActive(false);
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
                Destroy(go.gameObject);
            }
        }
        
        public bool Contain(ReuseableObject go)
        {
            return objects.Contains(go);
        }

        public ReuseableObject[] GetAllActiveObjects()
        {
            return objects.FindAll(x => x.InPool == false).ToArray();
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    [DisallowMultipleComponent]
    public abstract class ReuseableObject : MonoBehaviour, IReuseable
    {
        [Disable]
        public string objPoolID;
        public bool InPool { get; set; }
        public abstract void OnSpawn();

        public abstract void OnUnspawn();

        public void Unspawn()
        {
            if(InPool == false)
                ObjectPool.Instance.Unspawn(objPoolID, this);
        }
    }

}
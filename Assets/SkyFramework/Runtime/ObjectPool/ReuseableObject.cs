using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFramework
{
    public abstract class ReuseableObject : MonoBehaviour, IReuseable
    {
        public string ObjPoolID { get; set; }
        public GameObject ThisGameObject => gameObject;
        public bool InPool { get; set; }
        public abstract void OnSpawn();

        public abstract void OnUnspawn();

        public void Unspawn()
        {
            if(InPool == false)
                ObjectPoolSystem.Instance.Unspawn(ObjPoolID, this);
        }
    }

}
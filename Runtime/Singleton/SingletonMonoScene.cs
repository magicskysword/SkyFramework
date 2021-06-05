using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    public class SingletonMonoScene<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFramework
{
    /// <summary>
    /// 单例Mono类，不会自动生成，需要手动放置在初始化场景
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance => instance;

        public virtual void Awake()
        {
            instance = this as T;
            if(transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }
    }
}
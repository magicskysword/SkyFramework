using System;
using System.Collections;
using System.Collections.Generic;
using SkyFramework;
using UnityEngine;

namespace SkyFramework
{
    public abstract class ApplicationBase<T> : SingletonMono<T> where T : MonoBehaviour
    {
        public override void Awake()
        {
            base.Awake();
            StartCoroutine(Initialize());
        }

        /// <summary>
        /// 启动初始化
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator Initialize()
        {
            yield return StartCoroutine(BeforeInitialize());
            yield return StartCoroutine(Facade.Instance.Initialize());
            yield return StartCoroutine(AfterInitialize());
        }

        /// <summary>
        /// 初始化前执行
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator BeforeInitialize();
        
        /// <summary>
        /// 初始化后执行
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator AfterInitialize();
    }
}

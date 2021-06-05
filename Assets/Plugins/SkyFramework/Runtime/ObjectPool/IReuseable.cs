using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    /// <summary>
    /// 可被对象池创建的接口
    /// </summary>
    public interface IReuseable
    {
        /// <summary>
        /// 是否在对象池内
        /// </summary>
        bool InPool { get; set; }

        /// <summary>
        /// 当从对象池创建时调用
        /// </summary>
        void OnSpawn();
        /// <summary>
        /// 当放回对象池时调用
        /// </summary>
        void OnUnspawn();
    }

}
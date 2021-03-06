using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFramework
{
    /// <summary>
    /// 可被对象池创建的接口
    /// </summary>
    public interface IReuseable
    {
        /// <summary>
        /// 对象池指向的对象
        /// </summary>
        GameObject ThisGameObject { get; }
        
        string ObjPoolID { set; get; }
        
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
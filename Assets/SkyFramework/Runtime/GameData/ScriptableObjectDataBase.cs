using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

namespace SkyFramework
{
    /// <summary>
    /// 创建资源的方法：
    /// <para>[CreateAssetMenu(menuName = "Asset Name")]</para>
    /// </summary>
    public abstract class ScriptableObjectDataBase : ScriptableObject,IGameData
    { 
        public string assetID = "";
        public string DataID => assetID;
    }
}
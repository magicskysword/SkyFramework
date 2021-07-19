using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SkyFramework
{
    [CreateAssetMenu(menuName = "SkyFramework/LuaConfig")]
    public class LuaConfig : ConfigBase
    {
        [Title("Lua 配置")]
        
        [LabelText("Lua加载地址")]
        [FolderPath]
        public List<string> luaPath;
    }

}
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkyFramework
{
    public class ConfigSystem
    {
        public const string CONFIG_DIR = "Assets/SkyFramework/Resources";

        public static T GetOrCreateConfig<T>() where T : ConfigBase
        {
            var configPath = $"{CONFIG_DIR}/{typeof(T).Name}.asset";
#if UNITY_EDITOR
            if (!File.Exists(configPath))
            {
                T config = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(config,configPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"配置文件{typeof(T).Name}不存在，已经自动创建于 {configPath}");
                return config;
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<T>(configPath);
            }
#else
            return Resources.Load<T>(configPath);
#endif
        }
    }
}
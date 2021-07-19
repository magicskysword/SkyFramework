using System;
using System.Collections;
using System.Collections.Generic;
using SkyFramework;
using UnityEngine;
using FairyGUI;
using UnityEditor;

namespace SkyFramework
{
    public class UISystem : SingletonMono<UISystem>
    {
        public UIPanel uiPanel;
        public StageCamera stageCamera;

        private Dictionary<string, IUIBase> uiDic = new Dictionary<string, IUIBase>();

        private static UIPackage.LoadResource _loadFromAssetsPath = (string name, string extension, System.Type type, out DestroyMethod destroyMethod) =>
        {
            destroyMethod = DestroyMethod.Unload;
            var resourcesSystem = ResourcesSystem.Instance;
            if (resourcesSystem.HaveAsset(name + extension))
            {
                return resourcesSystem.LoadAsset(name + extension, type);
            }
            return null;
        };
        
        public void AddPackage(string packagePath)
        {
            UIPackage.AddPackage(packagePath, _loadFromAssetsPath);
        }

        public T CreateUI<T>(string uiID,string pkgName, string resName,UIType uiType) where T : class, IUIBase, new()
        {
            if (uiDic.ContainsKey(uiID))
            {
                LogError($"UI：{uiID} 已经存在，无法重复创建。");
                return null;
            }
            
            GComponent view = UIPackage.CreateObject(pkgName, resName).asCom;
            T uiBase = new T();
            uiBase.ContentPane = view;
            uiBase.UIType = uiType;

            uiDic.Add(uiID,uiBase);
            switch (uiType)
            {
                case UIType.Root:
                    GRoot.inst.AddChildAt(view, 0);
                    break;
                case UIType.Normal:
                    break;
                case UIType.Model:
                    break;
            }
            
            uiBase.Create();
            return uiBase;
        }

        public T GetUI<T>(string uiID) where T : class, IUIBase
        {
            uiDic.TryGetValue(uiID, out var uiBase);
            return uiBase as T;
        }

        public void DestroyUI(string uiID)
        {
            if (!uiDic.TryGetValue(uiID,out var uiBase))
            {
                LogError($"UI：{uiID} 不存在，销毁失败。");
                return;
            }
            uiBase.Destroy();
            uiDic.Remove(uiID);
        }

        public void Clear()
        {
            foreach (var kvp in uiDic)
            {
                kvp.Value.Destroy();
            }
            uiDic.Clear();
        }
        
        public static void Log(string msg)
        {
            Debug.Log($"<color=cyan>[{nameof(UISystem)}]</color>{msg}");
        }
        
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=cyan>[{nameof(UISystem)}]</color>{msg}");
        }
        
        public static void LogError(string msg)
        {
            Debug.LogError($"<color=cyan>[{nameof(UISystem)}]</color>{msg}");
        }

    }
}

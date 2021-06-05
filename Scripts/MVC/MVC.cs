using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SkyFrameWork
{
    public static class MVC
    {
        // 初始化   //////////////////////////////////////////
        public static void Initialize()
        {
            models = new Dictionary<string, MVCModel>();
            views = new Dictionary<string, MVCView>();
            commandMap = new Dictionary<string, MVCController>();
        }
        
        // 存储MVC //////////////////////////////////////////

        /// <summary>
        /// 名称 -- 模型
        /// </summary>
        public static Dictionary<string, MVCModel> models;
        /// <summary>
        /// 名称 -- 视图
        /// </summary>
        public static Dictionary<string, MVCView> views;
        /// <summary>
        /// 名称 -- 控制器类型
        /// </summary>
        public static Dictionary<string, MVCController> commandMap;
        
        // 注册 /////////////////////////////////////////////

        public static void RegisterModel(MVCModel model)
        {
            models[model.Name] = model;
            Log($"Model {model.Name} 注册成功");
        }
        
        public static void RegisterView(MVCView view)
        {
            view.OnRegisterEvents();
            views[view.Name] = view;
            Log($"View {view.Name} 注册成功");
        }
        
        public static void UnregisterView(MVCView view)
        {
            views.Remove(view.Name);
        }

        public static void RegisterController(string eventName, Type type)
        {
            MVCController mvcController = Activator.CreateInstance(type) as MVCController;
            commandMap[eventName] = mvcController;
            Log($"Controller {type} 注册成功");
        }
        
        // 获取 /////////////////////////////////////////////

        public static T GetModel<T>() where T : MVCModel
        {
            foreach (var model in models.Values)
            {
                if (model is T mvcModel)
                    return mvcModel;
            }

            return null;
        }
        
        public static T GetView<T>() where T : MVCView
        {
            foreach (var view in views.Values)
            {
                if (view is T mvcView)
                    return mvcView;
            }

            return null;
        }
        
        
        // 发送事件 //////////////////////////////////////////
        public static void SendEvent(string eventName, object data = null)
        {
            // 控制器响应事件
            if (commandMap.TryGetValue(eventName,out var controller))
            {
                controller.Execute(data);
            }

            foreach (var mvcView in views.Values)
            {
                if (mvcView.attationEvents.Contains(eventName))
                {
                    mvcView.HandleEvent(eventName,data);
                }
            }
        }
        
        public static void Log(string msg)
        {
            Debug.Log($"<color=white>[MVC]</color>{msg}");
        }
        
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=white>[MVC]</color>{msg}");
        }
        
        public static void LogError(string msg)
        {
            Debug.LogError($"<color=white>[MVC]</color>{msg}");
        }
    }
}
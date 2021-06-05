using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkyFrameWork
{
    public abstract class MVCController
    {
        protected T GetModel<T>() where T : MVCModel
        {
            return MVC.GetModel<T>();
        }
        
        protected T GetView<T>() where T : MVCView
        {
            return MVC.GetView<T>();
        }

        protected void RegisterModel(MVCModel model)
        {
            MVC.RegisterModel(model);
        }
        
        protected void RegisterView(MVCView view)
        {
            MVC.RegisterView(view);
        }
        
        protected void RegisterController(string eventID,Type controllerType)
        {
            MVC.RegisterController(eventID,controllerType);
        }
        
        protected void RegisterView<T>(string parentObj = "") where T : MVCView
        {
            T view = null;
            if (string.IsNullOrEmpty(parentObj))
            {
                view = GameObject.FindObjectOfType<T>();
            }
            else
            {
                Transform parent = GameObject.Find(parentObj).transform;
                int count = parent.childCount;
                for (int i = 0; i < count; i++)
                {
                    view = parent.GetChild(i).GetComponent<T>();
                    if(view != null)
                        break;
                }
            }
            MVC.RegisterView(view);
        }
        
        public abstract void Execute(object data = null);

        protected void Log(string msg)
        {
            Debug.Log($"<color=white>[MVCController]{GetType()}:</color>{msg}");
        }
        
        protected void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=white>[MVCController]{GetType()}:</color>{msg}");
        }
        
        protected void LogError(string msg)
        {
            Debug.LogError($"<color=white>[MVCController]{GetType()}:</color>{msg}");
        }
    }
}

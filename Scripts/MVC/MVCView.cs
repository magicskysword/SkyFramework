using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    public abstract class MVCView : MonoBehaviour
    {
        public abstract string Name { get; }
        /// <summary>
        /// 事件列表
        /// </summary>
        [HideInInspector]
        public List<string> attationEvents = new List<string>();

        public abstract void OnRegisterEvents();
        public abstract void HandleEvent(string eventName, object data = null);

        protected T GetModel<T>() where T : MVCModel
        {
            return MVC.GetModel<T>();
        }
        
        protected void SendEvent(string eventName, object data = null)
        {
            MVC.SendEvent(eventName,data);
        }

        protected void RegisterEvent(string eventID)
        {
            attationEvents.Add(eventID);
        }
        
        protected virtual void Log(string msg)
        {
            Debug.Log($"<color=white>[MVCView]{Name}:</color>{msg}");
        }
        
        protected virtual void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=white>[MVCView]{Name}:</color>{msg}");
        }
        
        protected virtual void LogError(string msg)
        {
            Debug.LogError($"<color=white>[MVCView]{Name}:</color>{msg}");
        }
    }
}
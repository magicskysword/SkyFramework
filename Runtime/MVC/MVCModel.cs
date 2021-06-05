using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    public abstract class MVCModel
    {
        public abstract string Name { get; }

        protected void SendEvent(string eventName, object data = null)
        {
            MVC.SendEvent(eventName,data);
        }
        
        protected void Log(string msg)
        {
            Debug.Log($"<color=white>[MVCModel]{Name}:</color>{msg}");
        }
        
        protected void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=white>[MVCModel]{Name}:</color>{msg}");
        }
        
        protected void LogError(string msg)
        {
            Debug.LogError($"<color=white>[MVCModel]{Name}:</color>{msg}");
        }
    }

}
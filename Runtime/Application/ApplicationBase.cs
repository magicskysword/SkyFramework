using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;

namespace SkyFrameWork
{
    public abstract class ApplicationBase<T> : SingletonMono<T> where T : MonoBehaviour
    {
        protected void RegisterController(string eventName, Type controllerType)
        {
            MVC.RegisterController(eventName, controllerType);
        }

        protected void SendEvent(string eventName, object eventArgs = null)
        {
            MVC.SendEvent(eventName, eventArgs);
        }
    }
}

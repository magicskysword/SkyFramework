using System;
using System.Collections;
using System.Collections.Generic;
using SkyFramework;
using UnityEngine;
using UnityEngine.Events;

namespace SkyFramework
{
    /// <summary>
    /// 事件中心
    /// </summary>
    public class EventSystem : SingletonMono<EventSystem>
    {
        private Dictionary<string, UnityAction<IEventArgs>> eventDic = new Dictionary<string, UnityAction<IEventArgs>>();

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="action"></param>
        public void AddEvent(string eventID, UnityAction<IEventArgs> action)
        {
            if (eventDic.ContainsKey(eventID))
            {
                eventDic[eventID] += action;
            }
            else
            {
                eventDic[eventID] = action;
            }
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="args"></param>
        public void SendEvent(string eventID, IEventArgs args = null)
        {
            if (eventDic.ContainsKey(eventID))
            {
                eventDic[eventID].Invoke(args);
            }
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="action"></param>
        public void RemoveEvent(string eventID, UnityAction<IEventArgs> action)
        {
            if (eventDic.ContainsKey(eventID))
            {
                eventDic[eventID] -= action;
            }
        }

        /// <summary>
        /// 清空事件中心
        /// </summary>
        public void Clear()
        {
            eventDic.Clear();
        }
    }
}

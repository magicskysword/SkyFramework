using System;
using System.Collections;
using System.Collections.Generic;
using SkyFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SkyFramework
{
    public class InputCenterSystem : SingletonMono<InputCenterSystem>
    {
        private Dictionary<string, InputEvent> inputEvents = new Dictionary<string, InputEvent>();
        public bool GlobalEnable { get; set; }

        public void RegisterInput(InputEvent inputEvent)
        {
            inputEvents[inputEvent.EventID] = inputEvent;
        }

        public InputEvent GetInput(string id)
        {
            if (inputEvents.TryGetValue(id, out var inputEvent))
                return inputEvent;
            return null;
        }
        
        public void RemoveInput(string id)
        {
            if (inputEvents.ContainsKey(id))
                inputEvents.Remove(id);
        }
        
        public void Clear()
        {
            inputEvents.Clear();
        }

        private void Start()
        {
            GlobalEnable = true;
        }

        public void Update()
        {
            if(!GlobalEnable)
                return;
            
            foreach (var inputEvent in inputEvents)
            {
                inputEvent.Value.CheckState();
            }
        }
    }
}
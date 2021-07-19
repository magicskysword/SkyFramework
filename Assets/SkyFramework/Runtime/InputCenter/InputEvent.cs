using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SkyFramework
{
    public class InputEvent
    {
        public string EventID { get;}

        private bool enable;
        public bool Enable
        {
            get => enable;
            set
            {
                if (enable != value)
                {
                    ResetState();
                }
                enable = value;
            }
        }

        /// <summary>
        /// 当前按键是否按下
        /// </summary>
        public bool IsPress { get; private set; }
        /// <summary>
        /// 当前轴速度
        /// </summary>
        public float Axis { get; private set; }
        /// <summary>
        /// 轴加速度，仅对键盘与按钮生效。
        /// </summary>
        public float AxisAcceleratedSpeed { get; set; }
        
        /// <summary>
        /// 按钮处于持续按下状态时
        /// </summary>
        public event Action<InputEvent> InputRun;
        /// <summary>
        /// 按钮刚按下时
        /// </summary>
        public event Action<InputEvent> InputDown;
        /// <summary>
        /// 按钮刚抬起时
        /// </summary>
        public event Action<InputEvent> InputUp;
        /// <summary>
        /// 按钮持续更新
        /// </summary>
        public event Action<InputEvent> InputUpdate;
        
        public HashSet<KeyControl> keyControls = new HashSet<KeyControl>();
        public HashSet<KeyControl> keyAxisDown = new HashSet<KeyControl>();
        public HashSet<KeyControl> keyAxisUp = new HashSet<KeyControl>();

        public HashSet<ButtonControl> buttonControls = new HashSet<ButtonControl>();
        public HashSet<ButtonControl> buttonAxisDown = new HashSet<ButtonControl>();
        public HashSet<ButtonControl> buttonAxisUp = new HashSet<ButtonControl>();
        
        public HashSet<AxisControl> axisControls = new HashSet<AxisControl>();

        private bool lastPress = false;

        private InputEvent()
        {
            AxisAcceleratedSpeed = 1;
            Enable = true;
        }
        
        public InputEvent(string id) : this()
        {
            EventID = id;
        }
        
        protected virtual void OnInputUpdate()
        {
            InputUpdate?.Invoke(this);
        }

        /// <summary>
        /// 重设输入状态
        /// </summary>
        public void ResetState()
        {
            lastPress = false;
            IsPress = false;
            Axis = 0;
        }
        
        public virtual void OnInputRun()
        {
            InputRun?.Invoke(this);
        }
        
        public virtual void OnInputDown()
        {
            InputDown?.Invoke(this);
        }

        public virtual void OnInputUp()
        {
            InputUp?.Invoke(this);
        }

        public void CheckState()
        {
            if(Enable)
            {
                CheckPress();
                CheckAxis();
                OnInputUpdate();
            }
        }

        private void CheckPress()
        {
            var curPress = false;
            foreach (var keyControl in keyControls)
            {
                if (keyControl.isPressed)
                    curPress = true;
            }
            foreach (var buttonControl in buttonControls)
            {
                if (buttonControl.isPressed)
                    curPress = true;
            }

            IsPress = curPress;
            if (curPress)
            {
                // 按下
                if (lastPress == false)
                    OnInputDown();
                // 当前按下
                OnInputRun();
            }
            else if(lastPress)
            {
                // 当前抬起
                OnInputUp();
            }
            lastPress = curPress;
        }

        private void CheckAxis()
        {
            float hardAxis = 0;
            float softAxis = 0;

            foreach (var keyControl in keyAxisUp)
            {
                if (keyControl.isPressed)
                    hardAxis += 1;
            }
            
            foreach (var keyControl in keyAxisDown)
            {
                if (keyControl.isPressed)
                    hardAxis -= 1;
            }
            
            foreach (var buttonControl in buttonAxisUp)
            {
                if (buttonControl.isPressed)
                    hardAxis += 1;
            }
            
            foreach (var buttonControl in buttonAxisDown)
            {
                if (buttonControl.isPressed)
                    hardAxis -= 1;
            }

            hardAxis = Mathf.Clamp(hardAxis, -1, 1) * AxisAcceleratedSpeed * Time.unscaledDeltaTime;

            foreach (var axisControl in axisControls)
            {
                softAxis = axisControl.ReadValue();
            }

            if (softAxis != 0)
            {
                Axis = softAxis;
            }
            else
            {
                Axis += hardAxis;
                Axis = Mathf.Clamp(Axis, -1, 1);
            }
            
        }
        
    }
}
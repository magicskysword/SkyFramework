using System;
using System.Collections.Generic;
using XLua;

namespace SkyFramework
{
    public class LuaWindow : BaseWindow
    {
        private LuaTable env;
        private string luaChunkName = null;

        public Action luaOnInit;
        public Action luaOnShown;
        public Action luaOnHide;
        public Action luaOnUpdate;
        public Action luaDoShowAnimation;
        public Action luaDoHideAnimation;
        
        public override void Create()
        {
            env = LuaSystem.Instance.GetNewEnvTable();
            env.Set("self",this);
        }

        public void BindLua(string luaPath)
        {
            if(!string.IsNullOrEmpty(luaChunkName))
                return;
            luaChunkName = luaPath;
            LuaSystem.Instance.Load(luaChunkName,env);

            luaOnInit = env.Get<Action>(nameof(OnInit));
            luaOnShown = env.Get<Action>(nameof(OnShown));
            luaOnHide = env.Get<Action>(nameof(OnHide));
            luaOnUpdate = env.Get<Action>(nameof(OnUpdate));
            luaDoShowAnimation = env.Get<Action>(nameof(DoShowAnimation));
            luaDoHideAnimation = env.Get<Action>(nameof(DoHideAnimation));
        }

        protected override void OnInit()
        {
            luaOnInit?.Invoke();
        }

        protected override void OnShown()
        {
            luaOnShown?.Invoke();
        }

        protected override void OnHide()
        {
            luaOnHide?.Invoke();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            luaOnUpdate?.Invoke();
        }

        protected override void DoShowAnimation()
        {
            if (luaDoShowAnimation != null)
            {
                luaDoShowAnimation.Invoke();
            }
            else
            {
                OnShown();
            }
        }

        protected override void DoHideAnimation()
        {
            if (luaDoHideAnimation != null)
            {
                luaDoHideAnimation.Invoke();
            }
            else
            {
                HideImmediately();
            }
        }
    }
}
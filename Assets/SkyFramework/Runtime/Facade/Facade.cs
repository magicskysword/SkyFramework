using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFramework
{
    public class Facade : SingletonMono<Facade>,IInitialize
    {
        private bool isInitialize = false;
        
        public ObjectPoolSystem objectPoolSystem;
        public ResourcesSystem resourcesSystem;
        public GameDataSystem gameDataSystem;
        public InputCenterSystem inputCenterSystem;
        public EventSystem eventSystem;
        public UISystem uiSystem;
        public LuaSystem luaSystem;
        public SoundSystem soundSystem;
        
        public static ObjectPoolSystem ObjectPoolSystem => Instance.objectPoolSystem;
        public static ResourcesSystem ResourcesSystem => Instance.resourcesSystem;
        public static GameDataSystem GameDataSystem => Instance.gameDataSystem;
        public static EventSystem EventSystem => Instance.eventSystem;
        public static InputCenterSystem InputCenterSystem => Instance.inputCenterSystem;
        public static UISystem UISystem => Instance.uiSystem;
        public static LuaSystem LuaSystem => Instance.luaSystem;
        public static SoundSystem SoundSystem => Instance.soundSystem;
        
        public IEnumerator Initialize()
        {
            if (isInitialize)
            {
                yield break;
            }
            yield return StartCoroutine(ResourcesSystem.Initialize());
            yield return StartCoroutine(GameDataSystem.Initialize());
            yield return StartCoroutine(LuaSystem.Initialize());
            yield return StartCoroutine(SoundSystem.Initialize());
            isInitialize = true;
        }
    }
}

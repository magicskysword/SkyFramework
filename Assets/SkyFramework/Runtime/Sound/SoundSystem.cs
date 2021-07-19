using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using XLua;
using Debug = UnityEngine.Debug;

namespace SkyFramework
{
    public class SoundSystem : SingletonMono<SoundSystem>,IInitialize
    {
        private SubObjectPool audioSourcePool;
        private Dictionary<string, float> volumes = new Dictionary<string, float>();
        private Dictionary<string, SoundInfo> cacheSound = new Dictionary<string, SoundInfo>();
        private bool isInit = false;

        public IEnumerator Initialize()
        {
            isInit = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            var config = ConfigSystem.GetOrCreateConfig<SoundConfig>();
            ObjectPoolSystem.Instance.CreatePool("soundPlayer", config.audioSourcePrefab);
            ObjectPoolSystem.Instance.TryGetPool("soundPlayer", out audioSourcePool);
            audioSourcePool.autoKill = false;
            foreach (string soundType in config.soundTypes)
            {
                volumes.Add(soundType,1f);
            }

            int totalCount = 0;
            int loadCount = 0;
            foreach (var soundDir in config.soundPath)
            {
                var allSound = ResourcesSystem.Instance.GetAssetsPath(soundDir);
                foreach (var soundPath in allSound)
                {
                    totalCount++;
                    ResourcesSystem.Instance.LoadAssetAsync<SoundInfo>(soundPath, sound =>
                    {
                        loadCount++;
                        AddSoundInfo(sound);
                    });
                }
            }

            while (loadCount < totalCount)
            {
                yield return null;
            }
            
            stopwatch.Stop();
            Log($"初始化成功，耗时 {stopwatch.ElapsedMilliseconds} ms 共加载音频数：{cacheSound.Count}.");
            isInit = true;
        }

        public void AddSoundInfos(IEnumerable<SoundInfo> soundInfos)
        {
            foreach (var soundInfo in soundInfos)
            {
                cacheSound[soundInfo.assetID] = soundInfo;
            }
        }

        public void AddSoundInfo(SoundInfo soundInfo)
        {
            cacheSound[soundInfo.assetID] = soundInfo;
        }
        
        public SoundInfo GetSoundInfo(string soundID)
        {
            cacheSound.TryGetValue(soundID, out SoundInfo soundInfo);
            return soundInfo;
        }

        public float GetVolume(string soundType)
        {
            return volumes[soundType];
        }
        
        public void SetVolume(string soundType,float volume)
        {
            volumes[soundType] = Mathf.Clamp(volume, 0f, 1f);
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item =  (SoundPlayer)o;
                if (item.soundInfo.soundType == soundType && item.IsActive)
                {
                    item.MaxVolume = volume;
                }
            }
        }

        /// <summary>
        /// 播放一个声音，如果达到上限则不会播放
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="fadeTime"></param>
        public SoundPlayer PlaySound(string id,bool isFadeIn = true,float fadeTime = 1f)
        {
            var soundInfo = GetSoundInfo(id);
            if (soundInfo == null)
            {
                LogError($"音频 {id} 不存在");
                return null;
            }
            int maxInstance = soundInfo.maxInstanceCount;
            if (maxInstance > 0 && IsMaxCount(soundInfo))
            {
                return null;
            }

            var player = (SoundPlayer)audioSourcePool.Spawn();
            player.transform.parent = transform;
            player.Init(id,soundInfo,GetVolume(soundInfo.soundType));
            PlaySound(player, isFadeIn, fadeTime);
            return player;
        }

        public void PlaySound(SoundPlayer soundPlayer,bool isFadeIn = true,float fadeTime = 1f)
        {
            soundPlayer.IsActive = true;
            soundPlayer.Play();
            if(isFadeIn)
                soundPlayer.FadeIn(fadeTime);
        }

        public void PauseSound(SoundPlayer soundPlayer,bool isFadeOut = true,float fadeTime = 1f)
        {
            if(isFadeOut)
                soundPlayer.FadeOut(fadeTime,player => player.Pause());
            else
                soundPlayer.Pause();
        }

        public void PauseSound(string id,bool stopAll = true,bool isFadeOut = true,float fadeTime = 1f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var soundPlayer = (SoundPlayer)o;
                if (soundPlayer.soundID == id)
                {
                    if(isFadeOut)
                        soundPlayer.FadeOut(fadeTime,player => player.Pause());
                    else
                        soundPlayer.Pause();
                    if (!stopAll)
                        return;
                }
            }
        }
        
        public void PauseAllSound(bool isFadeOut = true,float fadeTime = 1.0f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item = (SoundPlayer)o;
                PauseSound(item,isFadeOut,fadeTime);
            }
        }

        public void ContinueSound(string id,bool continueAll = true,bool isFadeIn = true,float fadeTime = 1f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var soundPlayer = (SoundPlayer)o;
                if (soundPlayer.soundID == id)
                {
                    PlaySound(soundPlayer,isFadeIn,fadeTime);
                    if (!continueAll)
                        return;
                }
            }
        }
        
        public void ContinueAllSound(bool isFadeIn = true,float fadeTime = 1.0f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item = (SoundPlayer)o;
                if (item.isPause)
                {
                    if (isFadeIn)
                    {
                        PlaySound(item,true,fadeTime);
                    }
                    else
                    {
                        PlaySound(item,false);
                    }
                }
            }
        }

        public void StopSound(string id,bool stopAll = true,bool isFadeOut = true,float fadeTime = 1f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item = (SoundPlayer)o;
                if (item.soundID == id)
                {
                    StopSound(item,isFadeOut,fadeTime);
                    if (!stopAll)
                        return;
                }
            }
        }

        public void StopSound(string soundType,bool isFadeOut = true,float fadeTime = 1f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item = (SoundPlayer)o;
                if (item.soundInfo.soundType == soundType)
                {
                    StopSound(item,isFadeOut,fadeTime);
                }
            }
        }

        public void StopSound(SoundPlayer soundPlayer,bool isFadeOut = true,float fadeTime = 1f)
        {
            soundPlayer.IsActive = false;
            if (isFadeOut)
            {
                soundPlayer.FadeOut(fadeTime,player => player.Stop());
            }
            else
            {
                soundPlayer.Stop();
            }
            
        }

        public void StopAllSound(bool isFadeOut = true,float fadeTime = 1.0f)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            foreach (var o in activeObjects)
            {
                var item = (SoundPlayer)o;
                StopSound(item,isFadeOut,fadeTime);
            }
        }
        
        public int GetSoundCount(string id)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            var soundInfo = cacheSound[id];
            int curCount = 0;
            foreach (var o in activeObjects)
            {
                var item =  (SoundPlayer)o;
                if (item.soundInfo == soundInfo && item.IsActive)
                {
                    curCount++;
                }
            }
            return curCount;
        }
        
        private bool IsMaxCount(SoundInfo soundInfo)
        {
            var activeObjects = audioSourcePool.GetAllActiveObjects();
            int maxCount = soundInfo.maxInstanceCount;
            int curCount = 0;
            foreach (var o in activeObjects)
            {
                var item =  (SoundPlayer)o;
                if (item.soundInfo == soundInfo && item.IsActive)
                {
                    curCount++;
                    if (curCount >= maxCount)
                        return true;
                }
            }

            return false;
        }
        
        public static void Log(string msg)
        {
            Debug.Log($"<color=red>[{nameof(SoundSystem)}]</color>{msg}");
        }
        
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=red>[{nameof(SoundSystem)}]</color>{msg}");
        }
        
        public static void LogError(string msg)
        {
            Debug.LogError($"<color=red>[{nameof(SoundSystem)}]</color>{msg}");
        }
    }

}
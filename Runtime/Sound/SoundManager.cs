using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;

namespace SkyFrameWork
{
    public class SoundManager : SingletonMono<SoundManager>
    {
        public ReuseableObject audioSourcePrefab;

        private SubObjectPool audioSourcePool;
        private Dictionary<SoundType, float> volumes = new Dictionary<SoundType, float>();
        private Dictionary<string, SoundInfo> cacheSound = new Dictionary<string, SoundInfo>();

        public void Initialize(SoundInfo[] soundInfos = null)
        {
            ObjectPool.Instance.CreatePool("soundPlayer", audioSourcePrefab);
            ObjectPool.Instance.TryGetPool("soundPlayer", out audioSourcePool);
            audioSourcePool.autoKill = false;
            foreach (int soundType in Enum.GetValues(typeof(SoundType)))
            {
                volumes.Add((SoundType)soundType,1f);
            }

            if(soundInfos != null)
            {
                AddSoundInfos(soundInfos);
            }
            Log($"初始化完毕，共缓存了 {cacheSound.Count} 首音频。");
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

        public float GetVolume(SoundType soundType)
        {
            return volumes[soundType];
        }
        
        public void SetVolume(SoundType soundType,float volume)
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
            cacheSound.TryGetValue(id, out SoundInfo soundInfo);
            if (soundInfo == null)
            {
                Debug.LogError($"音频 {id} 不存在");
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

        public void StopSound(SoundType soundType,bool isFadeOut = true,float fadeTime = 1f)
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
            Debug.Log($"<color=red>[SoundManager]</color>{msg}");
        }
        
        public static void LogWarning(string msg)
        {
            Debug.LogWarning($"<color=red>[SoundManager]</color>{msg}");
        }
        
        public static void LogError(string msg)
        {
            Debug.LogError($"<color=red>[SoundManager]</color>{msg}");
        }
    }

}
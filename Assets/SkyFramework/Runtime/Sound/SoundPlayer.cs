using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SkyFramework
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : ReuseableObject
    {
        [ReadOnly]
        public string soundID;
        public SoundInfo soundInfo;
        
        [HideInInspector]
        public AudioSource audioSource;
        [HideInInspector]
        public bool isPause = false;

        private float maxVolume;
        private int playIndex = 0;
        private bool isActive = false;
        public bool IsPlayComplete => audioSource.time == 0f;
        public float TrueVolume => soundInfo.volume * maxVolume;

        public float MaxVolume
        {
            get => maxVolume;
            set
            {
                maxVolume = Mathf.Clamp(value, 0f, 1f);
                audioSource.volume = TrueVolume;
            }
        }
        public bool IsActive
        {
            get => isActive && !InPool;
            set => isActive = value;
        }

        public float PlayPercentage
        {
            get => audioSource.time / audioSource.clip.length;
            set => audioSource.time = Mathf.Clamp(value, 0.001f, 1f) * audioSource.clip.length;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if(IsPlayComplete)
                OnPlayComplete();
        }

        public override void OnSpawn()
        {

        }

        public override void OnUnspawn()
        {
            soundID = "";
            soundInfo = null;
            maxVolume = 1;
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
            StopAllCoroutines();
        }

        public void Init(string id, SoundInfo tagSound, float tagVolume)
        {
            soundID = id;
            soundInfo = tagSound;
            maxVolume = tagVolume;
            playIndex = 0;
            audioSource.loop = false;
            
            audioSource.volume = TrueVolume;
            switch (tagSound.playType)
            {
                case SoundPlayType.Once:
                    playIndex = Random.Range(0, tagSound.source.Length);
                    break;
                case SoundPlayType.Order:
                    playIndex = 0;
                    break;
                case SoundPlayType.Random:
                    playIndex = Random.Range(0, tagSound.source.Length);
                    break;
                case SoundPlayType.RandomAndLoop:
                    playIndex = Random.Range(0, tagSound.source.Length);
                    break;
            }
            audioSource.clip = tagSound.source[playIndex];
            audioSource.time = 0.001f;
            isPause = true;
        }

        public void Play()
        {
            audioSource.Play();
            isPause = false;
        }

        public void Pause()
        {
            audioSource.Pause();
            isPause = true;
        }

        public void Stop()
        {
            audioSource.Stop();
            isPause = false;
            Unspawn();
        }

        public void Replay()
        {
            OnReplay();
            audioSource.Play();
            isPause = false;
        }

        public void FadeIn(float fadeTime)
        {
            audioSource.volume = 0f;
            StartCoroutine(FadeTo(TrueVolume, fadeTime));
        }

        /// <summary>
        /// 音效淡出
        /// </summary>
        /// <param name="fadeTime"></param>
        /// <param name="callback"></param>
        public void FadeOut(float fadeTime,Action<SoundPlayer> callback = null)
        {
            StartCoroutine(FadeTo(0f, fadeTime,() =>
            {
                callback?.Invoke(this);
            }));
        }

        public IEnumerator FadeTo(float endValue, float duration, Action callback = null)
        {
            float startValue = audioSource.volume;
            float curTime = 0;
            while (curTime >= duration)
            {
                duration -= Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startValue, endValue,curTime / duration);
                yield return null;
            }
            callback?.Invoke();
        }
        

        public void OnPlayComplete()
        {
            switch (soundInfo.playType)
            {
                case SoundPlayType.Once:
                    Stop();
                    break;
                case SoundPlayType.Order:
                    playIndex = (playIndex + 1) % soundInfo.source.Length;
                    audioSource.clip = soundInfo.source[playIndex];
                    audioSource.time = 0.001f;
                    audioSource.Play();
                    break;
                case SoundPlayType.Random:
                    int oldIndex = playIndex;
                    playIndex = Random.Range(0, soundInfo.source.Length);
                    if (oldIndex == playIndex && soundInfo.source.Length >= 2)
                    {
                        playIndex = (playIndex + 1) % soundInfo.source.Length;
                    }
                    audioSource.clip = soundInfo.source[playIndex];
                    audioSource.time = 0.001f;
                    audioSource.Play();
                    break;
                case SoundPlayType.RandomAndLoop:
                    audioSource.time = 0.001f;
                    audioSource.Play();
                    break;
            }
            
        }

        public void OnReplay()
        {
            audioSource.time = 0.001f;
        }
    }

}
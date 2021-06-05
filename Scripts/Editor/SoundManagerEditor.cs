using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SkyFrameWork;

namespace SkyFrameWork.Editor
{
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : UnityEditor.Editor
    {
        [HideInInspector]
        public SoundManager soundManager = null;
        
        private string selectSound = "";
        private List<SoundInfo> soundInfos;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("音频管理器");
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("游戏未运行");
                return;
            }
            
            soundManager = target as SoundManager;
            
            EditorGUILayout.BeginHorizontal();
            {
                //selectIndex = EditorGUILayout.Popup(selectIndex,GetSoundList());
                selectSound = EditorGUILayout.TextField("音效地址", selectSound);
                if (GUILayout.Button("播放"))
                {
                    PlaySound();
                }
                if (GUILayout.Button("停止"))
                {
                    StopSound();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("全部播放"))
                {
                    PlayAllSound();
                }
                if (GUILayout.Button("全部暂停"))
                {
                    PauseAllSound();
                }
                if (GUILayout.Button("全部停止"))
                {
                    StopAllSound();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private SoundInfo GetCurSound()
        {
            SoundInfo soundInfo = AssetDatabase.LoadAssetAtPath<SoundInfo>(selectSound);
            return soundInfo;
        }
        
        private void PlaySound()
        {
            soundManager.PlaySound(GetCurSound().assetID);
        }
        
        
        private void StopSound()
        {
            soundManager.StopSound(GetCurSound().assetID);
        }

        private void PlayAllSound()
        {
            soundManager.ContinueAllSound();
        }
        
        private void PauseAllSound()
        {
            soundManager.PauseAllSound();
        }
        
        private void StopAllSound()
        {
            soundManager.StopAllSound();
        }
    }

    [CustomEditor(typeof(SoundPlayer))]
    public class SoundPlayerEditor : UnityEditor.Editor
    {
        public SoundPlayer soundPlayer;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            soundPlayer = (SoundPlayer) target;
            int duration = (int) soundPlayer.audioSource.time;
            int total = (int) soundPlayer.audioSource.clip.length;
            int get = (int) GUILayout.HorizontalSlider(duration, 0, total);
            GUILayout.Space(10);
            if (get != duration)
            {
                duration = get;
                soundPlayer.audioSource.time = get;
            }
            TimeSpan timeDuration = new TimeSpan(0, 0,Convert.ToInt32( duration));
            TimeSpan timeTotal = new TimeSpan(0, 0,Convert.ToInt32( total));
            GUILayout.Label($"{timeDuration.Minutes:00}:{timeDuration.Seconds:00} / {timeTotal.Minutes:00}:{timeTotal.Seconds:00}");
        }
    }
}
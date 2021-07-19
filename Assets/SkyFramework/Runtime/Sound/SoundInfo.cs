using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SkyFramework
{
    [CreateAssetMenu(menuName = "SkyFramework/SoundInfo")]
    public class SoundInfo : ScriptableObjectDataBase
    {
        private static HashSet<string> SoundTypesSet => ConfigSystem.GetOrCreateConfig<SoundConfig>().soundTypes;
        
        /// <summary>
        /// 声音源
        /// </summary>
        [LabelText("音频源")]
        public AudioClip[] source;
        /// <summary>
        /// 声音类型
        /// </summary>
        [LabelText("音频类型")]
        [ValueDropdown("SoundTypesSet")]
        public string soundType;
        /// <summary>
        /// 默认最大音量
        /// </summary>
        [LabelText("默认音量")]
        public float volume = 1f;
        /// <summary>
        /// 播放类型
        /// </summary>
        [LabelText("播放类型")]
        public SoundPlayType playType;
        /// <summary>
        /// 最大同时可播放上限
        /// </summary>
        [LabelText("同时可播放上限")]
        public int maxInstanceCount;

        private void Reset()
        {
            soundType = SoundTypesSet.FirstOrDefault();
        }
    }

}
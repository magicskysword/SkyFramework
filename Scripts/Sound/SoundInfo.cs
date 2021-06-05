using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    [CreateAssetMenu(menuName = "Framework Asset/Sound Info")]
    public class SoundInfo : ScriptableObjectAsset
    {
        /// <summary>
        /// 声音源
        /// </summary>
        [Rename("音频源")]
        public AudioClip[] source;

        /// <summary>
        /// 声音类型
        /// </summary>
        [EnumLabel("音频类型",typeof(SoundType))]
        public SoundType soundType;
        /// <summary>
        /// 默认最大音量
        /// </summary>
        [Rename("默认音量")]
        public float volume = 1f;
        /// <summary>
        /// 播放类型
        /// </summary>
        [EnumLabel("播放类型",typeof(SoundPlayType))]
        public SoundPlayType playType;
        /// <summary>
        /// 最大同时可播放上限
        /// </summary>
        [Rename("同时可播放上限")]
        public int maxInstanceCount;


    }

}
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SkyFramework
{
    [CreateAssetMenu(menuName = "SkyFramework/SoundConfig")]
    public class SoundConfig : ConfigBase
    {
        [Title("Sound 配置")]

        [LabelText("SoundInfo加载地址")]
        [FolderPath]
        public List<string> soundPath;
        [LabelText("声音类型")]
        [ShowInInspector]
        public HashSet<string> soundTypes = new HashSet<string>(){"BGM","SFX"};
        [LabelText("音频播放预制体")]
        [AssetsOnly]
        public SoundPlayer audioSourcePrefab;
    }
}
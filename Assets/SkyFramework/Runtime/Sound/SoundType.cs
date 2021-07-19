using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SkyFramework
{
    [Serializable]
    public enum SoundPlayType
    {
        [LabelText("单次播放")]
        Once,
        [LabelText("循环播放")]
        Order,
        [LabelText("随机循环")]
        Random,
        [LabelText("单曲循环")]
        RandomAndLoop
    }
}
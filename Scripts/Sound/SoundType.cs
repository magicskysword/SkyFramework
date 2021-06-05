using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyFrameWork
{
    [Serializable]
    public enum SoundType
    {
        Bgm,
        Sfx,
    }

    
    [Serializable]
    public enum SoundPlayType
    {
        Once,
        Order,
        Random,
        RandomAndLoop
    }
}
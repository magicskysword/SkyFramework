using System;
using SkyFramework;
using UnityEngine;

namespace SkyFramework
{
    [Serializable]
    public class GameDataBase : MonoBehaviour,IGameData
    {
        public string dataID;
        
        public string DataID
        {
            get => dataID;
            set => dataID = value;
        }
    }
}
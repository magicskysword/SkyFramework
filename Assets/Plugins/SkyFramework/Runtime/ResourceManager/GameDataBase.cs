using System;
using SkyFrameWork;

namespace SkyFrameWork
{
    [Serializable]
    public class GameDataBase : IGameData
    {
        [Rename("数据ID")]
        public string dataID;
        
        public string DataID
        {
            get => dataID;
            set => dataID = value;
        }
    }
}
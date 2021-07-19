using UnityEngine;

namespace SkyFramework
{
    public interface IAsset
    {
        Object GetObjectAsset();
        void Release();
    }
}
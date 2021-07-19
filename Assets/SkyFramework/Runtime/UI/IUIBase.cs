using FairyGUI;

namespace SkyFramework
{
    public interface IUIBase
    {
        GComponent ContentPane { get; set; }
        UIType UIType { get; set; }
        void Create();
        void Destroy();
    }
}
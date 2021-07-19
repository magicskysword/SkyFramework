using FairyGUI;

namespace SkyFramework
{
    public class BaseWindow: Window,IUIBase
    {
        public GComponent ContentPane
        {
            get => contentPane;
            set => contentPane = value;
        }
        public UIType UIType { get; set; }
        public virtual void Create()
        {
            
        }

        public virtual void Destroy()
        {
            if (ContentPane != null)
            {
                ContentPane.Dispose();
                ContentPane = null;
            }
        }
    }
}
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace SkyFramework.Editor
{
    public class ConfigWindow : OdinMenuEditorWindow
    {
        [MenuItem("SkyFramework/Config Setting")]
        private static void OpenWindow()
        {
            GetWindow<ConfigWindow>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            titleContent.text = "SkyFramework 配置窗口";
            var tree = new OdinMenuTree();
            tree.Add("Lua设置", ConfigSystem.GetOrCreateConfig<LuaConfig>());
            tree.Add("Sound设置", ConfigSystem.GetOrCreateConfig<SoundConfig>());
            return tree;
        }
    }
}
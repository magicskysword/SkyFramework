using System.Collections;
using System.Collections.Generic;
using SkyFramework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : ApplicationBase<Test>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override IEnumerator BeforeInitialize()
    {
        yield break;
    }

    protected override IEnumerator AfterInitialize()
    {
        Facade.ResourcesSystem.LoadAssetAsync<GameObject>("Assets/Test/BuildResources/GameObject.prefab",
            o =>
            {
                var go = Instantiate(o);
                go.name = "generic";
            });
        Facade.ResourcesSystem.LoadAssetAsync("Assets/Test/BuildResources/GameObject.prefab",typeof(GameObject),
            o =>
            {
                var go = Instantiate(o as GameObject);
                go.name = "nongeneric";
            });
        Facade.UISystem.AddPackage("Assets/Test/BuildResources/FairyGUI/Bag");
        Facade.UISystem.CreateUI<BagWindow>("BagWindow", "Bag", "Main", UIType.Normal);
        Facade.LuaSystem.DoString("require(\"test\")");
        Facade.LuaSystem.Load("test");
        var inputTest = new InputEvent("text");
        inputTest.keyControls.Add(Keyboard.current.tKey);
        inputTest.InputDown += inputEvent =>
        {
            Facade.LuaSystem.DoString("print(\"hello!\")");
        };
        Facade.InputCenterSystem.RegisterInput(inputTest);
        yield break;
    }
}

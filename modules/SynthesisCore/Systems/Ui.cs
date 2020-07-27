using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;

namespace SynthesisCore.Systems
{
    public class Ui : SystemBase
    {
        public override void Setup()
        {
            var tabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Tab.uxml");
            Tab engineTab = new Tab("Engine", tabAsset, null);
            
            UIManager.AddTab(engineTab);
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }
    }
}
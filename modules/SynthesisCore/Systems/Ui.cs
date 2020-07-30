using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class Ui : SystemBase
    {
        public override void Setup()
        {
            var tabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/Tab.uxml");
            Tab engineTab = new Tab("Engine", tabAsset, null);

            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/Environments.uxml");
            Panel environmentsWindow = new Panel("Environments", environmentsAsset, null);
            
            UIManager.AddTab(engineTab);
            UIManager.AddPanel(environmentsWindow);
            
            UIManager.ShowPanel("Environments");
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }
    }
}
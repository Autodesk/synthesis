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
            var tabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Tab.uxml");
            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Environments.uxml");
            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Modules.uxml");
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Settings.uxml");
            
            Tab engineTab = new Tab("Engine", tabAsset, null);
            Panel environmentsWindow = new Panel("Environments", environmentsAsset, null);
            Panel modulesWindow = new Panel("Modules", modulesAsset, null);
            Panel settingsWindow = new Panel("Settings", settingsAsset, null);
            
            UIManager.AddTab(engineTab);
            UIManager.AddPanel(environmentsWindow);
            UIManager.AddPanel(modulesWindow);
            UIManager.AddPanel(settingsWindow);
            
            //UIManager.ShowPanel("Environments");
            //UIManager.ShowPanel("Modules");
            //UIManager.ShowPanel("Settings");
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }
    }
}
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class Ui : SystemBase
    {
        public static VisualElementAsset ToolbarAsset;
        public static VisualElementAsset ToolbarButtonAsset;
        public static VisualElementAsset ToolbarCategoryAsset;

        private static bool IsToolbarVisible = true;

        public override void Setup()
        {
            var blankTabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            ToolbarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Toolbar.uxml");
            ToolbarButtonAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ToolbarButton.uxml");
            ToolbarCategoryAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ToolbarCategory.uxml");

            UIManager.SetBlankTabAsset(blankTabAsset);

            var titleBarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/TitleBar.uxml");
            UIManager.SetTitleBar(titleBarAsset.GetElement("").Get(name: "title-bar"));

            EngineToolbar.CreateToolbar();

            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Modules.uxml");
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            var jointsAssset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Panel modulesWindow = new Panel("Modules", modulesAsset, 
                element =>
                {
                    Utilities.RegisterOKCloseButtons(element, "Modules");
                    LoadModulesWindowContent(element);
                });
            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Settings"));
            Panel jointsWindow = new Panel("Joints", jointsAssset,
            element =>
            {
                Utilities.RegisterOKCloseButtons(element, "Joints");
                LoadJointsWindowContent(element);
            });

            UIManager.AddPanel(modulesWindow);
            UIManager.AddPanel(settingsWindow);

            Button hideToolbarButton = (Button)UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });

            Button modulesButton = (Button) UIManager.RootElement.Get("modules-button");
            modulesButton.Subscribe(x => UIManager.TogglePanel("Modules"));

            Button settingsButton = (Button) UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x => UIManager.TogglePanel("Settings"));

            Button jointsButton = (Button)UIManager.RootElement.Get("joints-button");
            jointsButton.Subscribe(x => UIManager.TogglePanel("Joints"));

            Button helpButton = (Button) UIManager.RootElement.Get("help-button");
            helpButton.Subscribe(x => System.Diagnostics.Process.Start("https://synthesis.autodesk.com"));
        }

        private void LoadModulesWindowContent(VisualElement visualElement)
        {
            var moduleAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Module.uxml");

            foreach (var moduleInfo in ModuleManager.GetLoadedModules())
            {
                VisualElement moduleElement = moduleAsset?.GetElement("module");
                
                Label titleText = (Label) moduleElement?.Get("title");
                Label authorText = (Label) moduleElement?.Get("author");
                Label descriptionText = (Label) moduleElement?.Get("description");

                titleText.Text = titleText.Text
                    .Replace("%name%", moduleInfo.Name)
                    .Replace("%version%", moduleInfo.Version);

                authorText.Text = authorText.Text.Replace("%author%",
                    string.IsNullOrEmpty(moduleInfo.Author) ? "Unknown" : moduleInfo.Author);
                descriptionText.Text = descriptionText.Text.Replace("%description%",
                    string.IsNullOrEmpty(moduleInfo.Description) ? "No description" : moduleInfo.Description);

                ListView moduleList = (ListView) visualElement.Get("module-list");
                moduleList.Add(moduleElement);
            }
            
        }

        private void LoadJointsWindowContent(VisualElement visualElement)
        {
            var jointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/TempJoint.uxml");
            VisualElement jointElement = jointAsset?.GetElement("joint");
            Label titleText = (Label)jointElement?.Get("joint-type");

            titleText.Text = titleText.Text
                   .Replace("%jointName%", "test");
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}
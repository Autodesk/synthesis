using SynthesisAPI.AssetManager;
using SynthesisAPI.Modules;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI.Windows
{
    public class ModuleWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private VisualElementAsset ModuleAsset;
        private ListView ModuleList;

        public ModuleWindow()
        {
            ModuleAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Module.uxml");

            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Modules.uxml");
            Panel = new Panel("Modules", modulesAsset, OnWindowOpen);
            
            Button modulesButton = (Button) UIManager.RootElement.Get("modules-button");
            modulesButton.Subscribe(x => UIManager.TogglePanel("Modules"));
        }
        
        private void OnWindowOpen(VisualElement modulesWindow)
        {
            Window = modulesWindow;
            ModuleList = (ListView) Window.Get("module-list");
            
            LoadWindowContents();
            RegisterButtons();
        }

        private void LoadWindowContents()
        {
            foreach (var moduleInfo in ModuleManager.GetLoadedModules())
            {
                ModuleList.Add(new ModuleItem(ModuleAsset, moduleInfo).ModuleElement);
            }
        }

        private void RegisterButtons()
        {
            // TODO: save / apply changes implementation?
            Button importButton = (Button) Window.Get("import-button");
            importButton?.Subscribe(x => Logger.Log("TODO: Module import"));
            
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }
    }
}
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.UI.Windows;

namespace SynthesisCore.UI
{
    public class GeneralPage
    {
        public VisualElement Page { get; }
        private VisualElementAsset OptionAsset;

        public GeneralPage(VisualElementAsset generalAsset)
        {
            Page = generalAsset.GetElement("page");
            OptionAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Option.uxml");
            
            LoadPageContent();
            RegisterButtons();
        }

        private void LoadPageContent()
        {
            ToggleItem analytics = new ToggleItem(OptionAsset, new Toggle("analytics"), "Analytics");
            Page.Add(analytics.Element);
        }
        
        private void RegisterButtons()
        {
            
        }
    }
}
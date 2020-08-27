using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class GeneralPage
    {
        public VisualElement Page { get; }

        public GeneralPage(VisualElementAsset generalAsset)
        {
            Page = generalAsset.GetElement("page");
            
            LoadPageContent();
            RegisterButtons();
        }

        private void LoadPageContent()
        {
            Dropdown units = new Dropdown("units", "Metric", "Imperial");
            
        }
        
        private void RegisterButtons()
        {
            
        }
    }
}
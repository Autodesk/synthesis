using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class GraphicsPage
    {
        public VisualElement Page { get; }

        public GraphicsPage(VisualElementAsset graphicsAsset)
        {
            Page = graphicsAsset.GetElement("page");
            
            LoadPageContent();
            RegisterButtons();
        }

        private void LoadPageContent()
        {
            
        }
        
        private void RegisterButtons()
        {
            
        }
    }
}
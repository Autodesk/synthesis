using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.UI.Windows;

namespace SynthesisCore.UI
{
    public class GraphicsPage
    {
        public VisualElement Page { get; }
        private VisualElementAsset OptionAsset;

        public GraphicsPage(VisualElementAsset graphicsAsset)
        {
            Page = graphicsAsset.GetElement("page");
            OptionAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Option.uxml");
            
            LoadPageContent();
            RegisterButtons();
        }

        private void LoadPageContent()
        {
            Page.Add(new DropdownItem(OptionAsset, new Dropdown("screen-mode", 
                "Borderless Window", "Windowed", "Maximized Window", "Fullscreen")).Element);

            //Page.Add(new Dropdown("test", "df", "sdfd", "dfdf"));
        }
        
        private void RegisterButtons()
        {
            
        }

    }
}
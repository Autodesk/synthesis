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
            Dropdown screenMode = new Dropdown("screen-mode",
                "Borderless Window", "Windowed", "Maximized Window", "Fullscreen");
            screenMode.ItemHeight = 20;
            Page.Add(new DropdownItem(OptionAsset, screenMode, "Screen Mode").Element);
            
            Dropdown qualityLevel = new Dropdown("quality",
                "Low", "Medium", "High", "Ultra");
            qualityLevel.ItemHeight = 20;
            Page.Add(new DropdownItem(OptionAsset, qualityLevel, "Quality").Element);

            Dropdown resolution = new Dropdown("resolution",
                "1280x720", "1280x768", "1280x800", "1280x1024", "1360x768", "1366x768",
                            "1400x1050", "1440x900", "1600x900", "1680x1050", "1920x1080");
            resolution.ItemHeight = 20;
            Page.Add(new DropdownItem(OptionAsset, resolution, "Screen Resolution").Element);
        }
        
        private void RegisterButtons()
        {
            
        }

    }
}
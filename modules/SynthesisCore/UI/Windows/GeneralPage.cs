using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.UI.Windows;

namespace SynthesisCore.UI
{
    public class GeneralPage
    {
        public VisualElement Page { get; }
        private VisualElementAsset OptionAsset;

        private ToggleItem AnalyticsToggle;
        private DropdownItem UnitsDropdown;
        private DropdownItem QualityDropdown;
        private DropdownItem ResolutionDropdown;
        
        public GeneralPage(VisualElementAsset generalAsset)
        {
            Page = generalAsset.GetElement("page");
            OptionAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Option.uxml");
            
            CreateElements();
            LoadPageContent();
        }

        private void CreateElements()
        {
            AnalyticsToggle = new ToggleItem(OptionAsset, "Analytics",
                new Toggle("analytics"));
            
            UnitsDropdown = new DropdownItem(OptionAsset, "Measurement Units",
                new Dropdown("measurement-units", 0, "Metric", "Imperial"));
            
            QualityDropdown = new DropdownItem(OptionAsset, "Quality",
                new Dropdown("quality", 0, "Low", "Medium", "High", "Ultra"));
            
            ResolutionDropdown = new DropdownItem(OptionAsset, "Screen Resolution",
                new Dropdown("resolution", 0, "1280x720", "1280x768", "1280x800",
                    "1280x1024", "1360x768", "1366x768", "1400x1050", "1440x900", "1600x900",
                    "1680x1050", "1920x1080"));
        }

        private void LoadPageContent()
        {
            Page.Add(AnalyticsToggle.Element);
            Page.Add(UnitsDropdown.Element);
            Page.Add(QualityDropdown.Element);
            Page.Add(ResolutionDropdown.Element);
        }

        public void RefreshPreferences()
        {
            AnalyticsToggle.UpdateInformation();
            UnitsDropdown.UpdateInformation();
            QualityDropdown.UpdateInformation();
            ResolutionDropdown.UpdateInformation();
        }
        
    }
}
using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI.Windows
{
    public class DropdownItem
    {
        public VisualElement Element { get; }
        private Dropdown Dropdown;
        private Label NameLabel;
        private VisualElement ModifierContainer;
        private string OptionName;

        public DropdownItem(VisualElementAsset optionAsset, Dropdown dropdown, string optionName)
        {
            Element = optionAsset.GetElement("option");
            Dropdown = dropdown;
            
            NameLabel = (Label) Element.Get("option-name");
            ModifierContainer = Element.Get("modifier-container");
            OptionName = optionName;
            
            SetInformation();
            RegisterButtons();
        }

        private void SetInformation()
        {
            NameLabel.Text = NameLabel.Text.Replace("%name%", OptionName);
            ModifierContainer.Add(Dropdown);
        }

        private void RegisterButtons()
        {
            Dropdown.OnValueChanged += value =>
            {
                PreferenceManager.SetPreference("SynthesisCore", Dropdown.Name, value);
                PreferenceManager.Save();
            };
        }
    }
}
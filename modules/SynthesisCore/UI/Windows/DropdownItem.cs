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
        private string PreferenceName;

        public DropdownItem(VisualElementAsset optionAsset, Dropdown dropdown, string preferenceName)
        {
            Element = optionAsset.GetElement("option");
            Dropdown = dropdown;
            
            NameLabel = (Label) Element.Get("option-name");
            ModifierContainer = Element.Get("modifier-container");
            PreferenceName = preferenceName;
            
            SetInformation();
            RegisterButtons();
        }

        private void SetInformation()
        {
            NameLabel.Text = NameLabel.Text.Replace("%name%", PreferenceName);
            Dropdown.Selected = GetPreference();
            ModifierContainer.Add(Dropdown);
        }

        private void RegisterButtons()
        {
            Dropdown.OnValueChanged += value =>
            {
                PreferenceManager.SetPreference("SynthesisCore", PreferenceName, value);
                PreferenceManager.Save();
            };
        }

        private string GetPreference()
        {
            return PreferenceManager.GetPreference<string>("SynthesisCore", PreferenceName);
        }
    }
}
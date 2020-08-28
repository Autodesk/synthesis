using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI.Windows
{
    public class ToggleItem
    {
        public VisualElement Element;
        private Toggle Toggle;
        private Label NameLabel;
        private VisualElement ModifierContainer;
        private string PreferenceName;
        
        public ToggleItem(VisualElementAsset optionAsset, Toggle toggle, string preferenceName)
        {
            Element = optionAsset.GetElement("option");
            Toggle = toggle;

            NameLabel = (Label) Element.Get("option-name");
            ModifierContainer = Element.Get("modifier-container");
            PreferenceName = preferenceName;
            
            NameLabel.SetStyleProperty("width", "280px");
            ModifierContainer.SetStyleProperty("width", "20px");
            
            SetInformation();
            RegisterButtons();
        }

        private void SetInformation()
        {
            NameLabel.Text = NameLabel.Text.Replace("%name%", PreferenceName);
            ModifierContainer.Add(Toggle);
        }

        private void RegisterButtons()
        {
            Toggle.OnValueChanged += value =>
            {
                SettingsWindow.AddPendingChange(PreferenceName, value);
            };
        }
    }
}
using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class ControlItem
    {
        public VisualElement Element { get; }
        private ControlInfo ControlInfo;
        private Label NameLabel;
        private Button KeyButton;

        public ControlItem(VisualElementAsset controlAsset, ControlInfo controlInfo)
        {
            Element = controlAsset.GetElement("control");
            ControlInfo = controlInfo;
            
            NameLabel = (Label) Element.Get("name");
            KeyButton = (Button) Element.Get("change-key");
            
            SetInformation();
            RegisterButtons();
        }

        private void SetInformation()
        {
            NameLabel.Text = NameLabel.Text.Replace("%name%", ControlInfo.Name);
            KeyButton.Text = KeyButton.Text.Replace("%key%", ControlInfo.Key);
        }

        private void RegisterButtons()
        {
            KeyButton.Subscribe(x =>
            {
                KeyButton.Text = "Press Any Key";
                // wait for key press
                // PreferenceManager.SetPreference("SynthesisCore", ControlInfo.Name, /* updated key button */);
                // PreferenceManager.Save();
            });
        }
    }
}
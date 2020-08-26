using Api.InputManager;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

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
                void Callback(IEvent e)
                {
                    PreferenceManager.SetPreference("SynthesisCore", ControlInfo.Name, ((KeyEvent) e).KeyString);
                    KeyButton.Text = ((KeyEvent) e).KeyString;
                    PreferenceManager.Save();
                    
                    InputManager.UnassignDigitalInput("");
                    
                    InputSystem.IsAwaitingKey = false;
                    
                    EventBus.RemoveTypeListener<KeyEvent>(Callback);
                } 
                if (!InputSystem.IsAwaitingKey)
                {
                    KeyButton.Text = "Press Any Key";
                    EventBus.NewTypeListener<KeyEvent>(Callback);
                    InputSystem.IsAwaitingKey = true;
                }
            });
        }
    }
}
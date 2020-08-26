using Api.InputManager;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
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
                if (!InputSystem.IsAwaitingKey)
                {
                    KeyButton.Text = "Press Any Key";
                    EventBus.NewTypeListener<KeyEvent>(Callback);
                    InputSystem.IsAwaitingKey = true;
                }
                
                void Callback(IEvent e)
                {
                    PreferenceManager.SetPreference("SynthesisCore", ControlInfo.Name, ((KeyEvent) e).KeyString);
                    PreferenceManager.Save();
                    KeyButton.Text = ((KeyEvent) e).KeyString;
                    
                    // InputManager.UnassignDigitalInput(""); TODO unassign previous input
                    InputSystem.IsAwaitingKey = false;
                    EventBus.RemoveTypeListener<KeyEvent>(Callback);
                }
            });
        }
    }
}
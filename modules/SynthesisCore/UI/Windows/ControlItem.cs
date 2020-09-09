using Api.InputManager;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Utilities;

namespace SynthesisCore.UI
{
    public class ControlItem
    {
        public VisualElement Element { get; }
        private string ControlName;
        private Label NameLabel;
        private Button KeyButton;

        public ControlItem(VisualElementAsset controlAsset, string controlName)
        {
            Element = controlAsset.GetElement("control");
            ControlName = controlName;
            
            NameLabel = (Label) Element.Get("name");
            KeyButton = (Button) Element.Get("change-key");
            
            SetInformation();
            RegisterButtons();
        }

        private void SetInformation()
        {
            NameLabel.Text = ControlName;
            KeyButton.Text = GetFormattedPreference(ControlName);
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
                    SettingsWindow.AddPendingChange(ControlName, ((KeyEvent) e).KeyString);
                    KeyButton.Text = StringUtils.ReformatCondensedString(((KeyEvent) e).KeyString);

                    // InputManager.UnassignDigitalInput(""); TODO unassign previous input, assign new one
                    InputSystem.IsAwaitingKey = false;
                    EventBus.RemoveTypeListener<KeyEvent>(Callback);
                }
            });
        }

        private string GetFormattedPreference(string controlName)
        {
            var controlKey = PreferenceManager.GetPreference("SynthesisCore", controlName);
            if (controlKey is string)
            {
                return StringUtils.ReformatCondensedString((string) controlKey);
            }
            return "Unassigned";
        }
        
        public void UpdateInformation()
        {
            SetInformation();
        }
    }
}
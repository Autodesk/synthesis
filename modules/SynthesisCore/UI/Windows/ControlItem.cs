using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class ControlItem
    {
        public VisualElement ControlElement { get; }

        public ControlItem(VisualElementAsset controlAsset, ControlInfo controlInfo)
        {
            ControlElement = controlAsset.GetElement("control");
            
            SetInformation(controlInfo);
            RegisterButtons();
        }

        private void SetInformation(ControlInfo controlInfo)
        {
            Label nameLabel = (Label) ControlElement.Get("name");
            Button keyButton = (Button) ControlElement.Get("change-key");
            
            nameLabel.Text = nameLabel.Text.Replace("%name%", controlInfo.Name);
            keyButton.Text = keyButton.Text.Replace("%key%", controlInfo.Key);
        }

        private void RegisterButtons()
        {
            Button keyButton = (Button) ControlElement.Get("change-key");
            keyButton.Subscribe(x =>
            {
                keyButton.Text = "Press Any Key";
                // wait for key press, make sure only one control key can be changed at a time
            });
        }
    }
}
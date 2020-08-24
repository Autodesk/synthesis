using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class ControlsPage
    {
        public VisualElement ControlsElement { get; }
        private VisualElementAsset ControlAsset;
        private ListView ControlList;

        public ControlsPage(VisualElementAsset controlsAsset)
        {
            ControlsElement = controlsAsset.GetElement("page");
            ControlsElement.SetStyleProperty("height", "100%");
            
            ControlAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Control.uxml");
            ControlList = (ListView) ControlsElement.Get("controls");

            LoadPageContent();
        }

        private void LoadPageContent()
        {
            // load from preferencemanager (?)
            AddControlToList("Camera Forward", "W");
            AddControlToList("Camera Backward", "S");
            AddControlToList("Camera Left", "A");
            AddControlToList("Camera Right", "D");
            AddControlToList("Camera Up", "Space");
            AddControlToList("Camera Down", "Left Shift");
            AddControlToList("Entity Forward", "Up Arrow");
            AddControlToList("Entity Backward", "Down Arrow");
            AddControlToList("Entity Left", "Left Arrow");
            AddControlToList("Entity Right", "Right Arrow");
        }

        private void LoadPreferences()
        {
            
        }

        private void AddControlToList(string name, string key)
        {
            ControlList.Add(new ControlItem(ControlAsset, new ControlInfo(name, key)).ControlElement);
        }

    }
}
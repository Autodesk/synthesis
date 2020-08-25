using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
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

            LoadPreferences();
            LoadPageContent();
        }

        private void LoadPageContent()
        {
            AddControlToList("Camera Forward", GetPreference("Camera Forward"));
            AddControlToList("Camera Backward", GetPreference("Camera Backward"));
            AddControlToList("Camera Left", GetPreference("Camera Left"));
            AddControlToList("Camera Right", GetPreference("Camera Right"));
            AddControlToList("Camera Up", GetPreference("Camera Up"));
            AddControlToList("Camera Down", GetPreference("Camera Down"));
            AddControlToList("Entity Forward", GetPreference("Entity Forward"));
            AddControlToList("Entity Backward", GetPreference("Entity Backward"));
            AddControlToList("Entity Left", GetPreference("Entity Left"));
            AddControlToList("Entity Right", GetPreference("Entity Right"));
        }

        private void LoadPreferences()
        {
            Dictionary<string, string> defaultControls = new Dictionary<string, string>
            {
                {"Camera Forward", "W"},
                {"Camera Backward", "S"},
                {"Camera Left", "A"},
                {"Camera Up", "Space"},
                {"Camera Down", "Left Shift"},
                {"Entity Forward", "Up Arrow"},
                {"Entity Backward", "Down Arrow"},
                {"Entity Left", "Left Arrow"},
                {"Entity Right", "Right Arrow"}
            };

            LoadOrSetDefaultPreference(defaultControls);
        }

        private void AddControlToList(string name, string key)
        {
            ControlList.Add(new ControlItem(ControlAsset, new ControlInfo(name, key)).ControlElement);
        }

        private void LoadOrSetDefaultPreference(Dictionary<string, string> defaultControls)
        {
            foreach (string controlName in defaultControls.Keys)
            {
                if (PreferenceManager.GetPreference("SynthesisCore", controlName) == null)
                {
                    PreferenceManager.SetPreference("SynthesisCore", controlName, defaultControls[controlName]);
                    PreferenceManager.Save();
                }
            }
        }

        private string GetPreference(string controlName)
        {
            var controlKey = PreferenceManager.GetPreference("SynthesisCore", controlName);
            if (controlKey is string)
            {
                return (string) controlKey;
            }
            return "Unassigned";
        }
    }
}
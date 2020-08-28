using SynthesisAPI.AssetManager;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class ControlsPage
    {
        public VisualElement Page { get; }
        private VisualElementAsset ControlAsset;
        private ListView ControlList;

        public ControlsPage(VisualElementAsset controlsAsset)
        {
            Page = controlsAsset.GetElement("page");
            Page.SetStyleProperty("height", "100%");
            
            ControlAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Control.uxml");
            ControlList = (ListView) Page.Get("controls");

            LoadPageContent();
        }

        private void LoadPageContent()
        {
            AddControl("Camera Forward");
            AddControl("Camera Backward");
            AddControl("Camera Left");
            AddControl("Camera Right");
            AddControl("Camera Up");
            AddControl("Camera Down");
            AddControl("Entity Forward");
            AddControl("Entity Backward");
            AddControl("Entity Left");
            AddControl("Entity Right");
        }

        private void AddControl(string controlName)
        {
            string key = GetFormattedPreference(controlName);
            ControlList.Add(new ControlItem(ControlAsset, new ControlInfo(controlName, key)).Element);
        }

        private string GetFormattedPreference(string controlName)
        {
            var controlKey = PreferenceManager.GetPreference("SynthesisCore", controlName);
            if (controlKey is string)
            {
                return Utilities.ReformatCondensedString((string) controlKey);
            }
            return "Unassigned";
        }
    }
}
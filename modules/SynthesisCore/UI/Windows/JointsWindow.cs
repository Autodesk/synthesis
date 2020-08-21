using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;

namespace SynthesisCore.UI
{
    public class JointsWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private VisualElementAsset JointAsset;
        private ListView JointList;

        public JointsWindow()
        {
            JointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joint.uxml");

            var jointsAssset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Panel = new Panel("Joints", jointsAssset, OnWindowOpen);
        }
        
        private void OnWindowOpen(VisualElement jointsWindow)
        {
            Window = jointsWindow;

            jointsWindow.SetStyleProperty("position", "absolute");
            jointsWindow.SetStyleProperty("left", "0");

            JointList = (ListView) Window.Get("joint-list");
            LoadWindowContents();   
            RegisterButtons();
        }

        private void LoadWindowContents()
        {
            foreach (var entity in SynthesisCoreData.ModelsDict.Values)
            {
                if (entity.GetComponent<Joints>() != null)
                {
                    foreach (var joint in entity.GetComponent<Joints>().AllJoints)
                    {
                        JointList.Add(new JointItem(JointAsset, joint).JointElement);
                    }
                }
            }
        }

        private void RegisterButtons()
        {        
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }
    }
}
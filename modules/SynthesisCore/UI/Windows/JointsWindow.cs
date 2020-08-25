using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.Simulation;

namespace SynthesisCore.UI
{
    public class JointsWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private VisualElementAsset JointAsset = null;
        private ListView JointList;

        internal static Entity? jointHighlightEntity = null;
        
        public JointsWindow()
        {
            if(JointAsset == null)
                JointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joint.uxml");

            var jointsAssset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Panel = new Panel("Joints", jointsAssset, OnWindowOpen);
        }
        
        private void OnWindowOpen(VisualElement jointsWindow)
        {
            Window = jointsWindow;

            Window.SetStyleProperty("position", "absolute");
            Window.IsDraggable = true;

            Window.SetStyleProperty("left", "0");
            Window.SetStyleProperty("top", "70");

            JointList = (ListView) Window.Get("joint-list");
            LoadWindowContents();   
            RegisterButtons();
        }

        private void OnWindowClose()
        {
            JointItem.UnHighlightAllButtons();
            if (jointHighlightEntity?.RemoveEntity() ?? false)
                jointHighlightEntity = null;
            UIManager.ClosePanel(Panel.Name);
        }

        private void LoadWindowContents()
        {
            foreach (var entity in SynthesisCoreData.ModelsDict.Values)
            {
                var motorAssemblyManager = entity.GetComponent<MotorAssemblyManager>();
                if (motorAssemblyManager != null)
                {
                    foreach (var assembly in motorAssemblyManager.AllMotorAssemblies)
                    {
                            JointList.Add(new JointItem(JointAsset, assembly).JointElement);
                    }
                }
            }
        }

        private void RegisterButtons()
        {        
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(_ => OnWindowClose());

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(_ => OnWindowClose());
        }
    }
}
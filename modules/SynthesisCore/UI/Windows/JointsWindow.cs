using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
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
        internal static Entity SelectedJointEntity;

        public JointsWindow()
        {
            if (JointAsset == null)
                JointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joint.uxml");

            var jointsAssset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Panel = new Panel("Joints", jointsAssset, OnWindowOpen, false);
        }

        public static void GetUpdatedJointList(Entity entity)
        {
            SelectedJointEntity = entity;
        }

        private void OnWindowOpen(VisualElement jointsWindow)
        {
            Window = jointsWindow;

            Window.SetStyleProperty("position", "absolute");
            Window.IsDraggable = true;

            Window.SetStyleProperty("left", "0");
            Window.SetStyleProperty("top", "70");

            JointList = (ListView)Window.Get("joint-list");
            LoadWindowContents();
            RegisterButtons();
        }

        private void OnWindowClose()
        {
            JointItem.UnHighlightAllButtons();
            if (jointHighlightEntity?.RemoveEntity() ?? false)
                jointHighlightEntity = null;
            UIManager.ClosePanel(Panel.Name);
            RemoveWindowsContents();
        }

        private void LoadWindowContents()
        {
            foreach (var entity in SynthesisCoreData.ModelsDict.Values)
            {
                var motorAssemblyManager = entity.GetComponent<MotorAssemblyManager>();
                if (MotorAssemblyManager.IsDescendant(SelectedJointEntity, entity))
                {
                    if (motorAssemblyManager != null)
                    {
                        foreach (var assembly in motorAssemblyManager.AllMotorAssemblies)
                        {
                            JointList.Add(new JointItem(JointAsset, assembly).JointElement);
                        }
                    }
                }
                else Logger.Log("No joints are associated with this entity.", LogLevel.Debug);
            }
        }

        private void RemoveWindowsContents()
        {
            foreach (var jointChild in JointList.GetChildren())
            {
                JointList.Remove(jointChild);
            }
        }

        private void RegisterButtons()
        {
            Button okButton = (Button)Window.Get("ok-button");
            okButton?.Subscribe(_ => OnWindowClose());

            Button closeButton = (Button)Window.Get("close-button");
            closeButton?.Subscribe(_ => OnWindowClose());
        }
    }
}
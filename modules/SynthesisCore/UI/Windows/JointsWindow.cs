using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Simulation;

namespace SynthesisCore.UI
{
    public static class JointsWindow
    {
        public static Panel Panel { get; private set; }
        private static VisualElement Window;
        private static VisualElementAsset JointAsset = null;
        private static VisualElementAsset NoJointsAsset = null;
        private static ListView JointList;

        internal static Entity? jointHighlightEntity = null;

        public static void CreateWindow()
        {
            if (JointAsset == null)
                JointAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joint.uxml");

            if (NoJointsAsset == null)
                NoJointsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/JointNone.uxml");

            var jointsAssset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Panel = new Panel("Joints", jointsAssset, OnWindowOpen, false);
        }

        private static void OnWindowOpen(VisualElement jointsWindow)
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

        public static void OnWindowClose()
        {
            JointItem.UnHighlightAllButtons();
            if (jointHighlightEntity?.RemoveEntity() ?? false)
                jointHighlightEntity = null;
            UIManager.ClosePanel(Panel.Name);
            RemoveWindowsContents();
        }

        private static void LoadWindowContents()
        {
            if (Selectable.Selected != null)
            {
                foreach (var entity in SynthesisCoreData.ModelsDict.Values)
                {
                    var motorAssemblyManager = entity.GetComponent<MotorAssemblyManager>();
                    if (MotorAssemblyManager.IsDescendant(Selectable.Selected.Entity.Value, entity))
                    {
                        if (motorAssemblyManager != null)
                        {
                            foreach (var assembly in motorAssemblyManager.AllMotorAssemblies)
                            {
                                JointList.Add(new JointItem(JointAsset, assembly).JointElement);
                            }
                        }
                    }
                    else
                    {
                        JointList.Add(new JointItem(NoJointsAsset).JointElement);
                        Logger.Log("No joints are associated with this entity.", LogLevel.Debug);
                    }
                }
            }
        }

        private static void RemoveWindowsContents()
        {
            foreach (var jointChild in JointList.GetChildren())
            {
                JointList.Remove(jointChild);
            }
        }

        private static void RegisterButtons()
        {
            Button okButton = (Button)Window.Get("ok-button");
            okButton?.Subscribe(_ => OnWindowClose());

            Button closeButton = (Button)Window.Get("close-button");
            closeButton?.Subscribe(_ => OnWindowClose());
        }
    }
}
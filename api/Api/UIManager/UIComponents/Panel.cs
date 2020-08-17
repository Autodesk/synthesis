using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public struct Panel
    {
        public string Name { get; private set; }
        public VisualElementAsset Ui { get; private set; }
        public Action<VisualElement> BindPanel { get; set; }
        internal VisualElement PanelElement;

        public Panel(string name, VisualElementAsset ui, Action<VisualElement> bindPanel)
        {
            Name = name;
            Ui = ui;
            BindPanel = bindPanel;
            PanelElement = null;
        }

    }
}
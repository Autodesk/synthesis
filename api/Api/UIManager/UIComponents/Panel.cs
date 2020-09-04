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
        public bool CachePanel { get; set; }
        internal VisualElement PanelElement;

        public Panel(string name, VisualElementAsset ui, Action<VisualElement> bindPanel, bool cachePanel = true)
        {
            Name = name;
            Ui = ui;
            BindPanel = bindPanel;
            CachePanel = cachePanel;
            PanelElement = null;
        }

    }
}
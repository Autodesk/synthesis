using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public struct Tab
    {
        public string Name { get; private set; }
        public SynVisualElementAsset Ui { get; private set; }
        public Action<SynVisualElement> BindFunc { get; set; }

        public Tab(string name, SynVisualElementAsset ui, Action<SynVisualElement> bindFunc)
        {
            Name = name;
            Ui = ui;
            BindFunc = bindFunc;
        }
    }
}
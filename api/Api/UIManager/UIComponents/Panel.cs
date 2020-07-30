using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public struct Panel
    {
        public string Name { get; private set; }
        public VisualElementAsset Ui { get; private set; }
        public Action<VisualElement> BindFunc { get; set; }

        public Panel(string name, VisualElementAsset ui, Action<VisualElement> bindFunc)
        {
            Name = name;
            Ui = ui;
            BindFunc = bindFunc;
        }

    }
}
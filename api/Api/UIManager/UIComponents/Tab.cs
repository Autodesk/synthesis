using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public struct Tab
    {
        public string Name { get; private set; }
        public VisualElementAsset ToobarAsset { get; private set; }
        public BindToolbarDelegate BindToolbar { get; set; }
        internal VisualElement ToolbarElement;
        internal Button buttonElement;

        public delegate void BindToolbarDelegate(VisualElement toolbarElement);

        public Tab(string name, VisualElementAsset toolbarAsset, BindToolbarDelegate bindToolbar)
        {
            Name = name;
            BindToolbar = bindToolbar;
            ToobarAsset = toolbarAsset;
            ToolbarElement = null;
            buttonElement = null;
        }
    }
}
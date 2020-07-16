using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public abstract class Tab
    {
        public string Name { get; protected set; }
        public SynVisualElementAsset UI { get; protected set; }

        public Tab(string name, SynVisualElementAsset ui)
        {
            Name = name;
            UI = ui;
        }

        public abstract void Bind(SynVisualElement element);
    }
}
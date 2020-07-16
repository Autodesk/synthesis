using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisAPI.UIManager.UIComponents
{
    public abstract class Panel
    {
        public string Name { get; protected set; }
        public SynVisualElementAsset UI { get; protected set; }

        public Panel(string name, SynVisualElementAsset ui)
        {
            Name = name;
            UI = ui;
        }

        public abstract void Bind(SynVisualElement element);
    }
}
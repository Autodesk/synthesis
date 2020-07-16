using SynthesisAPI.Modules.Attributes;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
    public class Selectable : Component
    {
        public bool IsSelected { get; private set; }

        internal void SetSelected(bool selected)
        {
            IsSelected = selected;
        }
    }
}

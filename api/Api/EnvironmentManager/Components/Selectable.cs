using SynthesisAPI.Modules.Attributes;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
    public class Selectable : Component
    {
        public static Selectable? Selected { get; private set; } = null;

        public bool IsSelected { get; private set; }

        internal void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (IsSelected)
            {
                Selected = this;
            }
        }
    }
}

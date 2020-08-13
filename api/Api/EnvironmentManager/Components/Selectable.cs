using SynthesisAPI.Modules.Attributes;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    public class Selectable : Component
    {
        public static Selectable? Selected { get; private set; } = null;

        public bool IsSelected { get; private set; }

        public delegate void EventDelegate();

        public EventDelegate OnSelect;
        public EventDelegate OnDeselect;

        public Selectable()
        {
            IsSelected = false;
            OnSelect = () => { };
            OnDeselect = () => { };
        }

        internal void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (IsSelected)
            {
                Selected = this;
            }
        }

        internal static void ResetSelected()
        {
            Selected = null;
        }
    }
}

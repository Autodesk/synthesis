using SynthesisAPI.Modules.Attributes;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    /// <summary>
    /// Makes an entity mouse-selectable if it has a collider. If it does not have a collider, then
    /// it uses its children's colliders to determine if it has been selected.
    /// </summary>
    public class Selectable : Component
    {
        public static Selectable? Selected { get; private set; } = null;

        public bool IsSelected { get; private set; }
        /// <summary>
        /// If the selectable component is using its entity's collider or its' entity's childrens' colliders
        /// </summary>
        public bool UsingChildren { get; internal set; }

        public delegate void EventDelegate();

        public EventDelegate OnSelect;
        public EventDelegate OnDeselect;

        public Selectable()
        {
            IsSelected = false;
            UsingChildren = false;
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

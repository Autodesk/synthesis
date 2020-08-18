#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    /// <summary>
    /// Makes an entity mouse-selectable if it has a collider. If it does not have a collider, then
    /// it uses its children's colliders to determine if it has been selected.
    /// </summary>
    public class Selectable : Component
    {
        /// <summary>
        /// Unselected = Not clicked
        /// Selected = Single clicked
        /// ExtendedSelectionPending = Single clicked but may become double click
        /// ExtendedSelection = Double clicked
        /// </summary>
        public enum SelectionType
        {
            Unselected,
            Selected,
            ExtendedSelectionPending,
            ExtendedSelection
        }

        public SelectionType State { get; private set; }

        public static Selectable? Selected { get; private set; } = null;

        public bool IsSelected => State != SelectionType.Unselected;
        /// <summary>
        /// If the selectable component is using its entity's collider or its' entity's childrens' colliders
        /// </summary>
        public bool UsingChildren { get; internal set; }

        public delegate void EventDelegate();

        public EventDelegate OnSelect;
        public EventDelegate OnDeselect;

        public Selectable()
        {
            UsingChildren = false;
            State = Selectable.SelectionType.Unselected;
            OnSelect = () => { };
            OnDeselect = () => { };
        }

        internal void SetSelected(SelectionType state)
        {
            State = state;
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

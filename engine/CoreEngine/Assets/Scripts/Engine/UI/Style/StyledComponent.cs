using System;
using UnityEngine;

namespace Synthesis.UI.Style
{
    /// <summary>
    /// Used to create styled types (text, panels, etc.)
    /// </summary>
    public abstract class StyledComponent : MonoBehaviour
    {
        private void Awake()
        {
            OnAwake();
            if (StyleHandler.selectedStyle != null) ProcessStyle(); // if the style has been selected
            StyleHandler.ConveyStyle += ProcessStyle; // Attaches the ProcessStlye function to the event
        }

        protected virtual void OnAwake() { }

        public abstract void ProcessStyle();
    }
}

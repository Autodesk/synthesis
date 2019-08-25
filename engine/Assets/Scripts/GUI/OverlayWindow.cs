using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synthesis.GUI
{
    /// <summary>
    /// Generic interface representing an overlay shown by <see cref="GUIController"/>.
    /// </summary>
    interface OverlayWindow
    {
        /// <summary>
        /// Is this overlay currently visible.
        /// </summary>
        bool Active
        {
            get;
            set;
        }

        /// <summary>
        /// Action called when the overlay is closed.
        /// </summary>
        event Action<object> OnComplete;

        /// <summary>
        /// Renders this overlay
        /// </summary>
        void Render();

        /// <summary>
        /// Gets the rect.
        /// </summary>
        /// <returns>The rect.</returns>
        Rect GetWindowRect();
    }
}

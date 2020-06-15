using System;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI.Style
{
    /// <summary>
    /// Used for styling panel objects
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PanelStyle : StyledComponent
    {
        /// <summary>
        /// A enum for devs to choose which color to use
        /// </summary>
        public enum ColorToUse { Primary, PrimaryAccent, Secondary, SecondaryAccent }
        public ColorToUse WhichColorToUse = ColorToUse.Primary;

        private Image img;

        protected override void OnAwake()
        {
            // Gets the image component and stores it for when the style is loaded
            img = gameObject.GetComponent<Image>();
        }

        /// <summary>
        /// This takes the selected style and 
        /// </summary>
        public override void ProcessStyle()
        {
            // Check and see if a style has been selected
            Style s = StyleHandler.selectedStyle;
            if (s == null)
            {
                Debug.Log("Tried to process a style without a style selected. You must select a style using Styler.SelectStyle(Style)");
                return;
            }

            switch (WhichColorToUse)
            {
                case ColorToUse.Primary:
                    img.color = s.UiColor.Primary;
                    break;
                case ColorToUse.PrimaryAccent:
                    img.color = s.UiColor.PrimaryAccent;
                    break;
                case ColorToUse.Secondary:
                    img.color = s.UiColor.Secondary;
                    break;
                case ColorToUse.SecondaryAccent:
                    img.color = s.UiColor.SecondaryAccent;
                    break;
            }
        }
    }
}

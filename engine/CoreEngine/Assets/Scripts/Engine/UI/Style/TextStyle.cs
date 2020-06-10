using System;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI.Style
{
    /// <summary>
    /// Used for styling text objects
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextStyle : StyledComponent
    {
        private Text txt;

        protected override void OnAwake()
        {
            txt = GetComponent<Text>();
        }

        public override void ProcessStyle()
        {
            Style s = StyleHandler.selectedStyle;
            if (s == null)
            {
                Debug.Log("Tried to process a style without a style selected. You must select a style using Styler.SelectStyle(Style)");
                return;
            }

            txt.color = s.UiColor.TextColor;
        }
    }
}

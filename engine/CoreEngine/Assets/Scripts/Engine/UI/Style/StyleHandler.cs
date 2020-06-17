using System;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.Util;

namespace Synthesis.UI.Style
{
    /// <summary>
    /// Used for handling all the style components and which style to use
    /// </summary>
    public static class StyleHandler
    {
        /// <summary>
        /// An event that is made to load a <see cref="Style"/> after <see cref="Styler"/> components have loaded
        /// </summary>
        public delegate void Call();
        public static event Call ConveyStyle = () => { };
        public static Style selectedStyle { get; private set; }

        /// <summary>
        /// Selects the style. <see cref="selectedStyle"/> should not be access outside of this class
        /// </summary>
        /// <param name="style">The <see cref="Style"/> to use</param>
        public static void SelectStyle(Style style)
        {
            // Stores style
            selectedStyle = style;

            // Calls event to process the style in all registered Stylers
            ConveyStyle();

            // Update environment
            UnityHandler.Instance.PlaneMeshRenderer.material.color = selectedStyle.EnvColor.GridFill;
            
            // Debug.Log(RenderSettings.skybox. );

            // m.color = selectedStyle.EnvColor.Skybox;
            // Debug.Log(UnityHandles.Instance.SkyboxMaterial.color.ToString());
            RenderSettings.skybox.SetColor("_Tint", selectedStyle.EnvColor.Skybox);
            DynamicGI.UpdateEnvironment();
        }

    }

}
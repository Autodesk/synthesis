using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Configuration
{
    public class SelectableArrow : MonoBehaviour
    {
        private const float HiddenAlpha = 0.25f;

        private ArrowType arrowType;
        private Material material;
        private Color color;
        private bool selectable;

        /// <summary>
        /// Initializes the <see cref="ArrowType"/> and saves the assigned
        /// <see cref="Material"/>.
        /// </summary>
        private void Start()
        {
            if (!Enum.TryParse(name, out arrowType))
                arrowType = ArrowType.None;

            material = GetComponent<Renderer>().material;
            color = material.color;
            selectable = true;
        }

        /// <summary>
        /// Sends a message upwards when this <see cref="SelectableArrow"/>
        /// is selected.
        /// </summary>
        private void OnMouseDown()
        {
            SendMessageUpwards("OnArrowSelected", arrowType);
        }

        /// <summary>
        /// Sends a message upwards when this <see cref="SelectableArrow"/>
        /// is released.
        /// </summary>
        private void OnMouseUp()
        {
            SendMessageUpwards("OnArrowReleased");
            material.color = color;
        }

        /// <summary>
        /// Highlights the arrow yellow when it is hovered over.
        /// </summary>
        private void OnMouseEnter()
        {
            CameraController.isOverGizmo = true;
            if (selectable)
                material.color = Color.Lerp(color, Color.yellow, 0.75f);
        }

        /// <summary>
        /// Returns the arrow to its original color when the mouse
        /// is no longer hovering over it.
        /// </summary>
        private void OnMouseExit()
        {
            CameraController.isOverGizmo = false;
            if (selectable)
                material.color = color;
        }

        /// <summary>
        /// Sets the alpha of this <see cref="SelectableArrow"/> according to the
        /// <see cref="ArrowType"/> provided.
        /// </summary>
        /// <param name="activeArrow"></param>
        private void SetActiveArrow(ArrowType activeArrow)
        {
            if (selectable = (activeArrow == ArrowType.None))
            {
                material.color = color;
            }
            else
            {
                Color newColor = color;
                newColor.a = arrowType == activeArrow ? 1f : color.a * HiddenAlpha;
                material.color = newColor;
            }
        }
    }
}
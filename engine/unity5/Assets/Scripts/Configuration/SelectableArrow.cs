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
            color.a = 1f;
            material.color = color;
        }

        /// <summary>
        /// Sets the alpha of this <see cref="SelectableArrow"/> according to the
        /// <see cref="ArrowType"/> provided.
        /// </summary>
        /// <param name="activeArrow"></param>
        private void SetActiveArrow(ArrowType activeArrow)
        {
            color.a = activeArrow == ArrowType.None || arrowType == activeArrow ? 1f : HiddenAlpha;
            material.color = color;
        }
    }
}

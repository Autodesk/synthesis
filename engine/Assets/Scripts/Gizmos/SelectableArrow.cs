using System;
using UnityEngine;

namespace Synthesis.Configuration {
    public class SelectableArrow : MonoBehaviour {
        private const float HiddenAlpha = 0.25f;

        private ArrowType arrowType;
        private Material material;
        private Color color;
        private bool selectable;

        private Quaternion startRotation;

        /// <summary>
        /// Initializes the <see cref="ArrowType"/> and saves the assigned
        /// <see cref="Material"/>.
        /// </summary>
        private void Start() {
            if (!Enum.TryParse(name, out arrowType))
                arrowType = ArrowType.None;

            material   = GetComponent<Renderer>().material;
            color      = material.color;
            selectable = true;
        }
        private void LateUpdate() {
            // Keeps marker looking at the camera
            if (arrowType == ArrowType.P) {
                transform.LookAt(Camera.main.transform.position);
            } else if (arrowType <= ArrowType.Z) {
                // Keeps axis arrows looking at the camera
                transform.RotateAround(transform.position, transform.up,
                    CalcSignedCentralAngle(transform.forward,
                        Vector3.Normalize(Camera.main.transform.position - transform.position), transform.up) *
                        Mathf.Rad2Deg);
            }
        }

        // Calculates signed angle projected to a plane
        private float CalcSignedCentralAngle(Vector3 dir1, Vector3 dir2, Vector3 normal) => Mathf.Atan2(
            Vector3.Dot(Vector3.Cross(dir1, dir2), normal), Vector3.Dot(dir1, dir2));

        /// <summary>
        /// Sends a message upwards when this <see cref="SelectableArrow"/>
        /// is selected.
        /// </summary>
        public void OnMouseDown() {
            SendMessageUpwards("OnArrowSelected", arrowType);
        }

        /// <summary>
        /// Sends a message upwards when this <see cref="SelectableArrow"/>
        /// is released.
        /// </summary>
        public void OnMouseUp() {
            SendMessageUpwards("OnArrowReleased");
            material.color = color;
        }

        /// <summary>
        /// Highlights the arrow yellow when it is hovered over.
        /// </summary>
        public void OnMouseEnter() {
            CameraController.isOverGizmo = true;
            if (selectable) {
                material.color = Color.Lerp(color, new Color(30.0f / 255.0f, 164f / 255f, 212f / 255f, 1), 0.75f);
            }
        }

        /// <summary>
        /// Returns the arrow to its original color when the mouse
        /// is no longer hovering over it.
        /// </summary>
        public void OnMouseExit() {
            CameraController.isOverGizmo = false;
            if (selectable) {
                material.color = color;
            }
        }

        /// <summary>
        /// Sets the alpha of this <see cref="SelectableArrow"/> according to the
        /// <see cref="ArrowType"/> provided.
        /// </summary>
        /// <param name="activeArrow"></param>
        private void SetActiveArrow(ArrowType activeArrow) {
            if (selectable = (activeArrow == ArrowType.None)) {
                material.color = color;
            } else {
                Color newColor = material.color;
                newColor.a     = arrowType == activeArrow ? 1f : color.a * HiddenAlpha;
                material.color = newColor;
            }
        }
    }
}

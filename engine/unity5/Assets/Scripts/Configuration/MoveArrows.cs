using BulletUnity;
using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Configuration
{
    public class MoveArrows : MonoBehaviour
    {
        private const float Scale = 0.075f;
        private Vector3 initialScale;
        private Vector3 lastArrowPoint;
        private ArrowType activeArrow;

        /// <summary>
        /// Gets or sets the active selected arrow. When <see cref="ActiveArrow"/>
        /// is changed, the "SetActiveArrow" message is broadcasted to all
        /// <see cref="SelectableArrow"/>s.
        /// </summary>
        private ArrowType ActiveArrow
        {
            get
            {
                return activeArrow;
            }
            set
            {
                activeArrow = value;
                BroadcastMessage("SetActiveArrow", activeArrow);
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> representing the direction the selected
        /// arrow is facing, or <see cref="Vector3.zero"/> if no arrow is selected.
        /// </summary>
        private Vector3 ArrowDirection
        {
            get
            {
                switch (ActiveArrow)
                {
                    case ArrowType.X:
                    case ArrowType.YZ:
                        return transform.right;
                    case ArrowType.Y:
                    case ArrowType.XZ:
                        return transform.up;
                    case ArrowType.Z:
                    case ArrowType.XY:
                        return transform.forward;
                    default:
                        return Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Called when the arrows are dragged.
        /// The input parameter is the position delta of the <see cref="MoveArrows"/>.
        /// </summary>
        public Action<Vector3> Translate { get; set; }

        /// <summary>
        /// Sets the initial position and rotation.
        /// </summary>
        private void Start()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            initialScale = transform.localScale;
        }

        /// <summary>
        /// Updates the robot's position when the arrows are dragged.
        /// </summary>
        private void Update()
        {
            if (activeArrow == ArrowType.None)
                return;

            Ray mouseRay = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            Vector3 currentArrowPoint;

            if (activeArrow <= ArrowType.Z)
            {
                Vector3 closestPointScreenRay;
                Auxiliary.ClosestPointsOnTwoLines(out closestPointScreenRay, out currentArrowPoint,
                    mouseRay.origin, mouseRay.direction, transform.position, ArrowDirection);
            }
            else
            {
                Plane plane = new Plane(ArrowDirection, transform.position);

                float enter;
                plane.Raycast(mouseRay, out enter);

                currentArrowPoint = mouseRay.GetPoint(enter);
            }

            if (lastArrowPoint != Vector3.zero)
                Translate?.Invoke(currentArrowPoint - lastArrowPoint);

            lastArrowPoint = currentArrowPoint;
        }

        /// <summary>
        /// Scales the arrows to maintain a constant size relative to screen coordinates.
        /// </summary>
        private void LateUpdate()
        {
            Plane plane = new Plane(UnityEngine.Camera.main.transform.forward, UnityEngine.Camera.main.transform.position);
            float dist = plane.GetDistanceToPoint(transform.position);
            transform.localScale = initialScale * Scale * dist;
        }

        /// <summary>
        /// Sets the active arrow when a <see cref="SelectableArrow"/> is selected.
        /// </summary>
        /// <param name="arrowType"></param>
        private void OnArrowSelected(ArrowType arrowType)
        {
            ActiveArrow = arrowType;
            DynamicCamera.MovementEnabled = false;
            lastArrowPoint = Vector3.zero;
        }

        /// <summary>
        /// Sets the active arrow to <see cref="ArrowType.None"/> when a
        /// <see cref="SelectableArrow"/> is released.
        /// </summary>
        private void OnArrowReleased()
        {
            ActiveArrow = ArrowType.None;
            DynamicCamera.MovementEnabled = true;
        }
    }
}

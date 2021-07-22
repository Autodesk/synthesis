//using BulletUnity;
//using Synthesis.FSM;
//using Synthesis.States;
//using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace Synthesis.Configuration
{
    public class MoveArrow : MonoBehaviour
    {
        private const float Scale = 0.075f;
        private Vector3 initialScale;
        private Vector3 lastArrowPoint;
        private ArrowType activeArrow;
        private bool bufferPassed;
        private bool snapEnabled;

        private Plane axisPlane;
        private Plane markerPlane;

        private Transform parent;


        [SerializeField, Range(1f, 30.0f)] public float snapRotationToDegree;
        [SerializeField, Range(0.1f, 2f)] public float snapTransformToUnit;


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
                    case ArrowType.RX:
                        return transform.right;
                    case ArrowType.Y:
                    case ArrowType.XZ:
                    case ArrowType.RY:
                        return transform.up;
                    case ArrowType.Z:
                    case ArrowType.XY:
                    case ArrowType.RZ:
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
        /// Called when an arrow is first clicked.
        /// </summary>
        public Action OnClick { get; set; }

        /// <summary>
        /// Called when an arrow is released.
        /// </summary>
        public Action OnRelease { get; set; }


        /// <summary>
        /// Sets the initial position and rotation.
        /// </summary>
        private void Awake()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            initialScale = new Vector3(transform.localScale.x / transform.lossyScale.x,
                transform.localScale.y / transform.lossyScale.y, transform.localScale.z / transform.lossyScale.z);
        }

        /// <summary>
        /// Enables all colliders of any parent objects to allow for their own click detection. 
        /// </summary>
        private void OnBeforeTransformParentChanged()
        {
            SetOtherCollidersEnabled(true);
        }

        /// <summary>
        /// Disables all colliders of any parent objects to allow for proper click detection.
        /// </summary>
        private void OnTransformParentChanged()
        {
            SetOtherCollidersEnabled(false);
        }

        /// <summary>
        /// Disables all colliders of any parent objects to allow for proper click detection.
        /// </summary>
        private void OnEnable()
        {
            SetOtherCollidersEnabled(false);
        }

        /// <summary>
        /// Re-enables all colliders of any parent objects to allow for their own click detection.
        /// </summary>
        private void OnDisable()
        {
            SetOtherCollidersEnabled(true);
        }

        /// <summary>
        /// Updates the robot's position when the arrows are dragged.
        /// </summary>
        private void Update()
        {            
            if (Input.GetKeyDown(KeyCode.R))//Reset
            {
                parent.rotation = Quaternion.identity;
            }
            if(Input.GetKey(KeyCode.LeftControl)|| Input.GetKey(KeyCode.RightControl))
            {
                snapEnabled = true;
            }
            else
            {
                snapEnabled = false;
            }
            if (activeArrow == ArrowType.None)
                return;

            // This allows for any updates from OnClick to complete before translation starts
            if (!bufferPassed)
            {
                bufferPassed = true;
                return;
            }

            if (activeArrow == ArrowType.P)
            {
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayLength;
                //if mouse ray doesn't intersect plane, set default ray length
                if (!markerPlane.Raycast(cameraRay, out rayLength)) rayLength = Vector3.Distance(Camera.main.transform.position, parent.position);
                parent.position = markerPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));
            }
            else if (activeArrow <= ArrowType.XY)
            {
                Ray mouseRay = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                Vector3 currentArrowPoint;

                if (activeArrow <= ArrowType.Z)
                {
                    Vector3 closestPointScreenRay;
                    ClosestPointsOnTwoLines(out closestPointScreenRay, out currentArrowPoint,
                     mouseRay.origin, mouseRay.direction, transform.position, ArrowDirection);

                }//TO DO: SET LIMITS
                else
                {
                    Plane plane = new Plane(ArrowDirection, transform.position);

                    float enter;
                    plane.Raycast(mouseRay, out enter);

                    currentArrowPoint = mouseRay.GetPoint(enter);
                }
                //snapping

                //FIX THIS SCRIPT: its bad; use it to set limits
                //prevents move arrows from going below field 
                if (GameObject.Find("Plane") != null)
                {
                    if (currentArrowPoint.y < GameObject.Find("Plane").transform.position.y) 
                    { 
                        currentArrowPoint.y = GameObject.Find("Plane").transform.position.y; lastArrowPoint.y = GameObject.Find("Plane").transform.position.y; //FIX
                    }
                }

                if (lastArrowPoint != Vector3.zero)
                {
                    if(snapEnabled && activeArrow <= ArrowType.Z)//snaps to configurable amount when control is held down. does this by settings current arrow point to rounded distance
                    currentArrowPoint = LerpByDistance(lastArrowPoint, currentArrowPoint,
                        RoundTo(Vector3.Distance(lastArrowPoint, currentArrowPoint), snapTransformToUnit));

                    parent.position += currentArrowPoint - lastArrowPoint;
                }

                lastArrowPoint = currentArrowPoint;

            }
            else
            {
                //Project a ray from mouse
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);                
                float rayLength;

                //if mouse ray doesn't intersect plane, set default ray length
                if (!axisPlane.Raycast(cameraRay, out rayLength)) rayLength = Vector3.Distance(Camera.main.transform.position, parent.position)*10;

                //get intersection point; if none, find closest point to default length
                Vector3 pointToLook = axisPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));

                //Correct parent's forward depending on rotation axis. Y-axis does not need corrections
                Vector3 t;
                if (ActiveArrow == ArrowType.RZ) t = parent.right;
                else if (ActiveArrow == ArrowType.RX) t = parent.up;
                else t = parent.forward;
                parent.RotateAround(parent.position, axisPlane.normal, //defines point and axis plane
                    -1 * RoundTo(Vector3.SignedAngle(pointToLook - parent.position, t, axisPlane.normal), //rounds degrees of rotation axis forward to mouse ray intersection
                    snapEnabled?snapRotationToDegree:0f)); //if control is pressed, snap to configurable value, otherwise, don't snap
            }

            transform.parent = parent;

        }

        /// <summary>
        /// Scales the arrows to maintain a constant size relative to screen coordinates.
        /// </summary>
        private void LateUpdate()
        {
            Plane plane = new Plane(Camera.main.transform.forward, Camera.main.transform.position);
            float dist = plane.GetDistanceToPoint(transform.position);
            transform.localScale = initialScale * Scale * dist;
            Vector3 scaleTmp = gameObject.transform.localScale;
            scaleTmp.x /= parent.localScale.x;
            scaleTmp.y /= parent.localScale.y;
            scaleTmp.z /= parent.localScale.z;
            gameObject.transform.localScale = scaleTmp;

        }
        void DrawPlane(Vector3 normal, Vector3 position)//for debug only, can be removed
        {

            Vector3 v3;

            if (normal.normalized != Vector3.forward)
                v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude*2;
            else
                v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude*2 ;

            var corner0 = position + v3;
            var corner2 = position - v3;
            var q = Quaternion.AngleAxis(90.0f, normal);
            v3 = q * v3;
            var corner1 = position + v3;
            var corner3 = position - v3;

            Debug.DrawLine(corner0, corner2, Color.green);
            Debug.DrawLine(corner1, corner3, Color.green);
            Debug.DrawLine(corner0, corner1, Color.green);
            Debug.DrawLine(corner1, corner2, Color.green);
            Debug.DrawLine(corner2, corner3, Color.green);
            Debug.DrawLine(corner3, corner0, Color.green);
            Debug.DrawRay(position, normal, Color.red);
        }

        /// <summary>
        /// Sets the active arrow when a <see cref="SelectableArrow"/> is selected.
        /// </summary>
        /// <param name="arrowType"></param>
        private void OnArrowSelected(ArrowType arrowType)
        {
            ActiveArrow = arrowType;
            lastArrowPoint = Vector3.zero;
            bufferPassed = false;

            if(arrowType==ArrowType.P)
                markerPlane = new Plane(Vector3.Normalize(Camera.main.transform.forward), parent.position);
            else if (arrowType >= ArrowType.RX)
                axisPlane = new Plane(ArrowDirection, parent.position);

            OnClick?.Invoke();
        }

        /// <summary>
        /// Sets the active arrow to <see cref="ArrowType.None"/> when a
        /// <see cref="SelectableArrow"/> is released.
        /// </summary>
        private void OnArrowReleased()
        {
            ActiveArrow = ArrowType.None;

            OnRelease?.Invoke();
        }

        /// <summary>
        /// Enables or disables other colliders to ensure proper arrow click
        /// detection.
        /// </summary>
        /// <param name="enabled"></param>
        private void SetOtherCollidersEnabled(bool enabled)//CLEAN THIS UP
        {
            parent = transform.parent;

            foreach (Collider c in GetComponentsInParent<Collider>(true))
                c.enabled = enabled;
            foreach (Rigidbody r in GetComponentsInParent<Rigidbody>(true))
                r.isKinematic = !enabled;

            if (transform.parent == null)
                return;

            foreach (Transform child in transform.parent)
            {
                if (child == transform)
                    continue;

                foreach (Collider c in child.GetComponentsInChildren<Collider>(true))
                    c.enabled = enabled;
                foreach (Rigidbody r in child.GetComponentsInChildren<Rigidbody>(true))
                    r.isKinematic = !enabled;

            }
        }


        /// <summary>
        /// Based on a solution provided by the Unity Wiki (http://wiki.unity3d.com/index.php/3d_Math_functions).
        /// Finds the closest points on two lines.
        /// </summary>
        /// <param name="closestPointLine1"></param>
        /// <param name="closestPointLine2"></param>
        /// <param name="linePoint1"></param>
        /// <param name="lineVec1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="lineVec2"></param>
        /// <returns></returns>
        private bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            // Check if lines are parallel
            if (d == 0.0f)
                return false;

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }
        float RoundTo(float value, float multipleOf)//used for snapping the gizmo to the nearest value
        {
            if (multipleOf != 0)
                return Mathf.Round(value / multipleOf) * multipleOf;
            else
                return value;
        }
        /// <summary>
        /// Finds the Vector3 point a distance of x away from Point A and on line AB
        /// </summary>
        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float x) // for snapping transformations
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }


    }
}
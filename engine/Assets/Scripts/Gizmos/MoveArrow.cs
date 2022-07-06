using System;
using System.Collections.Generic;
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
        private Func<Vector3> originalCameraFocusPoint;
        private Transform gizmoCameraTransform;
        Dictionary<Rigidbody, bool> rigidbodiesKinematicStateInScene;

        private CameraController cam;
        private float originalLowerPitch;
        private float gizmoPitch = -80f;

        [SerializeField, Range(1f, 30.0f)] public float snapRotationToDegree; //configurable
        [SerializeField, Range(0.1f, 2f)] public float snapTransformToUnit;
        private float floorBound = 0f;
        private float bounds = 50f;
        private float singleMoveLimitScale = 20f; //limits a single movement boundry to this number times the distance from camera to position
        private float startRotation;


        private Transform axisArrowTransform;
        private Transform arrowX;
        private Transform arrowY;
        private Transform arrowZ;

        //To use: move gizmo as a child of the object you want to move in game
        //Press R to reset rotation
        //Press CTRL to snap to nearest configured multiple when moving
        //Added gameObjects to Game while this script is active will not have their rigidbodies disabled


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

        private void Awake()
        {
            cam = Camera.main.GetComponent<CameraController>();
            originalLowerPitch = cam.PitchLowerLimit;
            originalCameraFocusPoint = cam.FocusPoint;

            //makes a list of the rigidbodies in the hierarchy and their state
            HierarchyRigidbodiesToDictionary();

            //configure axis arrow transforms for later use
            arrowX = transform.Find("X").GetComponent<Transform>();
            arrowY = transform.Find("Y").GetComponent<Transform>();
            arrowZ = transform.Find("Z").GetComponent<Transform>();

            initialScale = new Vector3(transform.localScale.x / transform.lossyScale.x,
                transform.localScale.y / transform.lossyScale.y, transform.localScale.z / transform.lossyScale.z);
        }
        private void setTransform() //called to set certain value when activated or when the parent changes
        {
            SetRigidbodies(false);

            parent = transform.parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            gizmoCameraTransform = new GameObject().transform;
            gizmoCameraTransform.position = transform.parent.position; //camera shifting
            cam.FocusPoint = () => gizmoCameraTransform.position;//camera focus
            cam.PitchLowerLimit = gizmoPitch; //camera pitch limits
        }
        private void disableGizmo() //makes sure values are set correctly when the gizmo is removed
        {
            cam.PitchLowerLimit = originalLowerPitch;
            cam.FocusPoint = originalCameraFocusPoint;
            SetRigidbodies(true);
        }

        private void OnTransformParentChanged()//only called for testing for changing parent transforms
        {
            if (transform.parent != null)
            {
                setTransform();
            }
        }
        private void OnEnable()
        {
            setTransform();
        }
        private void OnDisable()
        {
            disableGizmo();
        }
        private void OnDestroy()
        {
            disableGizmo();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GizmoManager.ExitGizmo();
            }
            if (Input.GetKeyDown(KeyCode.R))//Reset on press R
            {
                parent.rotation = Quaternion.identity;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) //enable snap on crtl press
            {
                snapEnabled = true;
            }
            else
            {
                snapEnabled = false;
            }
            if (Input.GetKey(KeyCode.Return))
            {
                GizmoManager.OnEnter();
            }
            if (activeArrow == ArrowType.None) // skip if there no gizmo components being dragged
                return;

            // This allows for any updates from OnClick to complete before translation starts
            if (!bufferPassed)
            {
                bufferPassed = true;
                return;
            }

            if (activeArrow == ArrowType.P) //for marker point: allows for drag on a plane directly facing camera
            {
                //draws a ray from mouse to plane directly facing the camera and moves parent to that position

                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                float rayLength;
                markerPlane.Raycast(cameraRay, out rayLength); //intersection between mouseray and plane
                parent.position = cameraRay.GetPoint(rayLength); //finds point on ray

                if (parent.position.y < 0) //limits y axis movements
                    parent.position = new Vector3(parent.position.x, 0, parent.position.z);
            }
            else if (activeArrow <= ArrowType.XY) //plane and axis arrows movements
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 currentArrowPoint;

                if (activeArrow <= ArrowType.Z)
                {
                    //draws plane with the same normal as the axis arrow, which faces the camera
                    Plane axisArrowPlane = new Plane(axisArrowTransform.forward, parent.position);

                    //a ray from the mouse is drawn to intersect the plane
                    float rayDistance;
                    float rayLimit = Vector3.Distance(Camera.main.transform.position, gizmoCameraTransform.position) * singleMoveLimitScale;
                    if (!axisArrowPlane.Raycast(mouseRay, out rayDistance) || rayDistance > rayLimit) //limits the maximum distance the ray can be to prevent moving it to infinity
                        rayDistance = rayLimit;

                    //finds nearest point on the axis line that is closest to the intersection point
                    ClosestPointsOnTwoLines(out Vector3 p, out currentArrowPoint, axisArrowPlane.ClosestPointOnPlane(mouseRay.GetPoint(rayDistance)), axisArrowTransform.right, parent.position, ArrowDirection);

                }
                else
                {
                    //draws plane on the plane's axis then intersects mouseray with the axis
                    Plane plane = new Plane(ArrowDirection, parent.position);

                    float rayDistance;
                    float rayLimit = Vector3.Distance(Camera.main.transform.position, gizmoCameraTransform.position) * singleMoveLimitScale; 
                    //limit is distance from camera to object mulitplied by a scalar

                    if (!plane.Raycast(mouseRay, out rayDistance) || rayDistance > rayLimit) //limits ray distance to prevent moving to infinity
                        rayDistance = rayLimit;
                    
                    currentArrowPoint = plane.ClosestPointOnPlane(mouseRay.GetPoint(rayDistance));
                }

                bool setLastArrowPoint = true; //will be set to false if a boundry is hit
                if (lastArrowPoint != Vector3.zero)
                {
                    if (snapEnabled && activeArrow <= ArrowType.Z)//snaps to configurable amount when control is held down. does this by setting current arrow point to rounded distance
                        currentArrowPoint = LerpByDistance(lastArrowPoint, currentArrowPoint,
                            RoundTo(Vector3.Distance(lastArrowPoint, currentArrowPoint), snapTransformToUnit));

                    Vector3 projectedPosition = parent.position + currentArrowPoint - lastArrowPoint;
                    setLastArrowPoint = projectedPosition.y >= floorBound;//sets movement boundries
                    if (setLastArrowPoint)
                        parent.position += currentArrowPoint - lastArrowPoint; 
                }
                if (setLastArrowPoint) lastArrowPoint = currentArrowPoint; //last arrow point keeps track of where the mouse is relative to the center of the parent object

            }
            else
            {
                //Project a ray from mouse
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayLength;

                //if mouse ray doesn't intersect plane, set default ray length
                if (!axisPlane.Raycast(cameraRay, out rayLength)) rayLength = Vector3.Distance(Camera.main.transform.position, parent.position) * 10;

                //get intersection point; if none, find closest point to default length
                Vector3 pointToLook = axisPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));

                //Correct parent's forward depending on rotation axis. Y-axis does not need corrections
                Vector3 t;
                if (ActiveArrow == ArrowType.RZ) t = parent.right;
                else if (ActiveArrow == ArrowType.RX) t = parent.up;
                else t = parent.forward;
                parent.RotateAround(parent.position, axisPlane.normal, //defines point and axis plane
                    -1 * RoundTo(Vector3.SignedAngle(pointToLook - parent.position, t, axisPlane.normal) - startRotation, //rounds degrees of rotation axis forward to mouse ray intersection
                    snapEnabled ? snapRotationToDegree : 0f)); //if control is pressed, snap to configurable value, otherwise, don't snap
            }

            //allows for the parent transform to be modified multiple times without changing the parent's actual transform. It is set at the end of the Update loop.
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
        /// <summary>
        /// Sets the active arrow when a <see cref="SelectableArrow"/> is selected.
        /// </summary>
        /// <param name="arrowType"></param>
        private void OnArrowSelected(ArrowType arrowType)
        {

            ActiveArrow = arrowType;
            lastArrowPoint = Vector3.zero;
            bufferPassed = false;


            if (arrowType == ArrowType.P) //sets marker's plane
                markerPlane = new Plane(Vector3.Normalize(Camera.main.transform.forward), parent.position);
            else if (arrowType <= ArrowType.Z) //sets up axis arrows for plane creation
            {
                switch (arrowType)
                {
                    case ArrowType.X:
                        axisArrowTransform = arrowX;
                        break;
                    case ArrowType.Y:
                        axisArrowTransform = arrowY;
                        break;
                    case ArrowType.Z:
                        axisArrowTransform = arrowZ;
                        break;
                }
            }
            else if (arrowType >= ArrowType.RX) //creates plane for rotation
            {
                axisPlane = new Plane(ArrowDirection, parent.position);

                //the following code determines the starting offset between the mouse and the gizmo.
                //does the exact same thing as a normal rotation, but extracts the resulting angle as the initial offset

                //Project a ray from mouse
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayLength;

                //if mouse ray doesn't intersect plane, set default ray length
                if (!axisPlane.Raycast(cameraRay, out rayLength)) rayLength = Vector3.Distance(Camera.main.transform.position, parent.position) * 10;

                //get intersection point; if none, find closest point to default length
                Vector3 pointToLook = axisPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));

                //Correct parent's forward depending on rotation axis. Y-axis does not need corrections
                Vector3 t;
                if (ActiveArrow == ArrowType.RZ) t = parent.right;
                else if (ActiveArrow == ArrowType.RX) t = parent.up;
                else t = parent.forward;

                //sets the starting rotational offset to the angle
                startRotation = Vector3.SignedAngle(pointToLook - parent.position, t, axisPlane.normal);
            }


        }

        /// <summary>
        /// Sets the active arrow to <see cref="ArrowType.None"/> when a
        /// <see cref="SelectableArrow"/> is released.
        /// </summary>
        private void OnArrowReleased()
        {
            ActiveArrow = ArrowType.None;

            //detect if object is out of bounds
            float x = transform.parent.position.x;
            float y = transform.parent.position.y;
            float z = transform.parent.position.z;
            transform.parent.position = new Vector3(
                Mathf.Abs(x) > bounds ? (x / Mathf.Abs(x) * bounds) : x,
                Mathf.Abs(y) > bounds ? (y / Mathf.Abs(y) * bounds) : y,
                Mathf.Abs(z) > bounds ? (z / Mathf.Abs(z) * bounds) : z);

            //move the camera
            gizmoCameraTransform.position = transform.parent.position;
            cam.FocusPoint = () => gizmoCameraTransform.position;

        }
        public void HierarchyRigidbodiesToDictionary() //save the state of all gameobject's rigidbodies as a dictionary
        {
            rigidbodiesKinematicStateInScene = new Dictionary<Rigidbody, bool>();
            GameObject Game = GameObject.Find("Game");
            foreach (Rigidbody rb in Game.GetComponentsInChildren<Rigidbody>()) //searches for all rigidbodies under the "Game" parent
            {
                if (rb.gameObject.transform.parent != transform)//skips gizmos
                    rigidbodiesKinematicStateInScene.Add(rb, rb.isKinematic);
            }
        }
        /// <summary>
        /// Enables or disables rigidbodies using isKinematic and detect collisions
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRigidbodies(bool enabled)
        {
            foreach (KeyValuePair<Rigidbody, bool> rb in rigidbodiesKinematicStateInScene)
            {
                if (rb.Key != null)
                {
                    if (enabled)
                    {
                        rb.Key.isKinematic = rb.Value; //saved dictionary state for reactivating the rigidbody's motion
                        rb.Key.detectCollisions = true;
                    }
                    else
                    {
                        rb.Key.isKinematic = true;
                        rb.Key.detectCollisions = false;
                    }
                }
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
        private bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) //used for axis arrows movement
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
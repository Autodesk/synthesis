using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Gizmo;
using Synthesis.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synthesis.Configuration {
    public class MoveArrow : MonoBehaviour {
        private const float SCALE                    = 0.075f;
        private const float GIZMO_PITCH              = -80;
        private const float SNAP_ROTATION_TO_DEGREES = 15;
        private const float SNAP_POSITION_TO_METERS  = 0.5f;
        private const float FLOOR_BOUNDS             = 0f;
        private const float BOUNDS                   = 50f;

        // limits a single movement boundary to this number times the distance from camera to position
        private const float SINGLE_MOVE_LIMIT_SCALE = 20f;

        private Vector3 _initialScale;
        private Vector3 _lastArrowPoint;
        private ArrowType _activeArrow;
        private bool _bufferPassed;
        private bool _snapEnabled;

        private Plane _axisPlane;
        private Plane _markerPlane;

        private Transform _parent;
        private Func<Vector3> _originalCameraFocusPoint;
        private Transform _gizmoCameraTransform;

        private CameraController _camComtroller;
        private ICameraMode _previousMode;
        private Vector3 _previousCameraPosition;
        private Quaternion _previousCameraRotation;
        private OrbitCameraMode _orbit;
        private float _originalLowerPitch;

        private float _startRotation;

        private Transform _arrowTransform;
        private Transform _axisArrowTransform;
        private Transform _arrowX;
        private Transform _arrowY;
        private Transform _arrowZ;

        private RobotSimObject _robot;

        private Camera _mainCam;

        // To use: move gizmo as a child of the object you want to move in game
        // Press R to reset rotation
        // Press CTRL to snap to nearest configured multiple when moving
        // Added gameObjects to Game while this script is active will not have their rigidbodies disabled

        /// <summary>
        /// Gets or sets the active selected arrow. When <see cref="ActiveArrow"/>
        /// is changed, the "SetActiveArrow" message is broadcast to all
        /// <see cref="SelectableArrow"/>s.
        /// </summary>
        private ArrowType ActiveArrow {
            get => _activeArrow;
            set {
                _activeArrow = value;
                BroadcastMessage("SetActiveArrow", _activeArrow);
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> representing the direction the selected
        /// arrow is facing, or <see cref="Vector3.zero"/> if no arrow is selected.
        /// </summary>
        private Vector3 ArrowDirection {
            get {
                switch (ActiveArrow) {
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

        private void Awake() {
            _mainCam            = Camera.main!;
            _camComtroller      = _mainCam.GetComponent<CameraController>();
            _originalLowerPitch = _camComtroller.PitchLowerLimit;
            _robot              = RobotSimObject.GetCurrentlyPossessedRobot();

            if (OrbitCameraMode.FocusPoint == null) {
                _originalCameraFocusPoint = () =>
                    _robot.GroundedNode != null && _robot.GroundedBounds != null
                        ? _robot.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(_robot.GroundedBounds.center)
                        : Vector3.zero;
            } else {
                _originalCameraFocusPoint = (Func<Vector3>) OrbitCameraMode.FocusPoint.Clone();
            }
            _previousMode             = _camComtroller.CameraMode;
            _camComtroller.CameraMode = CameraController.CameraModes["Orbit"];

            var camTrf              = _camComtroller.transform;
            _previousCameraPosition = camTrf.position;
            _previousCameraRotation = camTrf.rotation;

            // configure axis arrow transforms for later use
            _arrowTransform = transform;

            _arrowX = transform.Find("X").GetComponent<Transform>();
            _arrowY = transform.Find("Y").GetComponent<Transform>();
            _arrowZ = _arrowTransform.Find("Z").GetComponent<Transform>();

            var arrowScale = _arrowTransform.localScale;

            _initialScale = new Vector3(arrowScale.x / _arrowTransform.lossyScale.x,
                _arrowTransform.localScale.y / arrowScale.y, arrowScale.z / arrowScale.z);
        }

        private void setTransform() // called to set certain value when activated or when the parent changes
        {
            // SetRigidbodies(false);
            PhysicsManager.IsFrozen = true;

            _parent                       = transform.parent;
            _arrowTransform.localPosition = Vector3.zero;
            _arrowTransform.localRotation = Quaternion.identity;

            // clang-format off
            _gizmoCameraTransform          = new GameObject().transform;
            _gizmoCameraTransform.position = _arrowTransform.parent.position;           // camera shifting
            OrbitCameraMode.FocusPoint    = () => _gizmoCameraTransform.position; // camera focus
            _camComtroller.PitchLowerLimit           = GIZMO_PITCH;                          // camera pitch limits
            // clang-format on
        }

        private void RestoreCameraMode() {
            _camComtroller.CameraMode         = _previousMode;
            _camComtroller.transform.position = _previousCameraPosition;
            _camComtroller.transform.rotation = _previousCameraRotation;

            if (_originalCameraFocusPoint == null) {
                OrbitCameraMode.FocusPoint = () =>
                    _robot.GroundedNode != null && _robot.GroundedBounds != null
                        ? _robot.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(_robot.GroundedBounds.center)
                        : Vector3.zero;
            } else {
                OrbitCameraMode.FocusPoint = _originalCameraFocusPoint;
            }

            // test if cam.cameraMode is of type OrbitCameraMode
            if (_camComtroller.CameraMode is OrbitCameraMode) {
                _camComtroller.PitchLowerLimit = _originalLowerPitch;
            }
        }

        private void DisableGizmo() // makes sure values are set correctly when the gizmo is removed
        {
            Destroy(_gizmoCameraTransform.gameObject);
            RestoreCameraMode();
            CameraController.isOverGizmo = false; // this doesn't get reset?
            PhysicsManager.IsFrozen      = false;
        }

        private void OnEnable() {
            setTransform();
        }

        private void OnDestroy() {
            DisableGizmo();
        }

        private SelectableArrow _currentlyHovering;

        private void Update() {
            Ray cameraRay = _mainCam.ScreenPointToRay(Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.Escape)) {
                GizmoManager.ExitGizmo();
                RestoreCameraMode();
            }
            if (Input.GetKeyDown(KeyCode.R)) // Reset on press R
            {
                _parent.rotation = Quaternion.identity;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) // enable snap on ctrl press
            {
                _snapEnabled = true;
            } else {
                _snapEnabled = false;
            }

            if (_activeArrow == ArrowType.None) // skip if there no gizmo components being dragged
                return;

            // This allows for any updates from OnClick to complete before translation starts
            if (!_bufferPassed) {
                _bufferPassed = true;
                return;
            }

            switch (_activeArrow) {
                // for marker point: allows for drag on a plane directly facing camera
                case ArrowType.P: {
                    _markerPlane.Raycast(cameraRay, out var rayLength); // intersection between mouse ray and plane
                    _parent.position = cameraRay.GetPoint(rayLength);   // finds point on ray

                    if (_parent.position.y < 0) // limits y axis movements
                        _parent.position = new Vector3(_parent.position.x, 0, _parent.position.z);
                    break;
                }
                // plane and axis arrows movements
                case <= ArrowType.XY: {
                    Ray mouseRay = _mainCam.ScreenPointToRay(Input.mousePosition);
                    Vector3 currentArrowPoint;

                    if (_activeArrow <= ArrowType.Z) {
                        // draws plane with the same normal as the axis arrow, which faces the camera
                        Plane axisArrowPlane = new Plane(_axisArrowTransform.forward, _parent.position);

                        // a ray from the mouse is drawn to intersect the plane
                        float rayLimit = Vector3.Distance(_mainCam.transform.position, _gizmoCameraTransform.position) *
                                         SINGLE_MOVE_LIMIT_SCALE;
                        if (!axisArrowPlane.Raycast(mouseRay, out var rayDistance) ||
                            rayDistance >
                                rayLimit) // limits the maximum distance the ray can be to prevent moving it to infinity
                            rayDistance = rayLimit;

                        // finds nearest point on the axis line that is closest to the intersection point
                        ClosestPointsOnTwoLines(out Vector3 _, out currentArrowPoint,
                            axisArrowPlane.ClosestPointOnPlane(mouseRay.GetPoint(rayDistance)),
                            _axisArrowTransform.right, _parent.position, ArrowDirection);

                    } else {
                        // draws plane on the plane's axis then intersects mouse ray with the axis
                        Plane plane = new Plane(ArrowDirection, _parent.position);

                        float rayLimit = Vector3.Distance(_mainCam.transform.position, _gizmoCameraTransform.position) *
                                         SINGLE_MOVE_LIMIT_SCALE;
                        // limit is distance from camera to object multiplied by a scalar

                        if (!plane.Raycast(mouseRay, out var rayDistance) ||
                            rayDistance > rayLimit) // limits ray distance to prevent moving to infinity
                            rayDistance = rayLimit;

                        currentArrowPoint = plane.ClosestPointOnPlane(mouseRay.GetPoint(rayDistance));
                    }

                    bool setLastArrowPoint = true; // will be set to false if a boundary is hit
                    if (_lastArrowPoint != Vector3.zero) {
                        if (_snapEnabled &&
                            _activeArrow <=
                                ArrowType.Z) // snaps to configurable amount when control is held down. does this
                            // by setting current arrow point to rounded distance
                            currentArrowPoint = LerpByDistance(_lastArrowPoint, currentArrowPoint,
                                RoundTo(Vector3.Distance(_lastArrowPoint, currentArrowPoint), SNAP_POSITION_TO_METERS));

                        Vector3 projectedPosition = _parent.position + currentArrowPoint - _lastArrowPoint;
                        setLastArrowPoint         = projectedPosition.y >= FLOOR_BOUNDS; // sets movement bounds
                        if (setLastArrowPoint)
                            _parent.position += currentArrowPoint - _lastArrowPoint;
                    }
                    if (setLastArrowPoint)
                        _lastArrowPoint =
                            currentArrowPoint; // last arrow point keeps track of where the mouse is relative
                    // to the center of the parent object
                    break;
                }
                default: {
                    // if mouse ray doesn't intersect plane, set default ray length
                    if (!_axisPlane.Raycast(cameraRay, out var rayLength))
                        rayLength = Vector3.Distance(_mainCam.transform.position, _parent.position) * 10;

                    // get intersection point; if none, find closest point to default length
                    Vector3 pointToLook = _axisPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));

                    // Correct parent's forward depending on rotation axis. Y-axis does not need corrections
                    Vector3 t;
                    if (ActiveArrow == ArrowType.RZ)
                        t = _parent.right;
                    else if (ActiveArrow == ArrowType.RX)
                        t = _parent.up;
                    else
                        t = _parent.forward;
                    _parent.RotateAround(_parent.position, _axisPlane.normal, // defines point and axis plane
                        -1 *
                            RoundTo(
                                Vector3.SignedAngle(pointToLook - _parent.position, t, _axisPlane.normal) -
                                    _startRotation, // rounds degrees of rotation axis forward to mouse ray intersection
                                _snapEnabled
                                    ? SNAP_ROTATION_TO_DEGREES
                                    : 0f)); // if control is pressed, snap to configurable value, otherwise, don't snap
                    break;
                }
            }

            // allows for the parent transform to be modified multiple times without changing the parent's actual
            // transform. It is set at the end of the Update loop.
            transform.parent = _parent;
        }

        /// <summary>
        /// Scales the arrows to maintain a constant size relative to screen coordinates.
        /// </summary>
        private void LateUpdate() {
            Plane plane          = new Plane(_mainCam.transform.forward, _mainCam.transform.position);
            float dist           = plane.GetDistanceToPoint(transform.position);
            transform.localScale = _initialScale * SCALE * dist;
            Vector3 scaleTmp     = gameObject.transform.localScale;
            scaleTmp.x /= _parent.localScale.x;
            scaleTmp.y /= _parent.localScale.y;
            scaleTmp.z /= _parent.localScale.z;
            gameObject.transform.localScale = scaleTmp;
        }

        /// <summary>
        /// Sets the active arrow when a <see cref="SelectableArrow"/> is selected.
        /// </summary>
        /// <param name="arrowType"></param>
        private void OnArrowSelected(ArrowType arrowType) {
            ActiveArrow     = arrowType;
            _lastArrowPoint = Vector3.zero;
            _bufferPassed   = false;

            if (arrowType == ArrowType.P) // sets marker's plane
                _markerPlane = new Plane(Vector3.Normalize(_mainCam.transform.forward), _parent.position);
            else if (arrowType <= ArrowType.Z) // sets up axis arrows for plane creation
            {
                switch (arrowType) {
                    case ArrowType.X:
                        _axisArrowTransform = _arrowX;
                        break;
                    case ArrowType.Y:
                        _axisArrowTransform = _arrowY;
                        break;
                    case ArrowType.Z:
                        _axisArrowTransform = _arrowZ;
                        break;
                }
            } else if (arrowType >= ArrowType.RX) // creates plane for rotation
            {
                _axisPlane = new Plane(ArrowDirection, _parent.position);

                // the following code determines the starting offset between the mouse and the gizmo.
                // does the exact same thing as a normal rotation, but extracts the resulting angle as the initial
                // offset

                // Project a ray from mouse
                Ray cameraRay = _mainCam.ScreenPointToRay(Input.mousePosition);

                // if mouse ray doesn't intersect plane, set default ray length
                if (!_axisPlane.Raycast(cameraRay, out var rayLength))
                    rayLength = Vector3.Distance(_mainCam.transform.position, _parent.position) * 10;

                // get intersection point; if none, find closest point to default length
                Vector3 pointToLook = _axisPlane.ClosestPointOnPlane(cameraRay.GetPoint(rayLength));

                // Correct parent's forward depending on rotation axis. Y-axis does not need corrections
                Vector3 t;
                if (ActiveArrow == ArrowType.RZ)
                    t = _parent.right;
                else if (ActiveArrow == ArrowType.RX)
                    t = _parent.up;
                else
                    t = _parent.forward;

                // sets the starting rotational offset to the angle
                _startRotation = Vector3.SignedAngle(pointToLook - _parent.position, t, _axisPlane.normal);
            }
        }

        /// <summary>
        /// Sets the active arrow to <see cref="ArrowType.None"/> when a
        /// <see cref="SelectableArrow"/> is released.
        /// </summary>
        private void OnArrowReleased() {
            ActiveArrow = ArrowType.None;

            // detect if object is out of bounds
            float x                   = transform.parent.position.x;
            float y                   = transform.parent.position.y;
            float z                   = transform.parent.position.z;
            transform.parent.position = new Vector3(Mathf.Abs(x) > BOUNDS ? (x / Mathf.Abs(x) * BOUNDS) : x,
                Mathf.Abs(y) > BOUNDS ? (y / Mathf.Abs(y) * BOUNDS) : y,
                Mathf.Abs(z) > BOUNDS ? (z / Mathf.Abs(z) * BOUNDS) : z);

            // move the camera
            _gizmoCameraTransform.position = transform.parent.position;
            OrbitCameraMode.FocusPoint = () => _gizmoCameraTransform.position;
        }

        /// <summary>
        /// Enables or disables rigidbodies using isKinematic and detect collisions
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRigidbodies(bool enabled) {
            // Robot exists
            if (RobotSimObject.CurrentlyPossessedRobot != String.Empty) {
                var robot = RobotSimObject.GetCurrentlyPossessedRobot();
                var rbs   = robot.RobotNode.GetComponentsInChildren<Rigidbody>();
                rbs.ForEach(e => {
                    e.isKinematic      = !enabled;
                    e.detectCollisions = enabled;
                });
            }

            if (FieldSimObject.CurrentField != null) {
                FieldSimObject.CurrentField.GroundedNode.GetComponentsInChildren<Rigidbody>()
                    .Where(e => e.name != "grounded" && !e.name.StartsWith("gamepiece"))
                    .Concat(FieldSimObject.CurrentField.Gamepieces.Where(e => !e.IsCurrentlyPossessed)
                                .Select(e => e.GamepieceObject.GetComponent<Rigidbody>()))
                    .ForEach(e => {
                        e.isKinematic      = !enabled;
                        e.detectCollisions = enabled;
                    });
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
        private bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2,
            Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) // used for axis arrows movement
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
            float c   = Vector3.Dot(lineVec1, r);
            float f   = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        float RoundTo(float value, float multipleOf) // used for snapping the gizmo to the nearest value
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
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

using Input = UnityEngine.Input;

public class GodMode : MonoBehaviour {
    public double ZoomSensitivity = 0.15;
    public double MovementSpeed   = 3.0;
    private GameObject grabbedObject;
    private ConfigurableJoint grabJoint;
    private double grabbedDistance = 0.0;

    private Rigidbody _pointerBody = null;

    private GameObject GetGameObjectWithRigidbody(GameObject gameObj) {
        Rigidbody rb = gameObj.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic) {
            return gameObj;
        } else {
            return gameObj.transform.parent != null ? GetGameObjectWithRigidbody(gameObj.transform.parent.gameObject)
                                                    : null;
        }
    }

    public const string ENABLED_GOD_MODE_INPUT = "input/enable_god_mode";
    public const string GOD_MODE_DRAG_INPUT    = "input/god_mode_drag_object";

    private void Start() {
        InputManager.AssignValueInput(ENABLED_GOD_MODE_INPUT,
            TryGetSavedInput(ENABLED_GOD_MODE_INPUT, new Digital("G", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
        InputManager.AssignValueInput(
            GOD_MODE_DRAG_INPUT, TryGetSavedInput(GOD_MODE_DRAG_INPUT,
                                     new Digital("Mouse0", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
    }

    private Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input            = (Digital)PreferenceManager.GetPreference<InputData[]>(key) [0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        } else {
            return defaultInput;
        }
    }

    private float _lastUpdate = float.NaN;
    private Vector3 _speed, _lastPosition;

    private void Update() {
        bool godModeKeyDown = InputManager.MappedValueInputs[ENABLED_GOD_MODE_INPUT].Value == 1.0F;
        bool mouseDown      = InputManager.MappedValueInputs[GOD_MODE_DRAG_INPUT].Value == 1.0F;
        if (godModeKeyDown) {
            if (mouseDown && grabbedObject == null) {
                Camera camera = Camera.main;
                Ray ray       = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(ray, out hitInfo);
                if (hit) {
                    grabbedObject = GetGameObjectWithRigidbody(hitInfo.collider.gameObject);
                    if (grabbedObject != null) {

                        GameObject gameObj       = new GameObject("GODMODE_POINTER_RB");
                        _pointerBody             = gameObj.AddComponent<Rigidbody>();
                        _pointerBody.isKinematic = true;

                        _lastUpdate   = Time.realtimeSinceStartup;
                        _speed        = Vector3.zero;
                        _lastPosition = grabbedObject.transform.position;

                        grabbedDistance                 = hitInfo.distance;
                        _pointerBody.transform.position = hitInfo.point;
                        // Add a joint to the grabbed object anchored at where the user clicked
                        Vector3 localCoords = grabbedObject.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
                        grabJoint           = grabbedObject.AddComponent<ConfigurableJoint>();
                        grabJoint.connectedBody = _pointerBody;
                        grabJoint.anchor        = localCoords;
                        // GrabJoint.autoConfigureConnectedAnchor = false;
                        // GrabJoint.connectedAnchor = localCoords;
                        grabJoint.xMotion = grabJoint.yMotion = grabJoint.zMotion = ConfigurableJointMotion.Locked;
                    }
                }
            } else if (mouseDown && grabJoint != null) {
                // Move towards and away from the camera on scroll
                grabbedDistance += ZoomSensitivity * Input.mouseScrollDelta.y;
                Camera camera         = Camera.main;
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z       = (float)grabbedDistance;
                Ray ray               = camera.ScreenPointToRay(mousePosition);

                // Move grabbed object towards mouse cursor
                if (ray.GetPoint((float)grabbedDistance).y >= 0) {
                    var delta = (ray.GetPoint((float)grabbedDistance) - _pointerBody.position) * (float)MovementSpeed *
                                Time.deltaTime;
                    _pointerBody.position += delta;
                    _speed = delta / Time.deltaTime;
                }
            }
        }

        if (!mouseDown && grabJoint != null) {
            Destroy(grabJoint);
            Destroy(_pointerBody);
            _pointerBody                                     = null;
            grabbedObject.GetComponent<Rigidbody>().velocity = _speed;
            grabbedObject                                    = null;
            grabJoint                                        = null;
        }
    }
}

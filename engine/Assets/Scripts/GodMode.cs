using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using Input = UnityEngine.Input;

public class GodMode : MonoBehaviour
{
    public double ZoomSensitivity = 0.15;
    public double MovementSpeed = 3.0;
    private GameObject grabbedObject;
    private ConfigurableJoint grabJoint;
    private double grabbedDistance = 0.0;

    private GameObject GetGameObjectWithRigidbody(GameObject gameObj)
    {
        Rigidbody rb = gameObj.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic) return gameObj;
        return gameObj.transform.parent != null ? GetGameObjectWithRigidbody(gameObj.transform.parent.gameObject) : null;
    }

    private void Start()
    {
        InputManager.AssignValueInput("enable_god_mode", new Digital("G"));
        InputManager.AssignValueInput("god_mode_drag_object", new Digital("Mouse0"));
    }

    private void Update()
    {
        bool godModeKeyDown = InputManager.MappedValueInputs["enable_god_mode"].Value == 1.0F;
        bool mouseDown = InputManager.MappedValueInputs["god_mode_drag_object"].Value == 1.0F;
        if (godModeKeyDown)
        {
            if (mouseDown && grabbedObject == null)
            {
                Camera camera = Camera.main;
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(ray, out hitInfo);
                if (hit)
                {
                    grabbedObject = GetGameObjectWithRigidbody(hitInfo.collider.gameObject);
                    if (grabbedObject != null)
                    {
                        grabbedDistance = hitInfo.distance;
                        // add a joint to the grabbed object anchored at where the user clicked
                        Vector3 localCoords = grabbedObject.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
                        grabJoint = grabbedObject.AddComponent<ConfigurableJoint>();
                        grabJoint.anchor = localCoords;
                        grabJoint.autoConfigureConnectedAnchor = false;
                        grabJoint.xMotion = grabJoint.yMotion = grabJoint.zMotion = ConfigurableJointMotion.Locked;
                    }
                }
            }

            if (mouseDown && grabJoint != null)
            {
                // move towards and away from the camera on scroll
                grabbedDistance += ZoomSensitivity * -Input.mouseScrollDelta.y;
                Camera camera = Camera.main;
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = (float)grabbedDistance;
                Ray ray = camera.ScreenPointToRay(mousePosition);
                // move grabbed object towards mouse cursor
                grabJoint.connectedAnchor += (ray.GetPoint((float)grabbedDistance) - grabJoint.connectedAnchor) *
                                             (float)MovementSpeed * Time.deltaTime;
            }
        }

        if (!mouseDown && grabJoint != null)
        {
            Destroy(grabJoint);
            grabbedObject = null;
            grabJoint = null;
        }
    }
}
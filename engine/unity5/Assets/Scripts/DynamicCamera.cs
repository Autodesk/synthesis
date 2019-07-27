using UnityEngine;
using System.Collections;
using System;
using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.Camera;
using Synthesis.States;
using Synthesis.Robot;

public class DynamicCamera : MonoBehaviour
{
    /// <summary>
    /// The scrolling enabled.
    /// </summary>
    public static bool ControlEnabled { get; set; }

    /// <summary>
    /// If true, this camera will move according to the active camera state.
    /// </summary>
    public static bool MovementEnabled { get; set; }

    /// <summary>
    /// Gets the state of the camera.
    /// </summary>
    /// <value>The state of the camera.</value>
    public CameraState ActiveState { get; private set; }

    /// <summary>
    /// Abstract class for defining various states of the camera.
    /// </summary>
    public abstract class CameraState
    {
        /// <summary>
        /// The attached camera instance.
        /// </summary>
        protected MonoBehaviour Mono { get; private set; }
        
        /// <summary>
        /// The state's <see cref="IRobotProvider"/> instance for accessing robot information.
        /// </summary>
        protected IRobotProvider RobotProvider { get; private set; }

        /// <summary>
        /// Initializes a nwe <see cref="CameraState"/> instance.
        /// </summary>
        /// <param name="mono"></param>
        public CameraState(MonoBehaviour mono)
        {
            Mono = mono;
            RobotProvider = StateMachine.SceneGlobal.FindState<IRobotProvider>();
        }

        /// <summary>
        /// Init this instance (will be called in SwitchCameraMode()).
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Update this instance (will be called in Update()).
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// End this instance (will be called in SwitchCameraMode()).
        /// </summary>
        public abstract void End();
    }

    /// <summary>
    /// Derives from CameraState to create a view from the Driver Station.
    /// </summary>
    public class DriverStationState : CameraState
    {
        Quaternion startRotation;
        Quaternion lookingRotation;
        Quaternion currentRotation;
        //The default starting position of the camera
        static Vector3 position1Vector = new Vector3(0f, 1.5f, -9.5f);
        //The opposite default starting position of the camera
        static Vector3 position2Vector = new Vector3(0f, 1.5f, 9.5f);
        Vector3 currentPosition;

        float transformSpeed;

        readonly bool opposite;

        public DriverStationState(MonoBehaviour mono, bool oppositeSide = false)
            : base(mono)
        {
            opposite = oppositeSide;
        }

        public override void Init()
        {
            //Decide which side the camera should be placed depending on whether it is opposite of default position
            if (opposite) currentPosition = position2Vector;
            else currentPosition = position1Vector;
            Mono.transform.position = currentPosition;

            startRotation = Quaternion.LookRotation(Vector3.zero - Mono.transform.position);
            currentRotation = startRotation;
            transformSpeed = 2.5f;
        }

        public override void Update()
        {
            lookingRotation = Quaternion.LookRotation(RobotProvider.Robot.transform.position - Mono.transform.position);
            currentRotation = Quaternion.Lerp(startRotation, lookingRotation, 0.5f);

            if (ControlEnabled && RobotProvider.RobotActive)
            {
                var delta = InputControl.GetAxis(Controls.Global.GetAxes().cameraHorizontal) * new Vector3(1, 0, 0) * transformSpeed * Time.deltaTime;
                currentPosition += opposite ? -delta : delta;
            }

            Mono.transform.rotation = currentRotation;
            Mono.transform.position = currentPosition;
        }


        public override void End()
        {
        }
    }

    /// <summary>
    /// Derives from CameraState to create a view that orbits and follows the robot.
    /// </summary>
    public class OrbitState : CameraState
    {
        Vector3 targetVector;
        Vector3 rotateVector;
        Vector3 lagVector;
        Vector3 lockedVector;
        const float lagResponsiveness = 10f;
        float magnification = 5.0f;
        float cameraAngle = 45f;
        float panValue = 0f;

        public OrbitState(MonoBehaviour mono)
            : base(mono) { }

        public override void Init()
        {
            // Calculates the initial robot position
            rotateVector = RotateXZ(new Vector3(-1f, 1f, 0f), Vector3.zero, 0f, magnification);
            lagVector = rotateVector;
            lockedVector = rotateVector;
        }

        public override void Update()
        {
            if (RobotProvider.Robot == null)
                return;

            // Focus on node 0 of the robot
            targetVector = RobotProvider.Robot.transform.position;

            bool adjusting = false;

            if (ControlEnabled && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetMouseButton(0))
                {
                    // Pan around the robot
                    adjusting = true;
                    cameraAngle = Mathf.Max(Mathf.Min(cameraAngle - Input.GetAxis("Mouse Y") * 5f, 90f), 0f);
                    panValue = -Input.GetAxis("Mouse X") / 5f;
                }
                else
                {
                    panValue = 0f;

                    if (Input.GetMouseButton(1))
                    {
                        // Zoom in and out
                        adjusting = true;
                        magnification = Mathf.Max(Mathf.Min(magnification - ((Input.GetAxis("Mouse Y") / 5f) * magnification), 12f), 1.5f);
                    }
                }
            }
            else
            {
                panValue = 0f;
            }

            if (adjusting)
            {
                // Unlocks the camera position for adjustment
                rotateVector = RotateXZ(rotateVector, targetVector, panValue, magnification);
                rotateVector.y = targetVector.y + magnification * Mathf.Sin(cameraAngle * Mathf.Deg2Rad);
                lockedVector = RobotProvider.Robot.transform.GetChild(0).InverseTransformPoint(rotateVector);
            }
            else
            {
                rotateVector = RobotProvider.Robot.transform.GetChild(0).TransformPoint(lockedVector);
                rotateVector.y = targetVector.y + Mathf.Abs(rotateVector.y - targetVector.y);
            }

            if (adjusting)
            {
                // Unlocks the camera position for adjustment
                rotateVector = RotateXZ(rotateVector, targetVector, panValue, magnification);
                rotateVector.y = targetVector.y + magnification * Mathf.Sin(cameraAngle * Mathf.Deg2Rad);
                lockedVector = RobotProvider.Robot.transform.InverseTransformPoint(rotateVector);
            }
            else
            {
                rotateVector = RobotProvider.Robot.transform.TransformPoint(lockedVector);
                rotateVector.y = targetVector.y + Mathf.Abs(rotateVector.y - targetVector.y);
            }

            // Calculate smooth camera movement
            lagVector = CalculateLagVector(lagVector, rotateVector, lagResponsiveness);

            Mono.transform.position = lagVector;
            Mono.transform.LookAt(targetVector);
        }

        public override void End()
        {
        }

        /// <summary>
        /// Rotates the Vector3 around the given origin with the provided angle and magnitude (distance).
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="origin"></param>
        /// <param name="theta"></param>
        /// <param name="mag"></param>
        /// <returns></returns>
        Vector3 RotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
        {
            vector -= origin;
            Vector3 output = vector;
            output.x = Mathf.Cos(theta) * (vector.x) - Mathf.Sin(theta) * (vector.z);
            output.z = Mathf.Sin(theta) * (vector.x) + Mathf.Cos(theta) * (vector.z);

            return output.normalized * mag + origin;
        }
    }

    /// <summary>
    /// Derives from CameraState to create a first person view from the robot.
    /// </summary>
    /// 
    public class FreeroamState : CameraState
    {
        Vector3 positionVector;
        Vector3 lagPosVector;
        Vector3 rotationVector;
        Vector3 lagRotVector;
        float zoomValue;
        float lagZoom;
        const float lagResponsiveness = 15f;
        float rotationSpeed;
        float transformSpeed;
        float scrollWheelSensitivity;

        public FreeroamState(MonoBehaviour mono)
            : base(mono) { }

        public override void Init()
        {
            Mono.transform.position = new Vector3(0f, 1f, 0f);
            positionVector = new Vector3(0f, 1f, 0f);
            lagPosVector = positionVector;
            rotationVector = Vector3.zero;
            lagRotVector = rotationVector;
            zoomValue = 60f;
            lagZoom = zoomValue;
            rotationSpeed = 3f;
            transformSpeed = 2.5f;
            scrollWheelSensitivity = 40f;
        }

        public override void Update()
        {
            if (ControlEnabled && RobotProvider.RobotActive && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                if (InputControl.GetMouseButton(0))
                {
                    if (GameObject.Find("ChangeRobotPanel") || GameObject.Find("ChangeFieldPanel"))
                    {
                        ControlEnabled = false;
                    }
                    else
                    {
                        rotationVector.x -= InputControl.GetAxis("Mouse Y") * rotationSpeed;
                        rotationVector.y += InputControl.GetAxis("Mouse X") * rotationSpeed;
                        ControlEnabled = true;
                    }
                }

                //Use WASD to move camera position
                positionVector += InputControl.GetAxis(Controls.Global.GetAxes().cameraHorizontal) * Mono.transform.right * transformSpeed * Time.deltaTime;
                positionVector += InputControl.GetAxis(Controls.Global.GetAxes().cameraVertical) * Mono.transform.forward * transformSpeed * Time.deltaTime;

                zoomValue = Mathf.Max(Mathf.Min(zoomValue - InputControl.GetAxis("Mouse ScrollWheel") * scrollWheelSensitivity, 60.0f), 10.0f);

                lagRotVector = CalculateLagVector(lagRotVector, rotationVector, lagResponsiveness);
                lagZoom = CalculateLagScalar(lagZoom, zoomValue, lagResponsiveness);

                Mono.transform.position += positionVector;
                positionVector = Vector3.zero;
                Mono.transform.eulerAngles = lagRotVector;
                Mono.GetComponent<Camera>().fieldOfView = lagZoom;
            }
        }

        public override void End()
        {
            Mono.GetComponent<Camera>().fieldOfView = 60.0f;
        }

    }


    /// <summary>
    /// This state locates directly above the field and looks straight down on the field in order for robot positioning
    /// Not working well with 2016&2017 field because they are not centered
    /// </summary>
    public class OverviewState : CameraState
    {
        Vector3 lagVector;
        Vector3 positionVector;
        Vector3 rotationVector;
        Vector3 fieldVector;
        GameObject field;

        public OverviewState(MonoBehaviour mono)
            : base(mono)
        {
            field = GameObject.Find("Field");
            fieldVector = field.transform.position;
        }

        public override void Init()
        {
            positionVector = new Vector3(0f, 14f, 0f) + fieldVector;
            Mono.transform.position = positionVector;
            rotationVector = new Vector3(90f, 90f, 0f);
            Mono.transform.rotation = Quaternion.Euler(rotationVector);
        }
        public override void Update()
        {
            float change = Input.GetAxis("Mouse Y");
            if (Input.GetMouseButton(1) && positionVector.y + change > fieldVector.y + 1)
                positionVector.y += change;
            Mono.transform.position = positionVector;
        }

        public override void End()
        {

        }
    }

    //This state locates directly above the robot and follows it
    public class SateliteState : CameraState
    {
        Vector3 targetPosition;
        public GameObject target = GameObject.Find("Robot");
        public Vector3 targetOffset = new Vector3(0f, 6f, 0f);
        public Vector3 rotationVector = new Vector3(90f, 90f, 0f);

        public SateliteState(MonoBehaviour mono)
            : base(mono) { }

        public override void Init()
        {
            targetPosition = target.transform.position;
            Mono.transform.position = targetPosition + targetOffset;
            Mono.transform.rotation = Quaternion.Euler(rotationVector);
        }

        public override void Update()
        {
            if (RobotProvider.Robot != null && RobotProvider.Robot.transform.childCount > 0)
                targetPosition = RobotProvider.Robot.transform.position;

            Mono.transform.position = targetPosition + targetOffset;
            Mono.transform.rotation = Quaternion.Euler(rotationVector);
        }

        public override void End()
        {

        }
    }
    public class OrthographicSateliteState : CameraState
    {
        Vector3 targetPosition;
        public GameObject target = GameObject.Find("Robot");
        public Vector3 targetOffset = new Vector3(0f, 6f, 0f);
        public Vector3 rotationVector = new Vector3(90f, 90f, 0f);
        public float orthoSize = 5;

        public OrthographicSateliteState(MonoBehaviour mono)
            : base(mono) { }

        public override void Init()
        {
            targetPosition = target.transform.position;

            Mono.transform.position = targetPosition + targetOffset;
            Mono.transform.rotation = Quaternion.Euler(rotationVector);

            Mono.GetComponent<Camera>().orthographic = true;
            Mono.GetComponent<Camera>().orthographicSize = orthoSize;
        }

        public override void Update()
        {
            if (target != null && target.transform.childCount > 0)
            {
                targetPosition = target.transform.GetChild(0).transform.position;
            }
            else if (target != null)
            {
                targetPosition = target.transform.position;
            }

            Mono.transform.position = targetPosition + targetOffset;
            Mono.transform.rotation = Quaternion.Euler(rotationVector);

            Mono.GetComponent<Camera>().orthographic = true;
            Mono.GetComponent<Camera>().orthographicSize = orthoSize;
        }

        public override void End()
        {
            Mono.GetComponent<Camera>().orthographic = false;
        }
    }

    /// <summary>
    /// This state is made for sensor/robot camera configuration, will focus a given target object or by default on the first robot node
    /// Works basically the same as orbit view but focus closer
    /// </summary>
    public class ConfigurationState : CameraState
    {
        Vector3 targetVector;
        Vector3 rotateVector;
        Vector3 lagVector;
        const float lagResponsiveness = 10f;
        float magnification = 5.0f;
        float cameraAngle = 45f;
        float panValue = 0f;
        bool isRobotCamera;
        GameObject target;

        public ConfigurationState(MonoBehaviour mono, GameObject targetObject = null)
            : base(mono)
        {
            target = targetObject ?? RobotProvider.Robot;
            target.transform.Translate(new Vector3(0.0001f, 0, 0));
        }

        public override void Init()
        {
        }

        public override void Update()
        {
            if (target != null)
            {
                targetVector = target.transform.position;
                if (ControlEnabled) //Works like the old orbit state
                {
                    //Use left mouse to change angle
                    if (Input.GetMouseButton(0))
                    {
                        cameraAngle = Mathf.Max(Mathf.Min(cameraAngle - Input.GetAxis("Mouse Y") * 5f, 90f), 0f);
                        panValue = -Input.GetAxis("Mouse X") / 5f;
                    }
                    else
                    {
                        panValue = 0f;
                        //Use right mouse to change magnification
                        if (Input.GetMouseButton(1))
                        {
                            magnification = Mathf.Max(Mathf.Min(magnification - ((Input.GetAxis("Mouse Y") / 5f) * magnification), 12f), 0.5f);
                        }

                    }
                }
                else
                {
                    panValue = 0f;
                }

                rotateVector = RotateXZ(rotateVector, targetVector, panValue, magnification);
                rotateVector.y = targetVector.y + magnification * Mathf.Sin(cameraAngle * Mathf.Deg2Rad);

                lagVector = CalculateLagVector(lagVector, rotateVector, lagResponsiveness);

                Mono.transform.position = lagVector;
                Mono.transform.LookAt(targetVector);
            }
            else
            {
                target = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>().CurrentCamera;
            }

        }

        public override void End()
        {
        }

        Vector3 RotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
        {
            vector -= origin;
            Vector3 output = vector;
            output.x = Mathf.Cos(theta) * (vector.x) - Mathf.Sin(theta) * (vector.z);
            output.z = Mathf.Sin(theta) * (vector.x) + Mathf.Cos(theta) * (vector.z);

            return output.normalized * mag + origin;
        }
    }

    /// <summary>
    /// Switch to orbit state when the simulator starts
    /// </summary>
    void Start()
    {
        MovementEnabled = true;
        SwitchCameraState(new OrbitState(this));
    }

    void LateUpdate()
    {
        if (ActiveState != null && MovementEnabled) ActiveState.Update();
    }

    /// <summary>
    /// Switch to the next camera state
    /// </summary>
    /// <param name="currentCameraState"></param>
    public void ToggleCameraState(CameraState currentCameraState)
    {
        if (ControlEnabled)
        {
            if (currentCameraState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new OrbitState(this));
            else if (currentCameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new FreeroamState(this));
            else if (currentCameraState.GetType().Equals(typeof(FreeroamState))) SwitchCameraState(new OverviewState(this));
            else if (currentCameraState.GetType().Equals(typeof(OverviewState))) SwitchCameraState(new DriverStationState(this));
        }
        if (ActiveState != null) ActiveState.Update();
    }

    /// <summary>
    /// Switches the camera mode.
    /// </summary>
    /// <param name="state">State</param>
    public void SwitchCameraState(CameraState state)
    {
        if (ActiveState != null) ActiveState.End();
        ActiveState = state;
        ActiveState.Init();
    }

    /// Calculates the appropriate lag vector from the given current vector, target vector, and responsiveness constant.
    /// </summary>
    /// <param name="lagVector"></param>
    /// <param name="targetVector"></param>
    /// <param name="lagResponsiveness"></param>
    public static Vector3 CalculateLagVector(Vector3 lagVector, Vector3 targetVector, float lagResponsiveness)
    {
        Vector3 lagAmount = (targetVector - lagVector) * (lagResponsiveness * Time.deltaTime);

        if (lagAmount.magnitude < (targetVector - lagVector).magnitude)
            lagVector += lagAmount;
        else
            lagVector = targetVector;

        return lagVector;
    }

    /// <summary>
    /// Calculates the appropriate lag scalar from the given current scalar, target scalar, and responsiveness constant.
    /// </summary>
    /// <param name="lagScalar"></param>
    /// <param name="targetScalar"></param>
    /// <param name="lagResponsiveness"></param>
    /// <returns></returns>
    public static float CalculateLagScalar(float lagScalar, float targetScalar, float lagResponsiveness)
    {
        float lagAmount = (targetScalar - lagScalar) * (lagResponsiveness * Time.deltaTime);

        if (Mathf.Abs(lagAmount) < Mathf.Abs(targetScalar - lagScalar))
            lagScalar += lagAmount;
        else
            lagScalar = targetScalar;

        return lagScalar;
    }

    /// <summary>
    /// Switch to a specific state given the target state, only decide on which type the target state is
    /// </summary>
    /// <param name="targetState"></param> The (type of) state you want to switch to
    public void SwitchToState(CameraState targetState)
    {
        if (targetState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new DriverStationState(this));
        else if (targetState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new OrbitState(this));
        else if (targetState.GetType().Equals(typeof(FreeroamState))) SwitchCameraState(new FreeroamState(this));
        else if (targetState.GetType().Equals(typeof(OverviewState))) SwitchCameraState(new OverviewState(this));
    }
}
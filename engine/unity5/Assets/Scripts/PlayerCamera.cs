using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.FSM;

public class PlayerCamera : MonoBehaviour
{
    /// <summary>
    /// The scrolling enabled.
    /// </summary>
    public bool MovingEnabled { get; set; }
    public GameObject Robot;

    /// <summary>
    /// The state of the camera.
    /// </summary>
    CameraState _cameraState;

    /// <summary>
    /// Gets the state of the camera.
    /// </summary>
    /// <value>The state of the camera.</value>
    public CameraState cameraState
    {
        get
        {
            return _cameraState;
        }
    }

    public int StationIndex;
    public bool IsActive { get; set; }
    /// <summary>
    /// Abstract class for defining various states of the camera.
    /// </summary>
    public abstract class CameraState
    {
        public GameObject robot;
        /// <summary>
        /// The index of the driver station, 0-5 is Red1, Red2, Red3, Blue1, Blue2, Blue3
        /// </summary>

        protected PlayerCamera mono;

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

        public void SetTargetRobot(GameObject robot)
        {
            this.robot = robot;
        }
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
        Vector3 Red1Position = new Vector3(2.0f, 1.5f, -9.5f);
        Vector3 Red2Position = new Vector3(0f, 1.5f, -9.5f);
        Vector3 Red3Position = new Vector3(-2.0f, 1.5f, -9.5f);
        //The opposite default starting position of the camera
        Vector3 Blue1Position = new Vector3(2.0f, 1.5f, 9.5f);
        Vector3 Blue2Position = new Vector3(0f, 1.5f, 9.5f);
        Vector3 Blue3Position = new Vector3(-2.0f, 1.5f, 9.5f);
        Vector3 currentPosition;
        int stationIndex;
        float transformSpeed;
        private MainState main;

        public DriverStationState(PlayerCamera mono)
        {
            this.mono = mono;
            stationIndex = mono.StationIndex;
        }

        public override void Init()
        {
            //Decide where the camera should be placed depending on its index
            switch (stationIndex){
                case 0:
                    currentPosition = Red1Position;
                    break;
                case 1:
                    currentPosition = Red2Position;
                    break;
                case 2:
                    currentPosition = Red3Position;
                    break;
                case 3:
                    currentPosition = Blue1Position;
                    break;
                case 4:
                    currentPosition = Blue2Position;
                    break;
                case 5:
                    currentPosition = Blue3Position;
                    break;
            }
            
            mono.transform.position = currentPosition;

            startRotation = Quaternion.LookRotation(Vector3.zero - mono.transform.position);
            currentRotation = startRotation;
            transformSpeed = 2.5f;
            main = StateMachine.Instance.FindState<MainState>();
        }

        public override void Update()
        {
            //Look towards the robot
            if (robot != null && robot.transform.childCount > 0)
            {
                lookingRotation = Quaternion.LookRotation(robot.transform.GetChild(0).transform.position - mono.transform.position);
                currentRotation = Quaternion.Lerp(startRotation, lookingRotation, 0.5f);
            }
            else
            {
                robot = main.ActiveRobot.gameObject;
            }
            mono.transform.rotation = currentRotation;
            mono.transform.position = currentPosition;
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

        public OrbitState(PlayerCamera mono)
        {
            this.mono = mono;
        }

        public override void Init()
        {
            // Calculates the initial robot position
            rotateVector = RotateXZ(new Vector3(-1f, 1f, 0f), Vector3.zero, 0f, magnification);
            lagVector = rotateVector;
            lockedVector = rotateVector;
        }

        public override void Update()
        {
            if (robot != null && robot.transform.childCount > 0)
            {
                // Focus on node 0 of the robot
                targetVector = robot.transform.GetChild(0).transform.position;

                bool adjusting = false;

                if (mono.MovingEnabled && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
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
                    lockedVector = robot.transform.GetChild(0).InverseTransformPoint(rotateVector);
                }
                else
                {
                    rotateVector = robot.transform.GetChild(0).TransformPoint(lockedVector);
                    rotateVector.y = targetVector.y + Mathf.Abs(rotateVector.y - targetVector.y);
                }

                // Calculate smooth camera movement
                lagVector = CalculateLagVector(lagVector, rotateVector, lagResponsiveness);

                mono.transform.position = lagVector;
                mono.transform.LookAt(targetVector);
            }
            else
            {
                robot = GameObject.Find("Robot");
            }
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
    /// Switch to orbit state when the simulator starts
    /// </summary>
    void Start()
    {
        SwitchCameraState(new DriverStationState(this));
        Debug.Log("Start print robot: " + cameraState.robot);
    }

    void Update()
    {
        if(_cameraState == null) SwitchCameraState(new DriverStationState(this));
        Debug.Log("Update print robot: " + cameraState.robot);
        Robot = cameraState.robot;
    }
    void LateUpdate()
    {
        if (_cameraState != null) _cameraState.Update();
    }

    /// <summary>
    /// Switch to the next camera state
    /// </summary>
    /// <param name="currentCameraState"></param>
    public void ToggleCameraState(CameraState currentCameraState)
    {
        if (MovingEnabled)
        {
            if (currentCameraState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new OrbitState(this));
            else if (currentCameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new DriverStationState(this));
        }
        if (_cameraState != null) _cameraState.Update();
    }

    /// <summary>
    /// Switches the camera mode.
    /// </summary>
    /// <param name="state">State</param>
    public void SwitchCameraState(CameraState state)
    {
        if (_cameraState != null) _cameraState.End();
        _cameraState = state;
        _cameraState.Init();
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
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        gameObject.SetActive(true);
        SwitchCameraState(new DriverStationState(this));
    }
}
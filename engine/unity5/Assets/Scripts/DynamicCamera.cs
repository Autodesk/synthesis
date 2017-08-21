﻿using UnityEngine;
using System.Collections;
using System;

public class DynamicCamera : MonoBehaviour
{
    /// <summary>
    /// The scrolling enabled.
    /// </summary>
    public static bool MovingEnabled { get; set; }

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

    /// <summary>
    /// Abstract class for defining various states of the camera.
    /// </summary>
    public abstract class CameraState
    {
        public GameObject robot;
        protected MonoBehaviour mono;

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
        static Vector3 position1Vector = new Vector3(0f, 1.5f, -9.5f);
        static Vector3 position2Vector = new Vector3(0f, 1.5f, 9.5f);
        Vector3 currentPosition;

        float transformSpeed;
        bool opposite;

        public DriverStationState(MonoBehaviour mono, bool oppositeSide = false)
        {
            this.mono = mono;
            this.opposite = oppositeSide;
        }

        public override void Init()
        {
            if (opposite) currentPosition = position2Vector;
            else currentPosition = position1Vector;
            mono.transform.position = currentPosition;

            startRotation = Quaternion.LookRotation(Vector3.zero - mono.transform.position);
            currentRotation = startRotation;
            transformSpeed = 2.5f;
        }

        public override void Update()
        {
            if (robot != null && robot.transform.childCount > 0)
            {
                lookingRotation = Quaternion.LookRotation(robot.transform.GetChild(0).transform.position - mono.transform.position);
                currentRotation = Quaternion.Lerp(startRotation, lookingRotation, 0.5f);
            }
            else
            {
                robot = GameObject.Find("Robot");
            }
            if (MovingEnabled)
            {
                if (!opposite)
                {
                    currentPosition += Input.GetAxis("CameraHorizontal") * new Vector3(1, 0, 0) * transformSpeed * Time.deltaTime;
                }
                else
                {
                    currentPosition -= Input.GetAxis("CameraHorizontal") * new Vector3(1, 0, 0) * transformSpeed * Time.deltaTime;
                }
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
        #region old orbit state
        //Vector3 targetVector;
        //Vector3 rotateVector;
        //Vector3 lagVector;
        //const float lagResponsiveness = 10f;
        //float magnification = 5.0f;
        //float cameraAngle = 45f;
        //float panValue = 0f;

        //public OrbitState(MonoBehaviour mono)
        //{
        //    this.mono = mono;
        //}

        //public override void Init()
        //{
        //    rotateVector = new Vector3(0f, 1f, 0f);
        //    lagVector = rotateVector;
        //}

        //public override void Update()
        //{
        //    if (robot != null && robot.transform.childCount > 0)
        //    {
        //        targetVector = robot.transform.GetChild(0).transform.position;//AuxFunctions.TotalCenterOfMass(robot);

        //        if (MovingEnabled)
        //        {
        //            if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
        //            {
        //                cameraAngle = Mathf.Max(Mathf.Min(cameraAngle - Input.GetAxis("Mouse Y") * 5f, 90f), 0f);
        //                panValue = -Input.GetAxis("Mouse X") / 5f;
        //            }
        //            else
        //            {
        //                panValue = 0f;

        //                if (Input.GetMouseButton(1))
        //                {
        //                    magnification = Mathf.Max(Mathf.Min(magnification - ((Input.GetAxis("Mouse Y") / 5f) * magnification), 12f), 1.5f);

        //                }

        //            }
        //        }
        //        else
        //        {
        //            panValue = 0f;
        //        }

        //        rotateVector = rotateXZ(rotateVector, targetVector, panValue, magnification);
        //        rotateVector.y = targetVector.y + magnification * Mathf.Sin(cameraAngle * Mathf.Deg2Rad);

        //        lagVector = CalculateLagVector(lagVector, rotateVector, lagResponsiveness);

        //        mono.transform.position = lagVector;
        //        mono.transform.LookAt(targetVector);

        //    }
        //    else
        //    {
        //        robot = GameObject.Find("Robot");
        //    }
        //}

        //public override void End()
        //{
        //}

        //Vector3 rotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
        //{
        //    vector -= origin;
        //    Vector3 output = vector;
        //    output.x = Mathf.Cos(theta) * (vector.x) - Mathf.Sin(theta) * (vector.z);
        //    output.z = Mathf.Sin(theta) * (vector.x) + Mathf.Cos(theta) * (vector.z);

        //    return output.normalized * mag + origin;
        //}
    #endregion
        
        private Transform target;
        // The distance in the x-z plane to the target
        private float distance = 5f;
        // the height we want the camera to be above the target
        private float height = 1.5f;
        private float angleOffset;
        private float heightDamping = 5f;
        private float rotationDamping = 5f;

        public OrbitState(MonoBehaviour mono)
        {
            this.mono = mono;
        }
        public override void Init()
        {

        }


        public override void Update()
        {
            //Focus on the node 0
            target = GameObject.Find("Robot").transform.GetChild(0);

            // Early out if we don't have a target
            if (!target)
                return;

            // Calculate the current rotation angles (+90 actually makes it follows from behind)
            float wantedRotationAngle = target.eulerAngles.y + angleOffset;
            float wantedHeight = target.position.y + height;
            float currentRotationAngle = mono.transform.eulerAngles.y;
            float currentHeight = mono.transform.position.y;

            if (MovingEnabled)
            {
                //Use right mouse to adjust the distance of camera from the robot
                if (Input.GetMouseButton(1))
                {
                    distance = Mathf.Max(Mathf.Min(distance - ((Input.GetAxis("Mouse Y") / 5f) * distance), 12f), 1.5f);
                }
                //Use left mouse to adjust the angle the camera is pointing from and the height of camera
                else if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                {
                    angleOffset += Input.GetAxis("Mouse X") * 5;
                    height -= Input.GetAxis("Mouse Y")/2;
                }
            }

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            // Convert the angle into a rotation
            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            mono.transform.position = target.position;
            mono.transform.position -= currentRotation * Vector3.forward * distance;

            // Set the height of the camera
            mono.transform.position = new Vector3(mono.transform.position.x, currentHeight, mono.transform.position.z);

            // Always look at the target
            mono.transform.LookAt(target);
        }

        public override void End()
        {

        }
    }

    /// <summary>
    /// Derives from CameraState to create a first person view from the robot.
    /// </summary>
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
        {
            this.mono = mono;
        }

        public override void Init()
        {
            mono.transform.position = new Vector3(0f, 1f, 0f);
            positionVector = new Vector3(0f, 1f, 0f);
            lagPosVector = positionVector;
            rotationVector = Vector3.zero;
            lagRotVector = rotationVector;
            zoomValue = 60f;
            lagZoom = zoomValue;
            rotationSpeed = 3f;
            transformSpeed = 2.5f;
            scrollWheelSensitivity = 40f;
            if (robot == null) robot = GameObject.Find("robot");
        }

        public override void Update()
        {
            if (MovingEnabled)
            {
                if (InputControl.GetMouseButton(1))
                {
                    rotationVector.x -= InputControl.GetAxis("Mouse Y") * rotationSpeed;
                    rotationVector.y += Input.GetAxis("Mouse X") * rotationSpeed;
                }

                positionVector += Input.GetAxis("CameraHorizontal") * mono.transform.right * transformSpeed * Time.deltaTime;
                positionVector += Input.GetAxis("CameraVertical") * mono.transform.forward * transformSpeed * Time.deltaTime;

                zoomValue = Mathf.Max(Mathf.Min(zoomValue - InputControl.GetAxis("Mouse ScrollWheel") * scrollWheelSensitivity, 60.0f), 10.0f);

                //lagPosVector = CalculateLagVector(lagPosVector, positionVector, lagResponsiveness);
                lagRotVector = CalculateLagVector(lagRotVector, rotationVector, lagResponsiveness);
                lagZoom = CalculateLagScalar(lagZoom, zoomValue, lagResponsiveness);

                mono.transform.position += positionVector;
                positionVector = Vector3.zero;
                mono.transform.eulerAngles = lagRotVector;
                mono.GetComponent<Camera>().fieldOfView = lagZoom;
            }
        }

        public override void End()
        {
            mono.GetComponent<Camera>().fieldOfView = 60.0f;
        }

    }


    /// <summary>
    /// This state locates directly above the field and looks straight down on the field in order for robot positioning
    /// Not working well with 2016&2017 field because they are not centered
    /// </summary>
    public class OverviewState : CameraState
    {
        Vector3 positionVector;
        Vector3 rotationVector;
        Vector3 fieldVector;
        GameObject field;

        public OverviewState(MonoBehaviour mono)
        {
            this.mono = mono;
            field = GameObject.Find("Field");
            fieldVector = field.transform.position;
        }

        public override void Init()
        {
            positionVector = new Vector3(0f, 9f, 0f) + fieldVector;
            mono.transform.position = positionVector;
            rotationVector = new Vector3(90f, 90f, 0f);
            mono.transform.rotation = Quaternion.Euler(rotationVector);
            if (robot == null) robot = GameObject.Find("robot");
        }
        public override void Update()
        {
        }

        public override void End()
        {

        }
    }

    //This state locates directly above the robot and follows it
    public class SateliteState : CameraState
    {
        Vector3 targetPosition;
        Vector3 rotationVector;
        public GameObject target;

        public SateliteState(MonoBehaviour mono)
        {
            this.mono = mono;
        }

        public override void Init()
        {
            target = GameObject.Find("Robot");
            targetPosition = target.transform.position;
            rotationVector = new Vector3(90f, 90f, 0f);
            mono.transform.rotation = Quaternion.Euler(rotationVector);
        }

        public override void Update()
        {
            if (target != null && target.transform.childCount > 0)
            {
                targetPosition = target.transform.GetChild(0).transform.position;

            }
            mono.transform.position = targetPosition + new Vector3(0f, 6f, 0f);
        }

        public override void End()
        {

        }
    }

    /// <summary>
    /// This state is made for sensor/robot camera configuration, will focus on whatever target object is, the target is default to the first node
    /// Works basically the same as orbit view but focus closer
    /// </summary>
    public class ConfigurationState : CameraState
    {
        Vector3 targetVector;
        Vector3 rotateVector;
        Vector3 lagVector;
        const float lagResponsiveness = 10f;
        float magnification = 2.0f;
        float cameraAngle = 45f;
        float panValue = 0f;
        bool isRobotCamera;
        GameObject target;

        public ConfigurationState(MonoBehaviour mono, GameObject targetObject = null)
        {
            this.mono = mono;
            if (robot == null) robot = GameObject.Find("Robot");
            this.target = targetObject;
            if (target == null) target = robot.transform.GetChild(0).gameObject;
        }
        public override void Init()
        {
        }

        public override void Update()
        {
            target = robot.transform.GetChild(0).gameObject;
            if (target != null)
            {
                targetVector = target.transform.position;
                if (MovingEnabled)
                {
                    if (Input.GetMouseButton(0))
                    {
                        cameraAngle = Mathf.Max(Mathf.Min(cameraAngle - Input.GetAxis("Mouse Y") * 5f, 90f), 0f);
                        panValue = -Input.GetAxis("Mouse X") / 5f;
                    }
                    else
                    {
                        panValue = 0f;

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

                rotateVector = rotateXZ(rotateVector, targetVector, panValue, magnification);
                rotateVector.y = targetVector.y + magnification * Mathf.Sin(cameraAngle * Mathf.Deg2Rad);

                lagVector = CalculateLagVector(lagVector, rotateVector, lagResponsiveness);

                mono.transform.position = lagVector;
                mono.transform.LookAt(targetVector);
            }
            else
            {
                target = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>().CurrentCamera;
            }

        }

        public override void End()
        {
        }

        Vector3 rotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
        {
            vector -= origin;
            Vector3 output = vector;
            output.x = Mathf.Cos(theta) * (vector.x) - Mathf.Sin(theta) * (vector.z);
            output.z = Mathf.Sin(theta) * (vector.x) + Mathf.Cos(theta) * (vector.z);

            return output.normalized * mag + origin;
        }
    }
    void Start()
    {
        SwitchCameraState(new OrbitState(this));
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
            else if (currentCameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new FreeroamState(this));
            else if (currentCameraState.GetType().Equals(typeof(FreeroamState))) SwitchCameraState(new OverviewState(this));
            else if (currentCameraState.GetType().Equals(typeof(OverviewState))) SwitchCameraState(new DriverStationState(this));
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

    public void SwitchCameraState(int type)
    {
        if (type == 0) SwitchCameraState(new FreeroamState(this));
        else if (type == 1) SwitchCameraState(new OrbitState(this));
        else SwitchCameraState(new DriverStationState(this, false));
    }

    public void SwitchToState(CameraState targetState)
    {
        if (targetState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new DriverStationState(this));
        else if (targetState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new OrbitState(this));
        else if (targetState.GetType().Equals(typeof(FreeroamState))) SwitchCameraState(new FreeroamState(this));
        else if (targetState.GetType().Equals(typeof(OverviewState))) SwitchCameraState(new OverviewState(this));
    }
}
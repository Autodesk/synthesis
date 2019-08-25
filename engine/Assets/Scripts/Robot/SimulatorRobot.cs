using BulletSharp;
using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.Camera;
using Synthesis.Configuration;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.MixAndMatch;
using Synthesis.RN;
using Synthesis.Sensors;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Synthesis.Robot
{
    public class SimulatorRobot : RobotBase
    {
        /// <summary>
        /// If true, the robot is in the process of resetting.
        /// </summary>
        public bool IsResetting { get; private set; } = false;

        public string FilePath { get; set; }

        private const float ResetVelocity = 5f;
        private const float HoldTime = 0.8f;

        private readonly SensorManager sensorManager;

        private DriverPracticeRobot dpmRobot;

        public Vector3 nodeToRobotOffset;

        private float keyDownTime = 0f;
        private RobotCameraManager robotCameraManager;

        private GameObject resetMoveArrows;

        private DynamicCamera.CameraState lastCameraState;

        private MainState state;

        GameObject toolbar;
        GameObject canvas;
        GameObject resetCanvas;

        /// <summary>
        /// Links this instance to the <see cref="MainState"/> state.
        /// </summary>
        private void Awake()
        {
            //Process.Start("\"C:\\Program Files\\qemu\\qemu-system-arm.exe\" (qemu-system-arm -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel \"C:\\Program Files\\Autodesk\\Synthesis\\Emulator\\zImage\" \"C:\\Program Files\\Autodesk\\Synthesis\\Emulator\\zynq-zed.dtb\" -display none -serial null -serial mon:stdio -localtime -append \"console = ttyPS0, 115200 earlyprintk root =/ dev / mmcblk0\" -redir tcp:10022::22  -redir tcp:11000::11000 -redir tcp:11001::11001 -redir tcp:2354::2354 -sd \"C:\\Program Files\\Autodesk\\Synthesis\\Emulator\\rootfs.ext4\"");

            StateMachine.SceneGlobal.Link<MainState>(this);
        }

        /// <summary>
        /// Creates a reference to the <see cref="MainState"/> instance.
        /// </summary>
        private void OnEnable()
        {
            state = StateMachine.SceneGlobal.CurrentState as MainState;
        }

        /// <summary>
        /// Initializes sensors and driver practice data.
        /// </summary>
        protected override void OnInitializeRobot()
        {
            //Detach and destroy all sensors on the original robot
            SensorManager sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            sensorManager.ResetSensorLists();

            //Removes Driver Practice component if it exists
            if (dpmRobot != null)
                Destroy(dpmRobot);
        }

        /// <summary>
        /// Called just after the robot is constructed.
        /// </summary>
        protected override void OnRobotSetup()
        {
            //Get the offset from the first node to the robot for new robot start position calculation
            //This line is CRITICAL to new reset position accuracy! DON'T DELETE IT!
            nodeToRobotOffset = gameObject.transform.GetChild(0).localPosition - robotStartPosition;

            //Initializes Driver Practice component
            dpmRobot = gameObject.AddComponent<DriverPracticeRobot>();

            //Initializing robot cameras
            bool hasRobotCamera = false;
            //If you are getting an error referencing this line, it is likely that the Game Object "RobotCameraList" in Scene.unity does not have the RobotCameraManager script attached to it.
            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();

            //Loop through robotCameraList and check if any existing camera should attach to this robot
            foreach (GameObject robotCamera in robotCameraManager.GetRobotCameraList())
            {
                if (robotCamera.GetComponent<RobotCamera>().robot.Equals(this))
                {
                    //Recover the robot camera configurations
                    robotCamera.GetComponent<RobotCamera>().RecoverConfiguration();
                    hasRobotCamera = true;
                }
            }

            //Add new cameras to the robot if there is none robot camera belong to the current robot (which means it is a new robot)
            if (!hasRobotCamera)
            {
                //Attached to the main frame and face the front
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0));
                ////Attached to main frame and face the back
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 180, 0));
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform);
            }
        }

        /// <summary>
        /// Generates the robot from the list of <see cref="RigidNode_Base"/>s and the
        /// number of wheels, and updates the collective mass.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numWheels"></param>
        /// <param name="collectiveMass"></param>
        /// <returns></returns>
        protected override bool ConstructRobot(List<RigidNode_Base> nodes, ref float collectiveMass)
        {
            if (!base.ConstructRobot(nodes, ref collectiveMass))
                return false;

            foreach (RigidNode_Base n in nodes)
            {
                RigidNode node = (RigidNode)n;

                if (node.MainObject.GetComponent<BRigidBody>() != null)
                    node.MainObject.AddComponent<Tracker>().Trace = true;
            }

            return true;
        }

        /// <summary>
        /// Updates positional information of the robot.
        /// </summary>
        protected override void UpdateTransform()
        {
            base.UpdateTransform();

            // TODO: Utilize the state machine here if possible

            if (IsResetting)
            {
                BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

                if (!rigidBody.GetCollisionObject().IsActive)
                    rigidBody.GetCollisionObject().Activate();

                Resetting();
            }
            else if (InputControl.GetButtonDown(Controls.Players[ControlIndex].GetButtons().resetRobot))
            {
                keyDownTime = Time.time;
            }
            else if (InputControl.GetButtonDown(Controls.Global.GetButtons().resetField))
            {
                Auxiliary.FindObject(GameObject.Find("Canvas"), "LoadingPanel").SetActive(true);
                MainState.timesLoaded--;
                SceneManager.LoadScene("Scene");

                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Playing,
                    AnalyticsLedger.TimingLabel.ChangeField);
            }
            else if (InputControl.GetButton(Controls.Players[ControlIndex].GetButtons().resetRobot) &&
                !state.DynamicCameraObject.GetComponent<DynamicCamera>().ActiveState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
            {
                if (Time.time - keyDownTime > HoldTime)
                    BeginReset();
            }
            else if (InputControl.GetButtonUp(Controls.Players[ControlIndex].GetButtons().resetRobot))
            {
                BeginReset();
                EndReset();
            }
        }

        /// <summary>
        /// Updates the physics of the robot.
        /// </summary>
        protected override void UpdatePhysics()
        {
            var begin = DateTime.Now;
            base.UpdatePhysics();

            if (!state.IsMetric)
            {
                Speed = (float)Math.Round(Speed * 3.28084, 3);
                Acceleration = (float)Math.Round(Acceleration * 3.28084, 3);
                Weight = (float)Math.Round(Weight * 2.20462, 3);
            }

            if (GameObject.Find("Field") != null) {
                if (gameObject.transform.GetChild(0).position.y < GameObject.Find("Field").transform.position.y - 2) {
                    if (robotStartPosition.y < GameObject.Find("Field").transform.position.y) robotStartPosition.y = GameObject.Find("Field").transform.position.y + 1.25f;
                    BeginReset();
                    EndReset();
                }
            }

            #region Encoder Calculations
            foreach (EmuNetworkInfo a in emuList)
            {
                RigidNode rigidNode = null;

                try
                {
                    rigidNode = (RigidNode)(a.wheel);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.StackTrace);
                }

                BRaycastWheel bRaycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();
            }
            #endregion
        }

        /// <summary>
        /// Returns the robot to a default starting spawnpoint
        /// </summary>
        public void BeginRevertSpawnpoint()
        {
            robotStartPosition = new Vector3(0f, 1f, 0f);
            state.BeginRobotReset();
            state.EndRobotReset();
            state.BeginRobotReset();
        }

        /// <summary>
        /// Return the robot to robotStartPosition and destroy extra game pieces
        /// </summary>
        /// <param name="resetTransform"></param>
        public void BeginReset()
        {
            //GetDriverPractice().DestroyAllGamepieces();

            InputControl.freeze = true;
            if (canvas == null) canvas = GameObject.Find("Canvas");
            if (resetCanvas == null) resetCanvas = Auxiliary.FindObject(UnityEngine.Camera.main.gameObject, "ResetRobotSpawnpointUI");
            canvas.GetComponent<Canvas>().enabled = false;
            resetCanvas.SetActive(true);

            #region init
            if (toolbar == null) toolbar = Auxiliary.FindObject(resetCanvas, "ResetStateToolbar");
            #endregion

            Button resetButton = Auxiliary.FindObject(resetCanvas, "ResetButton").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(BeginRevertSpawnpoint);
            Button returnButton = Auxiliary.FindObject(resetCanvas, "ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(EndReset);

            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            lastCameraState = dynamicCamera.ActiveState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));

            foreach (SimulatorRobot robot in state.SpawnedRobots)
                foreach (BRigidBody rb in robot.GetComponentsInChildren<BRigidBody>())
                    if (rb != null && !rb.GetCollisionObject().IsActive)
                        rb.GetCollisionObject().Activate();

            if (!state.DynamicCameraObject.GetComponent<DynamicCamera>().ActiveState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
            {
                IsResetting = true;

                BeginRobotReset();
                OnBeginReset();

                //Where "save orientation" works
                RotateRobot(robotStartOrientation);

                AttachMoveArrows();
            }
            else
            {
                UserMessageManager.Dispatch("Please don't reset robot during configuration!", 5f);
            }
        }

        /// <summary>
        /// Can move robot around in this state using WASD, update robotStartPosition if hit enter
        /// </summary>
        void Resetting()
        {
            if (UnityEngine.Input.GetMouseButton(1))
            {
                //Transform rotation along the horizontal plane
                Vector3 rotation = new Vector3(0f,
                    UnityEngine.Input.GetKey(KeyCode.D) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.A) ? -ResetVelocity : 0f,
                    0f) * Time.deltaTime;
                if (!rotation.Equals(Vector3.zero))
                    RotateRobot(rotation);
            }
            else
            {
                //Transform position
                Vector3 transposition = new Vector3(
                    UnityEngine.Input.GetKey(KeyCode.W) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.S) ? -ResetVelocity : 0f,
                    0f,
                    UnityEngine.Input.GetKey(KeyCode.A) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.D) ? -ResetVelocity : 0f) * Time.deltaTime;

                if (!transposition.Equals(Vector3.zero))
                    TranslateRobot(transposition);
            }

            //Update robotStartPosition when hit enter
            if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                robotStartOrientation = ((RigidNode)RootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
                robotStartPosition = transform.GetChild(0).transform.localPosition - nodeToRobotOffset;
                FieldDataHandler.robotSpawn = robotStartPosition;
                FieldDataHandler.WriteField();

                EndReset();
            }
            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape)) EndReset();
        }

        /// <summary>
        /// Attaches the movement arrows to the robot.
        /// </summary>
        private void AttachMoveArrows()
        {
            if (resetMoveArrows != null)
                Destroy(resetMoveArrows);

            resetMoveArrows = Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"),
                GetComponentInChildren<BRigidBody>().transform);
            resetMoveArrows.name = "ResetMoveArrows";
            resetMoveArrows.GetComponent<MoveArrows>().Translate = TranslateRobot;
        }

        /// <summary>
        /// End the reset process and puts the robot back down
        /// </summary>
        public void EndReset()
        {
            IsResetting = false;

            EndRobotReset();

            if (lastCameraState != null)
            {
                DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
                dynamicCamera.SwitchCameraState(lastCameraState);
                lastCameraState = null;
            }

            OnEndReset();

            Destroy(resetMoveArrows);
            resetMoveArrows = null;

            foreach (Tracker t in GetComponentsInChildren<Tracker>())
                t.Clear();

            InputControl.freeze = false;
            canvas.GetComponent<Canvas>().enabled = true;
            resetCanvas.SetActive(false);
        }

        /// <summary>
        /// Shifts the robot by a set position vector
        /// </summary>
        public void TranslateRobot(Vector3 transposition)
        {
            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                newTransform.Origin += transposition.ToBullet();
                r.WorldTransform = newTransform;
            }

            OnTransposeRobot(transposition);
        }

        /// <summary>
        /// Locks the robot in place.
        /// </summary>
        public void LockRobot()
        {
            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;
            }
        }

        /// <summary>
        /// If the robot is locked in place, unlocks the robot.
        /// </summary>
        public void UnlockRobot()
        {
            if (IsResetting)
                return;

            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
            }
        }

        /// <summary>
        /// Rotates the robot about its origin by a set vector
        /// </summary>
        public void RotateRobot(Vector3 rotation)
        {
            RotateRobot(BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.y, rotation.z, rotation.x));
        }

        /// <summary>
        /// Resets the robot orientation to how the CAD model was originally defined (should be standing upright and facing forward if CAD was done properly)
        /// </summary>
        public void ResetRobotOrientation()
        {
            robotStartOrientation = BulletSharp.Math.Matrix.Identity;
            BeginReset();
            EndReset();
        }

        /// <summary>
        /// Saves the robot's current orientation to be used whenever robot is reset
        /// </summary>
        public void SaveRobotOrientation()
        {
            robotStartOrientation = ((RigidNode)RootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
            robotStartOrientation.ToUnity();
        }

        /// <summary>
        /// Cancel current orientation & spawnpoint changes
        /// </summary>
        public void CancelRobotOrientation()
        {
            if (IsResetting)
            {
                BeginReset();
                EndReset();
            }
        }

        /// <summary>
        /// Returns the driver practice component of this robot
        /// </summary>
        public DriverPracticeRobot GetDriverPractice()
        {
            return GetComponent<DriverPracticeRobot>();
        }

        /// <summary>
        /// Called when resetting begins.
        /// </summary>
        protected virtual void OnBeginReset() { }

        /// <summary>
        /// Called when resetting ends.
        /// </summary>
        protected virtual void OnEndReset() { }

        /// <summary>
        /// Called when the robot is transposed.
        /// </summary>
        /// <param name="transposition"></param>
        protected virtual void OnTransposeRobot(Vector3 transposition) { }
    }
}

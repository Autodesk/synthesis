using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.Collections.Generic;
using BulletSharp.SoftBody;
using UnityEngine.SceneManagement;
using System.IO;
using Synthesis.FEA;
using Synthesis.FSM;
using System.Linq;
using UnityEngine.UI;
using Synthesis.BUExtensions;
using Synthesis.DriverPractice;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.MixAndMatch;
using Synthesis.Camera;
using Synthesis.Sensors;
using Synthesis.StatePacket;
using Synthesis.Utils;
using Synthesis.Robot;

namespace Synthesis.States
{
    /// <summary>
    /// This is the main class of the simulator; it handles all the initialization of robot and field objects within the simulator.
    /// Handles replay tracking and loading
    /// Handles interfaces between the SimUI and the active robot such as resetting, orienting, etc.
    /// </summary>
    public class MainState : State
    {
        private const int SolverIterations = 100;

        private BPhysicsWorld physicsWorld;
        private int lastFrameCount;

        public bool Tracking { get; private set; }
        private bool awaitingReplay;

        private UnityPacket unityPacket;

        // TODO: Create more robot classes that suit the needs of MainState.
        public SimulatorRobot ActiveRobot { get; private set; }

        private DynamicCamera dynamicCamera;
        public GameObject DynamicCameraObject;

        private RobotCameraManager robotCameraManager;

        private SensorManager sensorManager;
        private SensorManagerGUI sensorManagerGUI;

        private GameObject fieldObject;
        private UnityFieldDefinition fieldDefinition;

        public CollisionTracker CollisionTracker { get; private set; }

        private string fieldPath;
        private string robotPath;

        public List<SimulatorRobot> SpawnedRobots { get; private set; }
        private const int MAX_ROBOTS = 6;

        public bool IsMetric;

        /// <summary>
        /// Called when the script instance is being initialized.
        /// Initializes the bullet physics environment
        /// </summary>
        public override void Awake()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
            BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
            BPhysicsWorld.Get().DoDebugDraw = false;
            ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;

            CollisionTracker = new CollisionTracker(this);
            unityPacket = new UnityPacket();
            SpawnedRobots = new List<SimulatorRobot>();
        }

        /// <summary>
        /// Called after Awake() when the script instance is enabled.
        /// Initializes variables then loads the field and robot as well as setting up replay features.
        /// </summary>
        public override void Start()
        {
            AppModel.ClearError();

            //getting bullet physics information
            physicsWorld = BPhysicsWorld.Get();
            ((DynamicsWorld)physicsWorld.world).SetInternalTickCallback(BPhysicsTickListener.Instance.PhysicsTick);
            lastFrameCount = physicsWorld.frameCount;

            //setting up raycast robot tick callback
            BPhysicsTickListener.Instance.OnTick -= BRobotManager.Instance.UpdateRaycastRobots;
            BPhysicsTickListener.Instance.OnTick += BRobotManager.Instance.UpdateRaycastRobots;

            //starts a new instance of unity packet which receives packets from the driver station
            unityPacket.Start();

            //loads all the controls
            Controls.Load();

            //If a replay has been selected, load the replay. Otherwise, load the field and robot.
            string selectedReplay = PlayerPrefs.GetString("simSelectedReplay");

            if (string.IsNullOrEmpty(selectedReplay))
            {
                Tracking = true;

                if (!LoadField(PlayerPrefs.GetString("simSelectedField")))
                {
                    AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
                    return;
                }

                if (!LoadRobot(PlayerPrefs.GetString("simSelectedRobot"), RobotTypeManager.IsMixAndMatch))
                {
                    AppModel.ErrorToMenu("Could not load robot: " + PlayerPrefs.GetString("simSelectedRobot") + "\nHas it been moved or deleted?)");
                    return;
                }

                if (RobotTypeManager.IsMixAndMatch && RobotTypeManager.HasManipulator)
                {
                    Debug.Log(LoadManipulator(RobotTypeManager.ManipulatorPath) ? "Load manipulator success" : "Load manipulator failed");
                }
            }
            else
            {
                awaitingReplay = true;
                LoadReplay(selectedReplay);
            }

            //initializes the dynamic camera
            DynamicCameraObject = GameObject.Find("Main Camera");
            dynamicCamera = DynamicCameraObject.AddComponent<DynamicCamera>();
            DynamicCamera.MovingEnabled = true;

            sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            sensorManagerGUI = StateMachine.gameObject.GetComponent<SensorManagerGUI>();

            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();

            IsMetric = PlayerPrefs.GetString("Measure").Equals("Metric") ? true : false;

            StateMachine.Link<MainState>(GameObject.Find("Main Camera").transform.GetChild(0).gameObject);
            StateMachine.Link<ReplayState>(Auxiliary.FindGameObject("ReplayUI"));
            StateMachine.Link<SaveReplayState>(Auxiliary.FindGameObject("SaveReplayUI"));
        }

        /// <summary>
        /// Called every step of the program to listen to input commands for various features
        /// </summary>
        public override void Update()
        {
            if (ActiveRobot == null)
            {
                AppModel.ErrorToMenu("Robot instance not valid.");
                return;
            }

            //Spawn a new robot from the same path or switch active robot
            if (!ActiveRobot.IsResetting)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.U) && !MixAndMatchMode.setPresetPanelOpen) LoadRobot(robotPath, ActiveRobot is MaMRobot);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Y)) SwitchActiveRobot();
            }

            // Toggles between the different camera states if the camera toggle button is pressed
            if ((InputControl.GetButtonDown(Controls.buttons[0].cameraToggle)) && !MixAndMatchMode.setPresetPanelOpen)
            {
                if (DynamicCameraObject.activeSelf && DynamicCamera.MovingEnabled)
                {
                    dynamicCamera.ToggleCameraState(dynamicCamera.cameraState);
                }
            }

            // Switches to replay mode
            if (!ActiveRobot.IsResetting && UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            //if (!ActiveRobot.IsResetting && InputControl.GetButtonDown(Controls.buttons[controlIndex].replayMode))
            {
                CollisionTracker.ContactPoints.Add(null);
                StateMachine.PushState(new ReplayState(fieldPath, CollisionTracker.ContactPoints));
            }
        }

        /// <summary>
        /// Called at a fixed rate - updates robot packet information.
        /// </summary>
        public override void FixedUpdate()
        {
            //This line is essential for the reset to work accurately
            //robotCameraObject.transform.position = activeRobot.transform.GetChild(0).transform.position;
            if (ActiveRobot == null)
            {
                AppModel.ErrorToMenu("Robot instance not valid.");
                return;
            }

            SendRobotPackets();
        }

        /// <summary>
        /// If a replay has been loaded, this is called at the end of the initialization process to switch to the replay state
        /// </summary>
        public override void LateUpdate()
        {
            if (awaitingReplay)
            {
                awaitingReplay = false;
                StateMachine.PushState(new ReplayState(fieldPath, CollisionTracker.ContactPoints));
            }
        }

        /// <summary>
        /// Loads the field from a given directory
        /// </summary>
        /// <param name="directory">field directory</param>
        /// <returns>whether the process was successful</returns>
        bool LoadField(string directory)
        {
            fieldPath = directory;

            fieldObject = new GameObject("Field");

            FieldDefinition.Factory = delegate (Guid guid, string name)
            {
                return new UnityFieldDefinition(guid, name);
            };

            if (!File.Exists(directory + "\\definition.bxdf"))
                return false;

            string loadResult;
            fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + "\\definition.bxdf", out loadResult);
            Debug.Log(loadResult);
            fieldDefinition.CreateTransform(fieldObject.transform);
            return fieldDefinition.CreateMesh(directory + "\\mesh.bxda");
        }

        /// <summary>
        /// Loads a new robot from a given directory
        /// </summary>
        /// <param name="directory">robot directory</param>
        /// <returns>whether the process was successful</returns>
        public bool LoadRobot(string directory, bool isMixAndMatch)
        {
            if (SpawnedRobots.Count < MAX_ROBOTS)
            {
                if (isMixAndMatch)
                    robotPath = RobotTypeManager.RobotPath;
                else
                    robotPath = directory;

                GameObject robotObject = new GameObject("Robot");
                SimulatorRobot robot;

                if (isMixAndMatch)
                {
                    MaMRobot mamRobot = robotObject.AddComponent<MaMRobot>();
                    mamRobot.RobotHasManipulator = false; // Defaults to false
                    robot = mamRobot;
                }
                else
                {
                    robot = robotObject.AddComponent<SimulatorRobot>();
                }

                //Initialiezs the physical robot based off of robot directory. Returns false if not sucessful
                if (!robot.InitializeRobot(robotPath))
                    return false;

                //If this is the first robot spawned, then set it to be the active robot and initialize the robot camera on it
                if (ActiveRobot == null)
                {
                    ActiveRobot = robot;
                }

                robot.ControlIndex = SpawnedRobots.Count;
                SpawnedRobots.Add(robot);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes the active robot to a new robot with a given directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>whether the process was successful</returns>
        public bool ChangeRobot(string directory, bool isMixAndMatch)
        {
            sensorManager.RemoveSensorsFromRobot(ActiveRobot);
            sensorManagerGUI.ShiftOutputPanels();
            sensorManagerGUI.EndProcesses();

            RemoveRobot(SpawnedRobots.IndexOf(ActiveRobot));

            if (LoadRobot(directory, isMixAndMatch))
            {
                dynamicCamera.cameraState.robot = ActiveRobot.gameObject;
                DynamicCamera.MovingEnabled = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to delete manipulator nodes in MaM mode
        /// </summary>
        public void DeleteManipulatorNodes()
        {
            MaMRobot mamRobot = ActiveRobot as MaMRobot;
            mamRobot?.DeleteManipulatorNodes();
        }

        /// <summary>
        /// Changes the active robot from the current one to the next one in the list
        /// </summary>
        private void SwitchActiveRobot()
        {
            if (SpawnedRobots.Count >= 1)
            {
                if (ActiveRobot != null)
                {
                    int index = SpawnedRobots.IndexOf(ActiveRobot);

                    if (index < SpawnedRobots.Count - 1)
                        ActiveRobot = SpawnedRobots[index + 1];
                    else
                        ActiveRobot = SpawnedRobots[0];
                }
                else
                {
                    ActiveRobot = SpawnedRobots[0];
                }

                dynamicCamera.cameraState.robot = ActiveRobot.gameObject;
            }
        }

        /// <summary>
        /// Changes the active robot to a different robot based on a given index
        /// </summary>
        public void SwitchActiveRobot(int index)
        {
            if (index < SpawnedRobots.Count)
            {
                ActiveRobot = SpawnedRobots[index];
                dynamicCamera.cameraState.robot = ActiveRobot.gameObject;
            }
        }

        /// <summary>
        /// Changes the control index of the active robot
        /// </summary>
        public void ChangeControlIndex(int index)
        {
            ActiveRobot.ControlIndex = index;
        }

        /// <summary>
        /// If there are two robots or more, remove and delete the robot at that index
        /// </summary>
        public void RemoveRobot(int index)
        {
            if (index < SpawnedRobots.Count)
            {
                robotCameraManager.RemoveCamerasFromRobot(SpawnedRobots[index]);
                sensorManager.RemoveSensorsFromRobot(SpawnedRobots[index]);

                MaMRobot mamRobot = SpawnedRobots[index] as MaMRobot;

                if (mamRobot != null && mamRobot.RobotHasManipulator)
                    UnityEngine.Object.Destroy(mamRobot.ManipulatorObject);

                UnityEngine.Object.Destroy(SpawnedRobots[index].gameObject);
                SpawnedRobots.RemoveAt(index);
                ActiveRobot = null;
                SwitchActiveRobot();

                int i = 0;
                foreach (RobotBase robot in SpawnedRobots)
                {
                    robot.ControlIndex = i;
                    i++;
                }
            }
        }


        #region Replay Functions
        /// <summary>
        /// Loads the replay from the given replay file name.
        /// </summary>
        /// <param name="name"></param>
        void LoadReplay(string name)
        {
            List<FixedQueue<StateDescriptor>> fieldStates;
            List<KeyValuePair<string, List<FixedQueue<StateDescriptor>>>> robotStates;
            Dictionary<string, List<FixedQueue<StateDescriptor>>> gamePieceStates;
            List<List<KeyValuePair<ContactDescriptor, int>>> contacts;

            string fieldDirectory;

            ReplayImporter.Read(name, out fieldDirectory, out fieldStates, out robotStates, out gamePieceStates, out contacts);

            if (!LoadField(fieldDirectory))
            {
                AppModel.ErrorToMenu("Could not load field: " + fieldDirectory + "\nHas it been moved or deleted?");
                return;
            }

            foreach (KeyValuePair<string, List<FixedQueue<StateDescriptor>>> rs in robotStates)
            {
                if (!LoadRobot(rs.Key, false))
                {
                    AppModel.ErrorToMenu("Could not load robot: " + rs.Key + "\nHas it been moved or deleted?");
                    return;
                }

                int j = 0;

                foreach (Tracker t in SpawnedRobots.Last().GetComponentsInChildren<Tracker>())
                {
                    t.States = rs.Value[j];
                    j++;
                }
            }

            Tracker[] fieldTrackers = fieldObject.GetComponentsInChildren<Tracker>();

            int i = 0;

            foreach (Tracker t in fieldTrackers)
            {
                t.States = fieldStates[i];
                i++;
            }

            foreach (KeyValuePair<string, List<FixedQueue<StateDescriptor>>> k in gamePieceStates)
            {
                GameObject referenceObject = GameObject.Find(k.Key);

                if (referenceObject == null)
                    continue;

                foreach (FixedQueue<StateDescriptor> f in k.Value)
                {
                    GameObject currentPiece = UnityEngine.Object.Instantiate(referenceObject);
                    currentPiece.name = k.Key + "(Clone)";
                    currentPiece.GetComponent<Tracker>().States = f;
                }
            }

            foreach (var c in contacts)
            {
                if (c != null)
                {
                    List<ContactDescriptor> currentContacts = new List<ContactDescriptor>();

                    foreach (var d in c)
                    {
                        ContactDescriptor currentContact = d.Key;
                        currentContact.RobotBody = ActiveRobot.transform.GetChild(d.Value).GetComponent<BRigidBody>();
                        currentContacts.Add(currentContact);
                    }

                    CollisionTracker.ContactPoints.Add(currentContacts);
                }
                else
                {
                    CollisionTracker.ContactPoints.Add(null);
                }
            }
        }

        /// <summary>
        /// Loads a manipulator for Mix and Match mode and maps it to the robot. 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public bool LoadManipulator(string directory)
        {
            MaMRobot mamRobot = ActiveRobot as MaMRobot;

            if (mamRobot == null)
                return false;

            mamRobot.RobotHasManipulator = true;
            return mamRobot.InitializeManipulator(directory);
        }

        /// <summary>
        /// Loads a new robot and manipulator from given directorys
        /// </summary>
        /// <param name="directory">robot directory</param>
        /// <returns>whether the process was successful</returns>
        public bool LoadRobotWithManipulator(string baseDirectory, string manipulatorDirectory)
        {
            if (SpawnedRobots.Count >= MAX_ROBOTS)
                return false;

            robotPath = baseDirectory;

            GameObject robotObject = new GameObject("Robot");
            MaMRobot robot = robotObject.AddComponent<MaMRobot>();

            //Initialiezs the physical robot based off of robot directory. Returns false if not sucessful
            if (!robot.InitializeRobot(baseDirectory)) return false;

            robotObject.AddComponent<DriverPracticeRobot>().Initialize(baseDirectory);

            //If this is the first robot spawned, then set it to be the active robot and initialize the robot camera on it
            if (ActiveRobot == null)
                ActiveRobot = robot;

            robot.ControlIndex = SpawnedRobots.Count;
            SpawnedRobots.Add(robot);

            return robot.InitializeManipulator(manipulatorDirectory);
        }

        /// <summary>
        /// Resumes the normal simulation and exits the replay mode, showing all UI elements again
        /// </summary>
        public override void Resume()
        {
            lastFrameCount = physicsWorld.frameCount;
            Tracking = true;

            if (!awaitingReplay)
                CollisionTracker.Reset();
        }

        /// <summary>
        /// Pauses the normal simulation for rpelay mode by disabling tracking of physics objects and disabling UI elements
        /// </summary>
        public override void Pause()
        {
            Tracking = false;
        }

        /// <summary>
        /// Starts the replay state.
        /// </summary>
        public void EnterReplayState()
        {
            if (!ActiveRobot.IsResetting)
            {
                CollisionTracker.ContactPoints.Add(null);
                StateMachine.PushState(new ReplayState(fieldPath, CollisionTracker.ContactPoints));
            }
            else
            {
                UserMessageManager.Dispatch("Please finish resetting before entering replay mode!", 5f);
            }
        }
        #endregion


        #region Robot Interaction Functions

        /// <summary>
        /// Starts the resetting process of the active robot
        /// </summary>
        public void BeginRobotReset()
        {
            ActiveRobot.BeginReset();
        }

        /// <summary>
        /// Ends the restting process of the active robot and resets the replay tracking objects
        /// </summary>
        public void EndRobotReset()
        {
            ActiveRobot.EndReset();
            foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
            {
                t.Clear();

                CollisionTracker.Reset();
            }
        }

        /// <summary>
        /// Shifts the active robot by a set transposition vector
        /// </summary>
        public void TransposeRobot(Vector3 transposition)
        {
            ActiveRobot.TransposeRobot(transposition);
        }

        /// <summary>
        /// Rotates the active robot about its origin by a mathematical 4x4 matrix
        /// </summary>
        public void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
        {
            ActiveRobot.RotateRobot(rotationMatrix);
        }

        /// <summary>
        /// Rotates the active robot about its origin by a set vector
        /// </summary>
        public void RotateRobot(Vector3 rotation)
        {
            ActiveRobot.RotateRobot(rotation);
        }

        /// <summary>
        /// Resets the active robot orientation to how the CAD model was originally defined (should be standing upright and facing forward if CAD was done properly)
        /// </summary>
        public void ResetRobotOrientation()
        {
            ActiveRobot.ResetRobotOrientation();
        }

        /// <summary>
        /// Saves the active robot's current orientation to be used whenever robot is reset
        /// </summary>
        public void SaveRobotOrientation()
        {
            ActiveRobot.SaveRobotOrientation();
        }

        /// <summary>
        /// Cancels the active robot's unsaved orientation changes
        /// </summary>
        public void CancelRobotOrientation()
        {
            ActiveRobot.CancelRobotOrientation();
        }
        /// <summary>
        /// Sends the received packets to the active robot
        /// </summary>
        private void SendRobotPackets()
        {
            ActiveRobot.Packet = unityPacket.GetLastPacket();
            foreach (RobotBase robot in SpawnedRobots)
            {
                if (robot != ActiveRobot) robot.Packet = null;
            }
        }
        #endregion
    }
}
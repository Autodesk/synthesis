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
using Synthesis.Utils;
using Synthesis.Robot;
using Synthesis.Field;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Newtonsoft.Json;
using Assets.Scripts;

namespace Synthesis.States
{
    /// <summary>
    /// This is the main class of the simulator; it handles all the initialization of robot and field objects within the simulator.
    /// Handles replay tracking and loading
    /// Handles interfaces between the SimUI and the active robot such as resetting, orienting, etc.
    /// </summary>
    public class MainState : State, IRobotProvider
    {
        private string[] SampleRobotGUIDs = { "ee85355c-6daf-4588-ba47-cdf3f9143922", "fde5a9e9-4a1d-4d07-bafd-ae18bada7a8d", "d7f2959a-f9eb-4581-a4bb-898550193bda", "d1859211-db0f-4b75-866c-2d0e81b6732b", "52eb1ada-b051-461a-9cc4-1b5b74764ce5", "decdc6a1-5f76-4dea-add7-4c358f4a9921", "6b5d4484-db3c-425b-98b8-546c06d8d8bf", "c3bb1b94-dad8-4a8c-aa67-9c09eb9379c1", "ef4e3e2b-8cfb-437d-b63d-8bebc05fa3ba", "7d31cb8a-01e8-4eeb-9086-2955a993a374", "1478855a-60bd-42cb-8841-eece4fa0fbeb", "0b43729a-d8d3-4df2-bcbb-684343933c23", "9f19586c-a26f-4b28-9fb9-e06731178166", "f1225b7a-180e-456b-88d1-7315b0086001" };

        public string robotDirectory;

        public static int timesLoaded = 0;

        private const int SolverIterations = 100;

        private BPhysicsWorld physicsWorld;
        private int lastFrameCount;

        public bool Tracking { get; private set; }
        private bool awaitingReplay;

        /// <summary>
        /// The active robot in this state.
        /// </summary>
        public SimulatorRobot ActiveRobot { get; private set; }

        /// <summary>
        /// Used for accessing the active robot in this state.
        /// </summary>
        /// <returns></returns>
        public GameObject Robot => ActiveRobot.transform.GetChild(0).gameObject ?? ActiveRobot.gameObject;

        /// <summary>
        /// True if the robot is not resetting.
        /// </summary>
        public bool RobotActive => !ActiveRobot.IsResetting;

        private DynamicCamera dynamicCamera;
        public GameObject DynamicCameraObject;

        private RobotCameraManager robotCameraManager;

        private SensorManager sensorManager;
        private SensorManagerGUI sensorManagerGUI;

        private SimUI simUI;

        private GameObject fieldObject;
        private UnityFieldDefinition fieldDefinition;

        public CollisionTracker CollisionTracker { get; private set; }

        private string fieldPath;
        private string robotPath;

        public List<SimulatorRobot> SpawnedRobots { get; private set; }
        private const int MAX_ROBOTS = 6;

        public bool IsMetric;
        public bool isEmulationDownloaded = File.Exists(EmulatorManager.emulationDir + "kernel-native") &&
            File.Exists(EmulatorManager.emulationDir + "rootfs-native.ext4") &&
            File.Exists(EmulatorManager.emulationDir + "zynq-zed.dtb") &&
            File.Exists(EmulatorManager.emulationDir + "kernel-java") &&
            File.Exists(EmulatorManager.emulationDir + "rootfs-java.ext4") &&
            File.Exists(EmulatorManager.emulationDir + "grpc-bridge.exe");
        //public bool isEmulationDownloaded = true;

        bool reset;

        public static List<List<GameObject>> spawnedGamepieces = new List<List<GameObject>>() { new List<GameObject>(), new List<GameObject>() };
        /// <summary>
        /// Called when the script instance is being initialized.
        /// Initializes the bullet physics environment
        /// </summary>
        public override void Awake()
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("qualityLevel"));
            Screen.fullScreen = PlayerPrefs.GetInt("fullscreen", 1) == 1 ? true : false;

            string CurrentVersion = "4.3.0";
            GameObject.Find("VersionNumber").GetComponent<Text>().text = "Version " + CurrentVersion;

            if (CheckConnection())
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                var json = new WebClient().DownloadString("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json");
                VersionManager update = JsonConvert.DeserializeObject<VersionManager>(json);
                SimUI.updater = update.URL;

                var localVersion = new Version(CurrentVersion);
                var globalVersion = new Version(update.Version);

                var check = localVersion.CompareTo(globalVersion);

                if (check < 0)
                {
                    Auxiliary.FindGameObject("UpdatePrompt").SetActive(true);
                }
            }

            robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Robots"));
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
            //BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
            BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.All;
            BPhysicsWorld.Get().DoDebugDraw = false;
            ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;

            CollisionTracker = new CollisionTracker(this);
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

            //If a replay has been selected, load the replay. Otherwise, load the field and robot.
            string selectedReplay = PlayerPrefs.GetString("simSelectedReplay");

            if (string.IsNullOrEmpty(selectedReplay))
            {
                Tracking = true;

                if (timesLoaded > 0)
                {
                    if (!LoadField(PlayerPrefs.GetString("simSelectedField")))
                    {
                        AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
                        return;
                    }
                    else
                    {
                        MovePlane();
                    }
                }
                else
                {
                    timesLoaded++;
                }

                bool result = false;

                try
                {
                    result = LoadRobot(PlayerPrefs.GetString("simSelectedRobot"), false);
                }
                catch (Exception e)
                {
                    MonoBehaviour.Destroy(GameObject.Find("Robot"));
                }

                if (!result)
                {
                    AppModel.ErrorToMenu("ROBOT_SELECT|Could not find the selected robot");
                    return;
                }

                reset = FieldDataHandler.robotSpawn == new Vector3(99999, 99999, 99999);

                if (RobotTypeManager.IsMixAndMatch && RobotTypeManager.HasManipulator)
                {
                    Debug.Log(LoadManipulator(RobotTypeManager.ManipulatorPath) ? "Load manipulator success" : "Load manipulator failed");
                }
            }
            else
            {
                awaitingReplay = true;
                PlayerPrefs.SetString("simSelectedReplay", "");
                LoadReplay(selectedReplay);
            }

            //initializes the dynamic camera
            DynamicCameraObject = GameObject.Find("Main Camera");
            dynamicCamera = DynamicCameraObject.AddComponent<DynamicCamera>();
            DynamicCamera.ControlEnabled = true;

            sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            sensorManagerGUI = StateMachine.gameObject.GetComponent<SensorManagerGUI>();

            simUI = StateMachine.SceneGlobal.GetComponent<SimUI>();

            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();

            IsMetric = PlayerPrefs.GetString("Measure").Equals("Metric") ? true : false;

            StateMachine.Link<MainState>(GameObject.Find("Main Camera").transform.GetChild(0).gameObject);
            StateMachine.Link<MainState>(GameObject.Find("Main Camera").transform.GetChild(1).gameObject, false);
            StateMachine.Link<ReplayState>(Auxiliary.FindGameObject("ReplayUI"));
            StateMachine.Link<SaveReplayState>(Auxiliary.FindGameObject("SaveReplayUI"));
            StateMachine.Link<LoadReplayState>(Auxiliary.FindGameObject("LoadReplayPanel"));
            StateMachine.Link<GamepieceSpawnState>(Auxiliary.FindGameObject("ResetGamepieceSpawnpointUI"));
            StateMachine.Link<DefineNodeState>(Auxiliary.FindGameObject("DefineNodeUI"));
            StateMachine.Link<GoalState>(Auxiliary.FindGameObject("GoalStateUI"));
            StateMachine.Link<SensorSpawnState>(Auxiliary.FindGameObject("ResetSensorSpawnpointUI"));
            StateMachine.Link<DefineSensorAttachmentState>(Auxiliary.FindGameObject("DefineSensorAttachmentUI"));

            string defaultDirectory = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Emulator");
            string directoryPath = "";

            if (Directory.Exists(defaultDirectory))
            {
                directoryPath = defaultDirectory;
                isEmulationDownloaded = true;
            }

            MediaManager.getInstance();
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

            if (ActiveRobot.transform.GetChild(0).transform.position.y < -10 || ActiveRobot.transform.GetChild(0).transform.position.y > 60)
            {
                BeginRobotReset();
                EndRobotReset();
            }

            if (reset)
            {
                BeginRobotReset();
                reset = false;
            }

            //Spawn a new robot from the same path or switch active robot
            if (!ActiveRobot.IsResetting && ActiveRobot.ControlIndex == 0)
            {
                if (InputControl.GetButtonDown(Controls.Players[ActiveRobot.ControlIndex].GetButtons().duplicateRobot)) LoadRobot(robotPath, ActiveRobot is MaMRobot);
                if (InputControl.GetButtonDown(Controls.Players[ActiveRobot.ControlIndex].GetButtons().switchActiveRobot)) SwitchActiveRobot(SpawnedRobots.IndexOf(ActiveRobot) + 1 < SpawnedRobots.Count() ? SpawnedRobots.IndexOf(ActiveRobot) + 1 : 0);

            }

            // Toggles between the different camera states if the camera toggle button is pressed
            if ((InputControl.GetButtonDown(Controls.Global.GetButtons().cameraToggle)) &&
                DynamicCameraObject.activeSelf && DynamicCamera.ControlEnabled)
                dynamicCamera.ToggleCameraState(dynamicCamera.ActiveState);

            // Switches to replay mode
            if (!ActiveRobot.IsResetting && InputControl.GetButtonDown(Controls.Global.GetButtons().replayMode))
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

        public bool CheckConnection()
        {
            try
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

                using (client.OpenRead("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }

        public void MovePlane()
        {
            GameObject plane = GameObject.Find("Environment");
            MeshRenderer[] aLotOfMeshes = fieldObject.GetComponentsInChildren<MeshRenderer>();
            float lowPoint = 0;
            foreach (MeshRenderer singleMesh in aLotOfMeshes)
            {
                if (singleMesh.bounds.min.y < lowPoint)
                {
                    lowPoint = singleMesh.bounds.min.y;
                    Debug.Log("LowPoint: " + lowPoint);
                }
            }

            plane.transform.position = new Vector3(0, lowPoint, 0);
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

            if (!File.Exists(directory + Path.DirectorySeparatorChar + "definition.bxdf"))
                return false;

            FieldDataHandler.Load(fieldPath);
            timesLoaded++;

            Controls.Load();
            Controls.UpdateFieldControls();
            if (!Controls.HasBeenSaved())
                Controls.Save();

            fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + Path.DirectorySeparatorChar + "definition.bxdf", out string loadResult);
            Debug.Log(loadResult);
            fieldDefinition.CreateTransform(fieldObject.transform);
            return fieldDefinition.CreateMesh(directory + Path.DirectorySeparatorChar + "mesh.bxda");
        }

        /// <summary>
        /// Loads a new robot from a given directory
        /// </summary>
        /// <param name="directory">robot directory</param>
        /// <returns>whether the process was successful</returns>
        public bool LoadRobot(string directory, bool isMixAndMatch)
        {
            bool b = true;

            if (!Directory.Exists(directory))
            {
                return false;
            }
            else
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string a in files)
                {
                    string name = Path.GetFileName(a);
                    if (name.ToLower().Contains("skeleton"))
                    {
                        b = false;
                    }
                }
            }

            if (b)
            {
                return false;
            }

            if (SpawnedRobots.Count < MAX_ROBOTS)
            {
                GameObject robotObject = new GameObject("Robot");
                SimulatorRobot robot;

                if (isMixAndMatch)
                {
                    robotPath = RobotTypeManager.RobotPath;
                    MaMRobot mamRobot = robotObject.AddComponent<MaMRobot>();
                    mamRobot.RobotHasManipulator = false; // Defaults to false
                    robot = mamRobot;

                    if (AnalyticsManager.GlobalInstance != null)
                    {
                        AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.LoadRobot,
                            AnalyticsLedger.EventAction.Load,
                            "Robot - Mix and Match",
                            AnalyticsLedger.getMilliseconds().ToString());
                    }
                }
                else
                {
                    robotPath = directory;
                    robot = robotObject.AddComponent<SimulatorRobot>();

                    if (AnalyticsManager.GlobalInstance != null)
                    {
                        AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.LoadRobot,
                            AnalyticsLedger.EventAction.Load,
                            "Robot - Exported",
                            AnalyticsLedger.getMilliseconds().ToString());
                    }
                }

                robot.FilePath = robotPath;

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

                DPMDataHandler.Load(robotPath);

                if (!isMixAndMatch && !PlayerPrefs.HasKey(robot.RootNode.GUID.ToString()) && !SampleRobotGUIDs.Contains(robot.RootNode.GUID.ToString()))
                {
                    AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.LoadRobot,
                        AnalyticsLedger.EventAction.Load,
                        robot.RootNode.GUID.ToString(),
                        AnalyticsLedger.getMilliseconds().ToString());
                }

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
            //ActiveRobot = null;

            if (LoadRobot(directory, isMixAndMatch))
            {
                DynamicCamera.ControlEnabled = true;
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
        /// Changes the active robot to a different robot based on a given index
        /// </summary>
        public void SwitchActiveRobot(int index)
        {
            if (SpawnedRobots.Count() > 0)
            {
                ActiveRobot = SpawnedRobots[index];
                DPMDataHandler.Load(ActiveRobot.FilePath); //reload robot data to allow for driver practice for multiplayer
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
                //remove attached sensors/cameras
                robotCameraManager.RemoveCamerasFromRobot(SpawnedRobots[index]);
                sensorManager.RemoveSensorsFromRobot(SpawnedRobots[index]);

                MaMRobot mamRobot = SpawnedRobots[index] as MaMRobot;

                if (mamRobot != null && mamRobot.RobotHasManipulator)
                    UnityEngine.Object.Destroy(mamRobot.ManipulatorObject);

                UnityEngine.Object.Destroy(SpawnedRobots[index].gameObject);
                SpawnedRobots.RemoveAt(index);
                ActiveRobot = null;
                SwitchActiveRobot(index < SpawnedRobots.Count() ? index : SpawnedRobots.Count() - 1); //switch to either old location or last active robot

                int i = 0;
                foreach (SimulatorRobot robot in SpawnedRobots)
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

            bool hasField = !string.IsNullOrEmpty(fieldDirectory);

            if (hasField)
            {
                if (!LoadField(fieldDirectory))
                {
                    AppModel.ErrorToMenu("Could not load field: " + fieldDirectory + "\nHas it been moved or deleted?");
                    return;
                }
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

            if (hasField)
            {
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

            robot.FilePath = robotPath;

            //Initialiezs the physical robot based off of robot directory. Returns false if not sucessful
            if (!robot.InitializeRobot(baseDirectory)) return false;

            //If this is the first robot spawned, then set it to be the active robot and initialize the robot camera on it
            if (ActiveRobot == null)
                ActiveRobot = robot;

            robot.ControlIndex = SpawnedRobots.Count;
            SpawnedRobots.Add(robot);

            DPMDataHandler.Load(robotPath);
            return robot.InitializeManipulator(manipulatorDirectory);
        }

        /// <summary>
        /// Resumes the normal simulation and exits the replay mode, showing all UI elements again
        /// </summary>
        public override void Resume()
        {
            lastFrameCount = physicsWorld.frameCount;
            Tracking = true;

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
        /// Locks all <see cref="SimulatorRobot"/>s currently in the simulation.
        /// </summary>
        public void LockRobots()
        {
            foreach (SimulatorRobot robot in SpawnedRobots)
                robot.LockRobot();
        }

        /// <summary>
        /// Unlocks all <see cref="SimulatorRobot"/>s currently in the simulation.
        /// </summary>
        public void UnlockRobots()
        {
            foreach (SimulatorRobot robot in SpawnedRobots)
                robot.UnlockRobot();
        }

        public void RevertSpawnpoint()
        {
            ActiveRobot.BeginRevertSpawnpoint();
        }

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
        public void TranslateRobot(Vector3 transposition)
        {
            ActiveRobot.TranslateRobot(transposition);
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
        #endregion

        public UnityEngine.Camera GetCamera()
        {
            return DynamicCameraObject.GetComponent<UnityEngine.Camera>();
        }
    }
}
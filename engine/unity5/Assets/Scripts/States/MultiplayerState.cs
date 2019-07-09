//using UnityEngine;
//using System.Collections;
//using BulletUnity;
//using BulletSharp;
//using System;
//using System.Collections.Generic;
//using BulletSharp.SoftBody;
//using UnityEngine.SceneManagement;
//using System.IO;
//using System.Linq;
//using Assets.Scripts;
//using UnityEngine.UI;
//using UnityEngine.Networking;
//using Synthesis.FSM;
//using Synthesis.StatePacket;
//using Synthesis.Robot;
//using Synthesis.Utils;
//using Synthesis.BUExtensions;
//using Synthesis.Input;
//using Synthesis.Network;
//using Synthesis.GUI;
//using Synthesis.Field;
//using Assets.Scripts.GUI;

//namespace Synthesis.States
//{
//    /// <summary>
//    /// This is the main class of the simulator; it handles all the initialization of robot and field objects within the simulator.
//    /// Handles replay tracking and loading
//    /// Handles interfaces between the SimUI and the active robot such as resetting, orienting, etc.
//    /// </summary>
//    public class MultiplayerState : State, IRobotProvider
//    {
//        private const int SolverIterations = 100;

//        private BPhysicsWorld physicsWorld;

//        private UnityPacket unityPacket;

//        private List<NetworkRobot> robots;

//        /// <summary>
//        /// The active robot in this state.
//        /// </summary>
//        public NetworkRobot ActiveRobot { get; set; }

//        /// <summary>
//        /// Used for accessing the active robot in this state.
//        /// </summary>
//        /// <returns></returns>
//        public GameObject Robot => ActiveRobot?.GetComponentInChildren<NetworkMesh>()?.MeshObject ??
//            ActiveRobot?.transform.GetChild(0).gameObject;

//        /// <summary>
//        /// True if the scene's active robot is driveable by the user.
//        /// </summary>
//        public bool RobotActive => true;

//        private DynamicCamera dynamicCamera;
//        public GameObject DynamicCameraObject;

//        private GameObject fieldObject;
//        private UnityFieldDefinition fieldDefinition;

//        private string fieldPath;
//        private string robotPath;

//        public List<NetworkRobot> SpawnedRobots { get; private set; }
//        private const int MAX_ROBOTS = 6;

//        private GameObject tabCanvas;

//        private StateMachine homeTabStateMachine;

//        public MultiplayerNetwork Network { get; private set; }

//        enum NetworkMode
//        {
//            Disconnected,
//            Client,
//            Host
//        }

//        private NetworkMode clientNetworkMode;

//        public bool IsMetric;

//        /// <summary>
//        /// Called when the script instance is being initialized.
//        /// Initializes the bullet physics environment
//        /// </summary>
//        public override void Awake()
//        {
//            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
//            GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
//            BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
//            BPhysicsWorld.Get().DoDebugDraw = false;
//            ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;

//            clientNetworkMode = NetworkMode.Disconnected;
//        }

//        /// <summary>
//        /// Called after Awake() when the script instance is enabled.
//        /// Initializes variables then loads the field and robot as well as setting up replay features.
//        /// </summary>
//        public override void Start()
//        {
//            AppModel.ClearError();

//            //getting bullet physics information
//            physicsWorld = BPhysicsWorld.Get();
//            ((DynamicsWorld)physicsWorld.world).SetInternalTickCallback(BPhysicsTickListener.Instance.PhysicsTick);

//            //setting up raycast robot tick callback
//            BPhysicsTickListener.Instance.OnTick -= BRobotManager.Instance.UpdateRaycastRobots;
//            BPhysicsTickListener.Instance.OnTick += BRobotManager.Instance.UpdateRaycastRobots;

//            //starts a new instance of unity packet which receives packets from the driver station
//            unityPacket = new UnityPacket();
//            unityPacket.Start();

//            SpawnedRobots = new List<NetworkRobot>();

//            //initializes the dynamic camera
//            DynamicCameraObject = GameObject.Find("Main Camera");
//            dynamicCamera = DynamicCameraObject.AddComponent<DynamicCamera>();
//            DynamicCamera.ControlEnabled = true;

//            IsMetric = PlayerPrefs.GetString("Measure").Equals("Metric") ? true : false;

//            Network = GameObject.Find("NetworkManager").GetComponent<MultiplayerNetwork>();
//            Network.State = this;
//        }

//        /// <summary>
//        /// Called every step of the program to listen to input commands for various features
//        /// </summary>
//        public override void Update()
//        {
//            if (ActiveRobot == null)
//                return;

//            // Toggles between the different camera states if the camera toggle button is pressed
//            if ((InputControl.GetButtonDown(Controls.buttons[0].cameraToggle)))
//            {
//                if (DynamicCameraObject.activeSelf && DynamicCamera.ControlEnabled)
//                {
//                    dynamicCamera.ToggleCameraState(dynamicCamera.ActiveState);
//                }
//            }
//        }

//        /// <summary>
//        /// Called at a fixed rate - updates robot packet information.
//        /// </summary>
//        public override void FixedUpdate()
//        {
//            //This line is essential for the reset to work accurately
//            //robotCameraObject.transform.position = activeRobot.transform.GetChild(0).transform.position;
//            if (ActiveRobot == null)
//                return;

//            SendRobotPackets();
//        }

//        /// <summary>
//        /// Loads the field from a given directory
//        /// </summary>
//        /// <param name="directory">field directory</param>
//        /// <returns>whether the process was successful</returns>
//        public bool LoadField(string directory, bool host)
//        {
//            fieldPath = directory;

//            fieldObject = new GameObject("Field");

//            FieldDefinition.Factory = delegate (Guid guid, string name)
//            {
//                return new UnityFieldDefinition(guid, name);
//            };

//            if (!File.Exists(directory + Path.DirectorySeparatorChar + "definition.bxdf"))
//                return false;

//            FieldDataHandler.Load(fieldPath);
//            Controls.Load();

//            string loadResult;
//            fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + Path.DirectorySeparatorChar + "definition.bxdf", out loadResult);
//            Debug.Log(loadResult);
//            fieldDefinition.CreateTransform(fieldObject.transform);
//            return fieldDefinition.CreateMesh(directory + Path.DirectorySeparatorChar + "mesh.bxda", true, host);
//        }

//        /// <summary>
//        /// Loads a new robot from a given directory
//        /// </summary>
//        /// <param name="directory">robot directory</param>
//        /// <returns>whether the process was successful</returns>
//        public bool LoadRobot(NetworkRobot playerRobot, string directory, bool isLocal)
//        {
//            if (SpawnedRobots.Count < MAX_ROBOTS)
//            {
//                DynamicCamera.ControlEnabled = true;

//                //Initializes the physical robot based off of robot directory. Returns false if not sucessful
//                if (!playerRobot.InitializeRobot(directory)) return false;

//                //If this is the first robot spawned, then set it to be the active robot and initialize the robot camera on it
//                if (isLocal)
//                    ActiveRobot = playerRobot;
//                else
//                    playerRobot.ControlIndex = 5;

//                SpawnedRobots.Add(playerRobot);

//                return true;
//            }
//            return false;
//        }

//        /// <summary>
//        /// Sends the received packets to the active robot
//        /// </summary>
//        private void SendRobotPackets()
//        {
//            ActiveRobot.Packet = unityPacket.GetLastPacket();
//            foreach (NetworkRobot robot in SpawnedRobots)
//            {
//                if (robot != ActiveRobot) robot.Packet = null;
//            }
//        }
//    }
//}

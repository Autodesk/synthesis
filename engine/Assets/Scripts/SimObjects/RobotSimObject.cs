using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using Newtonsoft.Json;
using Synthesis;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using Synthesis.UI;
using Synthesis.WS.Translation;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Controller;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UI.Dynamic.Panels.Tooltip;
using UnityEngine;
using Utilities.ColorManager;
using Bounds   = UnityEngine.Bounds;
using Logger   = SynthesisAPI.Utilities.Logger;
using MVector3 = Mirabuf.Vector3;

#nullable enable

public class RobotSimObject : SimObject, IPhysicsOverridable, IGizmo {
    public const int MAX_ROBOTS = 6;

    public const string INTAKE_GAMEPIECES  = "input/intake";
    public const string OUTTAKE_GAMEPIECES = "input/shoot-gamepiece";

    private const float TIME_BETWEEN_SHOTS = 0.5f;
    public float LastShotTime              = 0;

    private static string _currentlyPossessedRobot = string.Empty;
    public static string CurrentlyPossessedRobot {
        get => _currentlyPossessedRobot;
        set {
            if (value != _currentlyPossessedRobot) {
                var old = _currentlyPossessedRobot;
                if (_currentlyPossessedRobot != string.Empty)
                    GetCurrentlyPossessedRobot().Unpossess();
                _currentlyPossessedRobot = value;
                if (_currentlyPossessedRobot != string.Empty)
                    GetCurrentlyPossessedRobot().Possess();

                EventBus.Push(new PossessionChangeEvent { NewBot = value, OldBot = old });
            }
        }
    }
    public static RobotSimObject GetCurrentlyPossessedRobot() => _currentlyPossessedRobot == string.Empty
                                                                     ? null
                                                                     : _spawnedRobots[_currentlyPossessedRobot];

    private static Dictionary<string, RobotSimObject> _spawnedRobots = new Dictionary<string, RobotSimObject>(); // Open
    public static Dictionary<string, RobotSimObject>.ValueCollection SpawnedRobots => _spawnedRobots.Values;

    private static Dictionary<ulong, ServerTransforms> _serverTransforms = new Dictionary<ulong, ServerTransforms>();
    public static Dictionary<ulong, ServerTransforms> ServerTransforms {
        get => _serverTransforms;
        set => _serverTransforms = value;
    }

    public static int ControllableJointCounter = 0;

    private CameraController cam;
    private OrbitCameraMode orbit;
    private ICameraMode previousMode;

    private LobbyClient? _client;
    public LobbyClient? Client {
        get => _client;
        set => _client = value;
    }

    private IEnumerable<WheelDriver>? _wheelDrivers;
    public (RotationalDriver azimuth, WheelDriver driver)[] modules;

    public string MiraGUID => MiraLive.MiraAssembly.Info.GUID;

    public MirabufLive MiraLive { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public Bounds GroundedBounds { get; private set; }
    public GameObject RobotNode { get; private set; } // Doesn't work??
    public Bounds RobotBounds { get; private set; }

    private WSSimBehavior _simBehaviour;
    private RioTranslationLayer _simulationTranslationLayer;
    public RioTranslationLayer SimulationTranslationLayer {
        get => _simulationTranslationLayer;
        set {
            _simulationTranslationLayer = value;
            _simBehaviour.Translation   = _simulationTranslationLayer;

            SimulationPreferences.SetRobotSimTranslationLayer(
                MiraLive.MiraAssembly.Info.GUID, _simulationTranslationLayer);
            PreferenceManager.Save();
        }
    }

    private bool _useSimBehaviour = false;
    public bool UseSimulationBehaviour {
        get => _useSimBehaviour;
        set {
            if (_useSimBehaviour != value) {
                _useSimBehaviour = value;
                SimulationManager.Behaviours[this._name].ForEach(b => {
                    // Kinda ugly but whatever
                    if (_useSimBehaviour ? b.GetType() != typeof(WSSimBehavior) : b.GetType() == typeof(WSSimBehavior))
                        b.Enabled = false;
                    else
                        b.Enabled = true;
                });
            }
        }
    }

    public SimBehaviour? DriveBehaviour { get; private set; }

    private (List<WheelDriver> leftWheels, List<WheelDriver> rightWheels)? _tankTrackWheels = null;

    private Dictionary<string, (Joint a, Joint b)> _jointMap;
    private List<Rigidbody> _allRigidbodies;
    public IReadOnlyCollection<Rigidbody> AllRigidbodies => _allRigidbodies.AsReadOnly();

    private Dictionary<string, GameObject> _nodes = new();

    // SHOOTING/PICKUP
    private GameObject _intakeTrigger;
    private IntakeTriggerData? _intakeData;
    public IntakeTriggerData? IntakeData {
        get => _intakeData;
        set {
            _intakeData = value;
            if (value.HasValue) {
                _intakeTrigger.SetActive(true);
                _intakeTrigger.transform.parent        = RobotNode.transform.Find(_intakeData.Value.NodeName);
                _intakeTrigger.transform.localPosition = _intakeData.Value.RelativePosition.ToVector3();
                _intakeTrigger.GetComponent<SphereCollider>().radius = _intakeData.Value.TriggerSize * 0.5f;
            } else {
                _intakeTrigger.SetActive(false);
            }
        }
    }

    private GameObject _trajectoryPointer;
    private ShotTrajectoryData? _trajectoryData;
    public ShotTrajectoryData? TrajectoryData {
        get => _trajectoryData;
        set {
            _trajectoryData = value;
            if (value.HasValue) {
                _trajectoryPointer.transform.parent        = RobotNode.transform.Find(_trajectoryData.Value.NodeName);
                _trajectoryPointer.transform.localPosition = _trajectoryData.Value.RelativePosition.ToVector3();
                _trajectoryPointer.transform.localRotation = _trajectoryData.Value.RelativeRotation.ToQuaternion();
            }
        }
    }

    private DrivetrainType? _drivetrainType;
    public DrivetrainType ConfiguredDrivetrainType {
        get => _drivetrainType ?? DrivetrainType.ARCADE;
        set {
            _drivetrainType = value;
            SimulationPreferences.SetRobotDrivetrainType(MiraLive.MiraAssembly.Info.GUID, value);
            PreferenceManager.Save();
            ConfigureDrivetrain();
        }
    }

    private Queue<GamepieceSimObject> _gamepiecesInPossession = new Queue<GamepieceSimObject>();
    public bool PickingUpGamepieces { get; private set; }

    public RobotSimObject(string name, ControllableState state, MirabufLive miraLive, GameObject groundedNode,
        Dictionary<string, (Joint a, Joint b)> jointMap)
        : base(name, state) {
        if (_spawnedRobots.ContainsKey(name)) {
            throw new Exception("Robot with that name already loaded");
        }
        _spawnedRobots.Add(name, this);
        EventBus.Push(new RobotSpawnEvent { Bot = name });

        MiraLive       = miraLive;
        GroundedNode   = groundedNode;
        _jointMap      = jointMap;
        RobotNode      = groundedNode.transform.parent.gameObject;
        RobotBounds    = GetBounds(RobotNode.transform);
        GroundedBounds = GetBounds(GroundedNode.transform);
        DebugJointAxes.DebugBounds.Add((GroundedBounds, () => GroundedNode.transform.localToWorldMatrix));
        SimulationPreferences.LoadRobotFromMira(MiraLive);

        _drivetrainType = SimulationPreferences.GetRobotDrivetrain(MiraLive.MiraAssembly.Info.GUID);

        _allRigidbodies = new List<Rigidbody>(RobotNode.transform.GetComponentsInChildren<Rigidbody>());
        PhysicsManager.Register(this);

        foreach (Transform child in RobotNode.transform) {
            _nodes.Add(child.name, child.gameObject);
        }

        // tags every mesh collider component in the robot with a tag of robot
        RobotNode.tag = "robot";
        RobotNode.GetComponentsInChildren<MeshCollider>().ForEach(g => g.tag = "robot");

        _intakeTrigger                        = new GameObject("INTAKE_TRIGGER");
        var trig                              = _intakeTrigger.AddComponent<SphereCollider>();
        trig.isTrigger                        = true;
        trig.radius                           = 0.01f;
        trig.tag                              = "robot";
        trig.transform.parent                 = GroundedNode.transform;
        trig.transform.localPosition          = Vector3.zero;
        trig.transform.localRotation          = Quaternion.identity;
        _trajectoryPointer                    = new GameObject("TRAJECTORY_POINTER");
        _trajectoryPointer.transform.parent   = GroundedNode.transform;
        _trajectoryPointer.transform.position = Vector3.zero;
        _trajectoryPointer.transform.rotation = Quaternion.identity;

        IntakeData = SimulationPreferences.GetRobotIntakeTriggerData(MiraLive.MiraAssembly.Info.GUID);
        SimulationPreferences.SetRobotIntakeTriggerData(MiraLive.MiraAssembly.Info.GUID, _intakeData);
        TrajectoryData = SimulationPreferences.GetRobotTrajectoryData(MiraLive.MiraAssembly.Info.GUID);
        SimulationPreferences.SetRobotTrajectoryData(MiraLive.MiraAssembly.Info.GUID, _trajectoryData);
        PreferenceManager.Save();
        _simulationTranslationLayer =
            SimulationPreferences.GetRobotSimTranslationLayer(MiraLive.MiraAssembly.Info.GUID) ??
            new RioTranslationLayer();
        cam = Camera.main.GetComponent<CameraController>();

        _allRigidbodies.ForEach(x => {
            var rc     = x.gameObject.AddComponent<HighlightComponent>();
            rc.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
            rc.enabled = false;
        });

        if (ModeManager.CurrentMode.GetType() == typeof(ServerTestMode)) {
            Task.Factory.StartNew(() => Client = new LobbyClient("127.0.0.1", Name));
        }
    }

    public static void Setup() {
        InputManager.AssignValueInput(INTAKE_GAMEPIECES,
            TryGetSavedInput(INTAKE_GAMEPIECES, new Digital("E", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
        InputManager.AssignValueInput(OUTTAKE_GAMEPIECES,
            TryGetSavedInput(OUTTAKE_GAMEPIECES, new Digital("Q", context: SimulationRunner.RUNNING_SIM_CONTEXT)));

        SimulationRunner.OnUpdate += () => {
            if (CurrentlyPossessedRobot == string.Empty) {
                return;
            }

            bool pickup = InputManager.MappedValueInputs[INTAKE_GAMEPIECES].Value == 1.0F;
            GetCurrentlyPossessedRobot().PickingUpGamepieces = pickup;
            bool shootGamepiece = InputManager.MappedValueInputs[OUTTAKE_GAMEPIECES].Value == 1.0F;

            if (shootGamepiece &&
                GetCurrentlyPossessedRobot().LastShotTime + TIME_BETWEEN_SHOTS < Time.realtimeSinceStartup) {
                GetCurrentlyPossessedRobot().LastShotTime = Time.realtimeSinceStartup;
                GetCurrentlyPossessedRobot().ShootGamepiece();
            }
        };
    }

    private static Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input            = PreferenceManager.GetPreference<Digital>(key);
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        return defaultInput;
    }

    private void Possess() {
        CurrentlyPossessedRobot    = this.Name;
        BehavioursEnabled          = true;
        OrbitCameraMode.FocusPoint = () =>
            GroundedNode != null && GroundedBounds != null
                ? GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center)
                : Vector3.zero;
    }

    private void Unpossess() {
        GizmoManager.ExitGizmo();
        BehavioursEnabled          = false;
        Vector3 currentPoint       = OrbitCameraMode.FocusPoint();
        OrbitCameraMode.FocusPoint = () => currentPoint;
    }

    public override void Destroy() {
        Client?.Dispose();
        Client = null;
        ClearGamepieces();
        PhysicsManager.Unregister(this);
        if (CurrentlyPossessedRobot.Equals(this._name)) {
            CurrentlyPossessedRobot = string.Empty;
        }
        MonoBehaviour.Destroy(GroundedNode.transform.parent.gameObject);
    }

    public void ClearGamepieces() {
        if (_gamepiecesInPossession.Count == 0)
            return;
        _trajectoryPointer.transform.GetChild(0).transform.parent = FieldSimObject.CurrentField.FieldObject.transform;
        _gamepiecesInPossession.Clear();
    }

    public void CollectGamepiece(GamepieceSimObject gp) {
        if (!_intakeData.HasValue || !_trajectoryData.HasValue)
            return;

        if (_gamepiecesInPossession.Count >= _intakeData.Value.StorageCapacity)
            return;

        var rb              = gp.GamepieceObject.GetComponent<Rigidbody>();
        rb.detectCollisions = false;
        rb.isKinematic      = true;
        gp.GamepieceObject.SetActive(false);

        _gamepiecesInPossession.Enqueue(gp);

        gp.IsCurrentlyPossessed = true;
        if (_gamepiecesInPossession.Count == 1)
            UpdateShownGamepiece();
    }

    public void ShootGamepiece() {
        if (_gamepiecesInPossession.Count == 0) {
            return;
        }

        var gp                              = _gamepiecesInPossession.Dequeue();
        var rb                              = gp.GamepieceObject.GetComponent<Rigidbody>();
        rb.detectCollisions                 = true;
        rb.isKinematic                      = false;
        gp.GamepieceObject.transform.parent = FieldSimObject.CurrentField.FieldObject.transform;
        rb.AddForce(_trajectoryPointer.transform.rotation * Vector3.forward * _trajectoryData.Value.EjectionSpeed,
            ForceMode.VelocityChange);
        gp.IsCurrentlyPossessed = false;

        UpdateShownGamepiece();
    }

    private void UpdateShownGamepiece() {
        // Take the first gamepiece in the queue and display it at trajectory pointer
        if (_gamepiecesInPossession.Count == 0)
            return;

        if (_trajectoryPointer.transform.childCount > 0)
            return;

        var gp = _gamepiecesInPossession.Peek();
        gp.GamepieceObject.SetActive(true);
        gp.GamepieceObject.transform.parent        = _trajectoryPointer.transform;
        gp.GamepieceObject.transform.localRotation = Quaternion.identity;
        gp.GamepieceObject.transform.localPosition = Vector3.zero;
        gp.GamepieceObject.transform.localPosition -= gp.GamepieceBounds.center;
    }

    public void UpdateWheels() {
        if (_wheelDrivers == null)
            return;

        // if (!DriversEnabled) return;

        int wheelsInContact = _wheelDrivers.Count(x => x.HasContacts);
        float mod           = wheelsInContact <= 4 ? 1f : Mathf.Pow(0.7f, wheelsInContact - 4);
        _wheelDrivers.ForEach(x => x.WheelsPhysicsUpdate(mod));
    }

    private static Bounds GetBounds(Transform top) {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        top.GetComponentsInChildren<Renderer>().ForEach(x => {
            var b = x.bounds;
            if (min.x > b.min.x)
                min.x = b.min.x;
            if (min.y > b.min.y)
                min.y = b.min.y;
            if (min.z > b.min.z)
                min.z = b.min.z;
            if (max.x < b.max.x)
                max.x = b.max.x;
            if (max.y < b.max.y)
                max.y = b.max.y;
            if (max.z < b.max.z)
                max.z = b.max.z;
        });
        return new Bounds(((max + min) / 2f) - top.position, max - min);
    }

    public (List<WheelDriver> leftWheels, List<WheelDriver> rightWheels)? GetLeftRightWheels() {
        if (!_tankTrackWheels.HasValue) {
            var wheels = SimulationManager.Drivers[base.Name].OfType<WheelDriver>();

            var leftWheels  = new List<WheelDriver>();
            var rightWheels = new List<WheelDriver>();

            Dictionary<WheelDriver, float> wheelDotProducts = new Dictionary<WheelDriver, float>();
            foreach (var wheel in wheels) {
                wheel.MainInput         = 0f;
                wheelDotProducts[wheel] = Vector3.Dot(Vector3.right, wheel.LocalAnchor);
            }
            float min = float.MaxValue;
            float max = float.MinValue;
            wheelDotProducts.ForEach(x => {
                if (x.Value < min)
                    min = x.Value;
                if (x.Value > max)
                    max = x.Value;
            });
            float mid = (min + max) / 2f;
            wheelDotProducts.ForEach(x => {
                if (x.Value > mid)
                    rightWheels.Add(x.Key);
                else
                    leftWheels.Add(x.Key);
            });

            // Spin all of the wheels straight
            wheels.ForEach(x => {
                var def        = MiraLive.MiraAssembly.Data.Joints.JointDefinitions[x.JointInstance.JointReference];
                var jointAxis  = new Vector3(def.Rotational.RotationalFreedom.Axis.X,
                     def.Rotational.RotationalFreedom.Axis.Y, def.Rotational.RotationalFreedom.Axis.Z);
                var globalAxis = GroundedNode.transform.rotation * jointAxis.normalized;
                var cross      = Vector3.Cross(GroundedNode.transform.up, globalAxis);
                if (MiraLive.MiraAssembly.Info.Version < 5) {
                    if (Vector3.Dot(GroundedNode.transform.forward, cross) > 0) {
                        var ogAxis = jointAxis;

                        ogAxis.x *= -1;
                        ogAxis.y *= -1;
                        ogAxis.z *= -1;
                        def.Rotational.RotationalFreedom.Axis =
                            new MVector3() { X = jointAxis.x, Y = jointAxis.y, Z = jointAxis.z };

                        x.LocalAxis = ogAxis;
                    }
                } else {
                    if (Vector3.Dot(GroundedNode.transform.forward, cross) < 0) {
                        jointAxis.x = -jointAxis.x;
                        var ogAxis  = jointAxis;
                        ogAxis.x *= -1;
                        ogAxis.y *= -1;
                        ogAxis.z *= -1;
                        def.Rotational.RotationalFreedom.Axis =
                            new MVector3() { X = -jointAxis.x, Y = jointAxis.y, Z = jointAxis.z };

                        x.LocalAxis = ogAxis;
                    }
                }
            });
            _tankTrackWheels = (leftWheels, rightWheels);
        }

        return _tankTrackWheels;
    }

    public void ConfigureDefaultBehaviours() {
        if (_wheelDrivers == null) {
            _wheelDrivers = SimulationManager.Drivers[base.Name].OfType<WheelDriver>();
            if (_wheelDrivers.Any()) {
                _wheelDrivers.ForEach(x => x.ImpulseMax = (GroundedNode.GetComponent<Rigidbody>().mass *
                                                              Physics.gravity.magnitude * (1f / 120f)) /
                                                          _wheelDrivers.Count());
                float radius = _wheelDrivers.Average(x => x.Radius);
                _wheelDrivers.ForEach(x => x.Radius = radius);
            }
        }

        ConfigureDrivetrain();
        ConfigureArmBehaviours();
        ConfigureSliderBehaviours();
        ConfigureTestSimulationBehaviours();
        _simBehaviour.Enabled = false;
    }

    public void ConfigureDrivetrain() {
        if (DriveBehaviour != null) {
            SimulationManager.RemoveBehaviour(base.Name, DriveBehaviour);
            DriveBehaviour = null;
        }

        bool success = true;

        if (ConfiguredDrivetrainType.Value == DrivetrainType.ARCADE.Value) {
            ConfigureArcadeDrivetrain();
        } else if (ConfiguredDrivetrainType.Value == DrivetrainType.TANK.Value) {
            ConfigureTankDrivetrain();
        } else if (ConfiguredDrivetrainType.Value == DrivetrainType.SWERVE.Value) {
            success = ConfigureSwerveDrivetrain();
        }

        if (!success) {
            Logger.Log(
                $"Failed to switch to '{ConfiguredDrivetrainType.Name}'. Please select another.", LogLevel.Error);
            ConfiguredDrivetrainType = DrivetrainType.NONE;
        }
    }

    public void ConfigureTestSimulationBehaviours() {
        _simBehaviour = new WSSimBehavior(this.Name, _simulationTranslationLayer);
        SimulationManager.AddBehaviour(this.Name, _simBehaviour);
    }

    // Account for passive joints
    public void ConfigureArmBehaviours() {
        SimulationManager.Drivers[this.Name].ForEach(x => {
            if (x is RotationalDriver driver && !driver.IsReserved) {
                var genArmBehaviour = new GeneralArmBehaviour(this.Name, driver);
                SimulationManager.AddBehaviour(this.Name, genArmBehaviour);
            }
        });
    }

    public void ConfigureSliderBehaviours() {
        SimulationManager.Drivers[this.Name].ForEach(x => {
            if (x is LinearDriver) {
                var behaviour = new GeneralSliderBehaviour(this.Name, x as LinearDriver);
                SimulationManager.AddBehaviour(this.Name, behaviour);
            }
        });
    }

    public void ConfigureArcadeDrivetrain() {
        var wheels = GetLeftRightWheels();

        var arcadeBehaviour = new ArcadeDriveBehaviour(this.Name, wheels!.Value.leftWheels, wheels!.Value.rightWheels);
        DriveBehaviour      = arcadeBehaviour;

        SimulationManager.AddBehaviour(this.Name, arcadeBehaviour);
    }

    public void ConfigureTankDrivetrain() {
        var wheels = GetLeftRightWheels();

        var tankBehaviour = new TankDriveBehavior(this.Name, wheels!.Value.leftWheels, wheels!.Value.rightWheels);
        DriveBehaviour    = tankBehaviour;

        SimulationManager.AddBehaviour(this.Name, tankBehaviour);
    }

    public bool ConfigureSwerveDrivetrain() {
        // Sets wheels rotating forward
        GetLeftRightWheels();

        try {
            List<RotationalDriver> potentialAzimuthDrivers =
                SimulationManager.Drivers[base._name]
                    .OfType<RotationalDriver>()
                    .Where(x => !x.IsWheel &&
                                (x.Axis - Vector3.Dot(GroundedNode.transform.up, x.Axis) * GroundedNode.transform.up)
                                        .magnitude < 0.05f)
                    .ToList();

            var wheels = SimulationManager.Drivers[base._name].OfType<WheelDriver>();

            if (potentialAzimuthDrivers.Count() < wheels.Count())
                return false;

            modules = new(RotationalDriver azimuth, WheelDriver driver)[wheels.Count()];
            int i   = 0;
            wheels.ForEach(x => {
                RotationalDriver closest = null;
                float distance           = float.MaxValue;
                potentialAzimuthDrivers.ForEach(
                    y => closest = (y.Anchor - x.Anchor).magnitude < distance ? y : closest);
                modules[i] = (closest, x);
                potentialAzimuthDrivers.Remove(closest);
                i++;
            });

        } catch (Exception _) {
            return false;
        }

        var swerveBehaviour = new SwerveDriveBehaviour(this, modules);
        DriveBehaviour      = swerveBehaviour;

        SimulationManager.AddBehaviour(this.Name, swerveBehaviour);
        return true;
    }

    public static void SpawnRobot(string filePath) {
        SpawnRobot(filePath, new Vector3(0f, 0.5f, 0f), Quaternion.identity, true);
    }

    public static void SpawnRobot(string filePath, bool spawnGizmo) {
        SpawnRobot(filePath, new Vector3(0f, 0.5f, 0f), Quaternion.identity, spawnGizmo);
    }

    public static void SpawnRobot(string filePath, Vector3 position, Quaternion rotation) {
        SpawnRobot(filePath, position, rotation, true);
    }

    public static void SpawnRobot(string filePath, Vector3 position, Quaternion rotation, bool spawnGizmo) {
        var mira                 = Importer.MirabufAssemblyImport(filePath);
        RobotSimObject simObject = mira.Sim as RobotSimObject;
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        simObject.ConfigureDefaultBehaviours();

        mira.MainObject.transform.position = position;
        mira.MainObject.transform.rotation = rotation;

        simObject.Possess();
        MainHUD.SelectedRobot = simObject;

        if (spawnGizmo)
            GizmoManager.SpawnGizmo(simObject);
        AnalyticsManager.LogCustomEvent(AnalyticsEvent.RobotSpawned, ("RobotName", mira.MainObject.name));
    }

    public static bool RemoveRobot(string robot) {
        if (!_spawnedRobots.ContainsKey(robot))
            return false;

        GizmoManager.ExitGizmo();

        if (robot == CurrentlyPossessedRobot)
            CurrentlyPossessedRobot = string.Empty;
        _spawnedRobots.Remove(robot);
        MainHUD.SelectedRobot = null;
        return SimulationManager.RemoveSimObject(robot);
    }

    public static void RemoveAllRobots() {
        string[] robots = new string[_spawnedRobots.Keys.Count];
        _spawnedRobots.Keys.CopyTo(robots, 0);

        robots.ForEach(x => { RemoveRobot(x); });
    }

    private Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)> _preFreezeStates =
        new Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)>();
    // clang-format off
    private bool _isFrozen = false;
    // clang-format on
    public bool isFrozen() => _isFrozen;

    public void Freeze() {
        if (_isFrozen)
            return;

        _allRigidbodies.ForEach(x => {
            _preFreezeStates[x] = (x.isKinematic, x.velocity, x.angularVelocity);
            x.isKinematic       = true;
            x.detectCollisions  = false;
        });

        _isFrozen = true;
    }

    public void Unfreeze() {
        if (!_isFrozen) {
            return;
        }

        _allRigidbodies.ForEach(x => {
            var originalState  = _preFreezeStates[x];
            x.isKinematic      = originalState.isKine;
            x.detectCollisions = true;
        });
        _preFreezeStates.Clear();

        _isFrozen = false;
    }

    public List<Rigidbody> GetAllRigidbodies() => _allRigidbodies;

    public GameObject GetRootGameObject() {
        return RobotNode;
    }

    public TransformData GetGizmoData() {
        return new TransformData { Position =
                                       GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center),
            Rotation = GroundedNode.transform.rotation };
    }

    public void UpdateMultiplayer() {
        if (Client is not null) {
            if (RobotNode.name != "host") {
                List<SignalData> changedSignals = new List<SignalData>();
                foreach (var driver in SimulationManager.Drivers[Name]) {
                    List<SignalData> changes =
                        driver.State.CompileChanges().Where(s => s.Name != string.Empty).ToList();
                    foreach (var signal in changes)
                        changedSignals.Add(signal);
                }

                Client.UpdateControllableState(changedSignals).ContinueWith((x, o) => {
                    if (!x.IsCompletedSuccessfully)
                        return;
                    var msg = x.Result.GetResult();
                    msg?.FromSimulationTransformData.TransformData.ForEach(t => {
                        if (t.Transforms.Count != 0)
                            ServerTransforms[t.Guid] = t.Clone();
                    });
                }, false);
            }

            // TODO compare guids once networking between computers
            // right now only does it if ghost because ghost is acting as client
            if (RobotNode.name != "host") {
                if (Client.Guid.HasValue && ServerTransforms.TryGetValue(Client.Guid.Value, out var transform)) {
                    if (transform != null) {
                        foreach (var td in transform.Transforms) {
                            Matrix4x4 matrix        = (Matrix4x4) td.Value;
                            Transform nodeTransform = _nodes[td.Key].transform;
                            nodeTransform.position  = matrix.GetPosition();
                            nodeTransform.rotation  = matrix.rotation;
                        }
                    }
                }
            }
        }
    }

    public void Update(TransformData data) {
        RobotNode.transform.rotation = Quaternion.identity;
        RobotNode.transform.position = Vector3.zero;

        RobotNode.transform.rotation = data.Rotation * Quaternion.Inverse(GroundedNode.transform.rotation);
        RobotNode.transform.position =
            data.Position - GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);
    }

    public void End(TransformData data) {
        PracticeMode.SetInitialState(RobotNode);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct DrivetrainType {
        public static readonly DrivetrainType NONE   = new DrivetrainType(0, "None");
        public static readonly DrivetrainType TANK   = new DrivetrainType(1, "Tank");
        public static readonly DrivetrainType ARCADE = new DrivetrainType(2, "Arcade");
        public static readonly DrivetrainType SWERVE = new DrivetrainType(3, "Swerve");

        [JsonProperty]
        private string _name;
        public string Name => _name;
        [JsonProperty]
        private int _value;
        public int Value => _value;

        private DrivetrainType(int val, string name) {
            _value = val;
            _name  = name;
        }
    }

    public static readonly DrivetrainType[] DRIVETRAIN_TYPES = { DrivetrainType.NONE, DrivetrainType.TANK,
        DrivetrainType.ARCADE, DrivetrainType.SWERVE };

    public struct IntakeTriggerData {
        public string NodeName;
        public float TriggerSize;
        public float[] RelativePosition;
        public int StorageCapacity;
    }

    public struct ShotTrajectoryData {
        public string NodeName;
        public float EjectionSpeed;
        public float[] RelativePosition;
        public float[] RelativeRotation;
    }

    public class PossessionChangeEvent : IEvent {
        public string NewBot;
        public string OldBot;
    }

    public class RobotSpawnEvent : IEvent {
        public string Bot;
    }

    public class RobotRemoveEvent : IEvent {
        public string Bot;
    }

    public void CreateDrivetrainTooltip() {
        switch (ConfiguredDrivetrainType.Name) {
            case "Arcade":
                TooltipManager.CreateTooltip(("WASD", "Drive"), ("E", "Intake"), ("Q", "Dispense"));
                return;
            case "Tank":
                TooltipManager.CreateTooltip(
                    ("WS", "Drivetrain Left"), ("IK", "Drivetrain Right"), ("E", "Intake"), ("Q", "Dispense"));
                return;
            case "Swerve":
                TooltipManager.CreateTooltip(("WASD", "Drive"), ("< >", "Turn"), ("E", "Intake"), ("Q", "Dispense"));
                return;
        }
    }
}

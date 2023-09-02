using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Analytics;
using Newtonsoft.Json;
using SimObjects.MixAndMatch;
using Synthesis;
using Synthesis.Gizmo;
using Synthesis.Import;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
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
using Input    = UnityEngine.Input;
using Logger   = SynthesisAPI.Utilities.Logger;
using MVector3 = Mirabuf.Vector3;
using Object   = UnityEngine.Object;

#nullable enable

public class RobotSimObject : SimObject, IPhysicsOverridable, IGizmo {
    public const int MAX_ROBOTS = 6;

    private const string INTAKE_GAMEPIECES  = "input/intake";
    private const string OUTTAKE_GAMEPIECES = "input/shoot-gamepiece";

    private const float TIME_BETWEEN_SHOTS = 0.5f;
    private float _lastShotTime;

    public static int ControllableJointCounter = 0;

    private OrbitCameraMode orbit;
    private ICameraMode previousMode;

    private IEnumerable<WheelDriver>? _wheelDrivers;
    public (RotationalDriver azimuth, WheelDriver driver)[] modules;

    public string RobotGUID { get; }
    public MirabufLive[] MiraLiveFiles { get; }
    public MixAndMatchRobotData? MixAndMatchRobotData { get; }
    public GameObject GroundedNode { get; }
    public Bounds GroundedBounds { get; }
    public GameObject RobotNode { get; }
    public Bounds RobotBounds { get; }

    private readonly List<Rigidbody> _allRigidbodies;
    public IReadOnlyCollection<Rigidbody> AllRigidbodies => _allRigidbodies.AsReadOnly();

    private readonly Dictionary<string, GameObject> _nodes = new();

    public readonly bool IsMixAndMatch;

#region Robot Possession

    private static string _currentlyPossessedRobot = string.Empty;

    public static string CurrentlyPossessedRobot {
        get => _currentlyPossessedRobot;
        set {
            if (value == _currentlyPossessedRobot)
                return;

            var old = _currentlyPossessedRobot;

            if (_currentlyPossessedRobot != string.Empty)
                GetCurrentlyPossessedRobot()?.Unpossess();

            _currentlyPossessedRobot = value;

            if (_currentlyPossessedRobot != string.Empty)
                GetCurrentlyPossessedRobot()?.Possess();

            EventBus.Push(new PossessionChangeEvent { NewBot = value, OldBot = old });
        }
    }

    public static RobotSimObject? GetCurrentlyPossessedRobot() => _currentlyPossessedRobot == string.Empty
                                                                      ? null
                                                                      : _spawnedRobots[_currentlyPossessedRobot];

    private static readonly Dictionary<string, RobotSimObject> _spawnedRobots       = new();
    public static Dictionary<string, RobotSimObject>.ValueCollection SpawnedRobots => _spawnedRobots.Values;

#endregion

#region Multiplayer

    private static Dictionary<ulong, ServerTransforms> _serverTransforms = new();

    public static Dictionary<ulong, ServerTransforms> ServerTransforms {
        get => _serverTransforms;
        set => _serverTransforms = value;
    }

    private LobbyClient? _client;

    public LobbyClient? Client {
        get => _client;
        set => _client = value;
    }

#endregion

#region Behaviours

    private WSSimBehavior _simBehaviour;
    private RioTranslationLayer _simulationTranslationLayer;

    public RioTranslationLayer SimulationTranslationLayer {
        get => _simulationTranslationLayer;
        set {
            _simulationTranslationLayer = value;
            _simBehaviour.Translation   = _simulationTranslationLayer;

            SimulationPreferences.SetRobotSimTranslationLayer(RobotGUID, _simulationTranslationLayer);
            PreferenceManager.Save();
        }
    }

    private bool _useSimBehaviour;

    public bool UseSimulationBehaviour {
        get => _useSimBehaviour;
        set {
            if (_useSimBehaviour != value) {
                _useSimBehaviour = value;
                SimulationManager.Behaviours[_name].ForEach(b => {
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

    private readonly GameObject _intakeTrigger;
    private IntakeTriggerData? _intakeData;

    public IntakeTriggerData? IntakeData {
        get => _intakeData;
        set {
            _intakeData = value;
            if (value.HasValue) {
                _intakeTrigger.SetActive(true);
                _intakeTrigger.transform.parent                      = RobotNode.transform.Find(_intakeData?.NodeName);
                _intakeTrigger.transform.localPosition               = _intakeData!.Value.RelativePosition.ToVector3();
                _intakeTrigger.GetComponent<SphereCollider>().radius = _intakeData.Value.TriggerSize * 0.5f;
            } else {
                _intakeTrigger.SetActive(false);
            }
        }
    }

    private readonly GameObject _trajectoryPointer;
    private ShotTrajectoryData? _trajectoryData;

    public ShotTrajectoryData? TrajectoryData {
        get => _trajectoryData;
        set {
            _trajectoryData = value;
            if (value.HasValue) {
                _trajectoryPointer.transform.parent        = RobotNode.transform.Find(_trajectoryData?.NodeName);
                _trajectoryPointer.transform.localPosition = _trajectoryData!.Value.RelativePosition.ToVector3();
                _trajectoryPointer.transform.localRotation = _trajectoryData.Value.RelativeRotation.ToQuaternion();
            }
        }
    }

    private readonly Queue<GamepieceSimObject> _gamepiecesInPossession = new();
    public bool PickingUpGamepieces { get; private set; }

    private DrivetrainType _drivetrainType;

    public DrivetrainType ConfiguredDrivetrainType {
        get => _drivetrainType;
        set {
            _drivetrainType = value;
            SimulationPreferences.SetRobotDrivetrainType(RobotGUID, value);
            PreferenceManager.Save();
            ConfigureDrivetrain();
        }
    }

    private (List<WheelDriver> leftWheels, List<WheelDriver> rightWheels)? _tankTrackWheels;

#endregion

    public RobotSimObject(string name, ControllableState state, MirabufLive[] miraLiveFiles, GameObject groundedNode,
        bool isMixAndMatch, MixAndMatchRobotData? robotData)
        : base(name, state) {
        RobotGUID = (isMixAndMatch) ? robotData!.Name : miraLiveFiles![0].MiraAssembly.Info.GUID;

        MiraLiveFiles        = miraLiveFiles;
        IsMixAndMatch        = isMixAndMatch;
        MixAndMatchRobotData = robotData;

        GroundedNode   = groundedNode;
        GroundedBounds = GetBounds(GroundedNode.transform);

        RobotNode   = groundedNode.transform.parent.gameObject;
        RobotBounds = GetBounds(RobotNode.transform);

        DebugJointAxes.DebugBounds.Add((GroundedBounds, () => GroundedNode.transform.localToWorldMatrix));

        if (_spawnedRobots.ContainsKey(name)) {
            Logger.Log($"Robot \"{name}\" already loaded!", LogLevel.Error);
            throw new Exception($"Robot \"{name}\" already loaded!");
        }

        _spawnedRobots.Add(name, this);

        _allRigidbodies = new List<Rigidbody>(RobotNode.transform.GetComponentsInChildren<Rigidbody>());

        foreach (Transform child in RobotNode.transform) {
            _nodes.Add(child.name, child.gameObject);
        }

        // tags every mesh collider component in the robot with a tag of robot
        RobotNode.tag = "robot";
        RobotNode.GetComponentsInChildren<MeshCollider>().ForEach(g => g.tag = "robot");

        // Highlight component MonoBehaviour for configuring intake/trajectory data
        _allRigidbodies.ForEach(x => {
            var rc     = x.gameObject.AddComponent<HighlightComponent>();
            rc.Color   = ColorManager.GetColor(ColorManager.SynthesisColor.HighlightHover);
            rc.enabled = false;
        });

        // Intake and outtake gameobjects
        {
            _intakeTrigger = new GameObject("INTAKE_TRIGGER") {
                transform = {localPosition = Vector3.zero, localRotation = Quaternion.identity}
            };

            var intakeCollider = _intakeTrigger.AddComponent<SphereCollider>();

            intakeCollider.isTrigger        = true;
            intakeCollider.tag              = "robot";
            intakeCollider.transform.parent = GroundedNode.transform;
            intakeCollider.radius           = 0.01f;

            _trajectoryPointer = new GameObject("TRAJECTORY_POINTER") {
                transform = {parent = GroundedNode.transform, position = Vector3.zero, rotation = Quaternion.identity}
            };
        }

        // Robot Preferences
        {
            if (isMixAndMatch)
                SimulationPreferences.LoadRobotFromMixAndMatch(robotData!);
            else
                SimulationPreferences.LoadRobotFromMira(MiraLiveFiles[0]);

            var _drivetrainInfo = SimulationPreferences.GetRobotDrivetrain(RobotGUID);

            // If no drivetrain is found in robot data, search all mira files for a drivetrain type
            if (isMixAndMatch && !_drivetrainInfo.foundDrivetrain) {
                foreach (var m in MiraLiveFiles) {
                    SimulationPreferences.LoadRobotFromMira(m);
                    var miraDrivetrain = SimulationPreferences.GetRobotDrivetrain(m.MiraAssembly.Info.GUID);
                    if (miraDrivetrain.foundDrivetrain) {
                        _drivetrainType = miraDrivetrain.drivetrain;
                        break;
                    }
                }
            } else
                _drivetrainType = _drivetrainInfo.drivetrain;

            SimulationPreferences.SetRobotDrivetrainType(RobotGUID, _drivetrainType);

            IntakeData = SimulationPreferences.GetRobotIntakeTriggerData(RobotGUID);
            SimulationPreferences.SetRobotIntakeTriggerData(RobotGUID, _intakeData);

            TrajectoryData = SimulationPreferences.GetRobotTrajectoryData(RobotGUID);
            SimulationPreferences.SetRobotTrajectoryData(RobotGUID, _trajectoryData);

            PreferenceManager.Save();
        }

        _simulationTranslationLayer =
            SimulationPreferences.GetRobotSimTranslationLayer(RobotGUID) ?? new RioTranslationLayer();

        if (ModeManager.CurrentMode.GetType() == typeof(ServerTestMode)) {
            Task.Factory.StartNew(() => Client = new LobbyClient("127.0.0.1", Name));
        }

        PhysicsManager.Register(this);
        EventBus.Push(new RobotSpawnEvent { Bot = name });
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
                GetCurrentlyPossessedRobot()._lastShotTime + TIME_BETWEEN_SHOTS < Time.realtimeSinceStartup) {
                GetCurrentlyPossessedRobot()._lastShotTime = Time.realtimeSinceStartup;
                GetCurrentlyPossessedRobot().ShootGamepiece();
            }
        };
    }

    private static Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (InputManager.MappedValueInputs.ContainsKey(key)) {
            var input            = InputManager.GetAnalog(key);
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        if (PreferenceManager.ContainsPreference(key)) {
            var input = PreferenceManager.GetPreference<Digital>(key) ??
                        (Digital) PreferenceManager.GetPreference<InputData[]>(key) [0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        return defaultInput;
    }

    private void Possess() {
        CurrentlyPossessedRobot    = Name;
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

        Object.Destroy(GroundedNode.transform.parent.gameObject);
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

    private void ShootGamepiece() {
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
        float mod           = wheelsInContact <= 3 ? 1f : Mathf.Pow(0.7f, wheelsInContact - 3);

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
                foreach (var miraLive in MiraLiveFiles) {
                    if (!miraLive.MiraAssembly.Data.Joints.JointDefinitions.TryGetValue(
                            x.JointInstance.JointReference, out var def))
                        continue;

                    var jointAxis  = new Vector3(def.Rotational.RotationalFreedom.Axis.X,
                         def.Rotational.RotationalFreedom.Axis.Y, def.Rotational.RotationalFreedom.Axis.Z);
                    var globalAxis = GroundedNode.transform.rotation * jointAxis.normalized;
                    var cross      = Vector3.Cross(GroundedNode.transform.up, globalAxis);
                    if (miraLive.MiraAssembly.Info.Version < 5) {
                        if (Vector3.Dot(GroundedNode.transform.forward, cross) > 0) {
                            var ogAxis = jointAxis;

                            ogAxis.x *= -1;
                            ogAxis.y *= -1;
                            ogAxis.z *= -1;
                            // Modify assembly for if a new behaviour evaluates this again
                            // def.Rotational.RotationalFreedom.Axis = ogAxis; // I think this is irrelevant after the
                            // last few lines
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
                            // Modify assembly for if a new behaviour evaluates this again
                            // def.Rotational.RotationalFreedom.Axis = ogAxis; // I think this is irrelevant after the
                            // last few lines
                            def.Rotational.RotationalFreedom.Axis =
                                new MVector3() { X = -jointAxis.x, Y = jointAxis.y, Z = jointAxis.z };

                            x.LocalAxis = ogAxis;
                        }
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

            var wheelDrivers = _wheelDrivers as WheelDriver[] ?? _wheelDrivers.ToArray();

            if (wheelDrivers.Any()) {
                wheelDrivers.ForEach(x => x.ImpulseMax = (GroundedNode.GetComponent<Rigidbody>().mass *
                                                             Physics.gravity.magnitude * (1f / 120f)) /
                                                         _wheelDrivers.Count());

                float radius = wheelDrivers.Average(x => x.Radius);
                wheelDrivers.ForEach(x => x.Radius = radius);
            }
        }

        ConfigureDrivetrain();
        ConfigureArmBehaviours();
        ConfigureSliderBehaviours();
        ConfigureTestSimulationBehaviours();
        _simBehaviour.Enabled = false;
    }

    private void ConfigureDrivetrain() {
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

    private void ConfigureTestSimulationBehaviours() {
        _simBehaviour = new WSSimBehavior(Name, _simulationTranslationLayer);
        SimulationManager.AddBehaviour(Name, _simBehaviour);
    }

    // Account for passive joints
    private void ConfigureArmBehaviours() {
        SimulationManager.Drivers[Name].ForEach(x => {
            if (x is RotationalDriver { IsReserved : false } driver) {
                var genArmBehaviour = new GeneralArmBehaviour(Name, driver);
                SimulationManager.AddBehaviour(Name, genArmBehaviour);
            }
        });
    }

    private void ConfigureSliderBehaviours() {
        SimulationManager.Drivers[Name].ForEach(x => {
            if (x is not LinearDriver linearDriver)
                return;

            var behaviour = new GeneralSliderBehaviour(Name, linearDriver);
            SimulationManager.AddBehaviour(Name, behaviour);
        });
    }

    private void ConfigureArcadeDrivetrain() {
        var wheels = GetLeftRightWheels();

        var arcadeBehaviour = new ArcadeDriveBehaviour(Name, wheels!.Value.leftWheels, wheels!.Value.rightWheels);
        DriveBehaviour      = arcadeBehaviour;

        SimulationManager.AddBehaviour(Name, arcadeBehaviour);
    }

    private void ConfigureTankDrivetrain() {
        var wheels = GetLeftRightWheels();

        var tankBehaviour = new TankDriveBehavior(Name, wheels!.Value.leftWheels, wheels!.Value.rightWheels);
        DriveBehaviour    = tankBehaviour;

        SimulationManager.AddBehaviour(Name, tankBehaviour);
    }

    private bool ConfigureSwerveDrivetrain() {
        // Sets wheels rotating forward
        GetLeftRightWheels();

        try {
            List<RotationalDriver> potentialAzimuthDrivers =
                SimulationManager.Drivers[_name]
                    .OfType<RotationalDriver>()
                    .Where(x => !x.IsWheel &&
                                (x.Axis - Vector3.Dot(GroundedNode.transform.up, x.Axis) * GroundedNode.transform.up)
                                        .magnitude < 0.05f)
                    .ToList();

            var wheels = SimulationManager.Drivers[_name].OfType<WheelDriver>();

            var wheelDrivers = wheels as WheelDriver[] ?? wheels.ToArray();

            if (potentialAzimuthDrivers.Count() < wheelDrivers.Count())
                return false;

            modules = new(RotationalDriver azimuth, WheelDriver driver)[wheelDrivers.Count()];
            int i   = 0;
            wheelDrivers.ForEach(x => {
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

        SimulationManager.AddBehaviour(Name, swerveBehaviour);
        return true;
    }

    public static void SpawnRobot(
        MixAndMatchRobotData mixAndMatchRobotData, bool spawnGizmo = true, string? filePath = null) {
        SpawnRobot(mixAndMatchRobotData, new Vector3(0f, 0.5f, 0f), Quaternion.identity, spawnGizmo, filePath);
    }

    public static void SpawnRobot(MixAndMatchRobotData mixAndMatchRobotData, Vector3 position, Quaternion rotation,
        bool spawnGizmo, string? filePath) {
        if (mixAndMatchRobotData?.PartTransformData.Length == 0) {
            Logger.Log("Custom robot contains no parts", LogLevel.Info);
            return;
        }

        var mira = filePath == null ? Importer.ImportMixAndMatchRobot(mixAndMatchRobotData)
                                    : Importer.ImportSimpleRobot(filePath);

        RobotSimObject simObject = (mira.sim as RobotSimObject)!;
        mira.mainObject.transform.SetParent(GameObject.Find("Game").transform);
        simObject.ConfigureDefaultBehaviours();

        mira.mainObject.transform.position = position;
        mira.mainObject.transform.rotation = rotation;

        simObject.Possess();
        MainHUD.SelectedRobot = simObject;

        if (spawnGizmo)
            GizmoManager.SpawnGizmo(simObject);
        AnalyticsManager.LogCustomEvent(AnalyticsEvent.RobotSpawned, ("RobotName", mira.mainObject.name));
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

    private Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)> _preFreezeStates = new();

    private bool _isFrozen;
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

                Client.UpdateControllableState(changedSignals).ContinueWith((x, _) => {
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
        public static readonly DrivetrainType NONE   = new(0, "None");
        public static readonly DrivetrainType TANK   = new(1, "Tank");
        public static readonly DrivetrainType ARCADE = new(2, "Arcade");
        public static readonly DrivetrainType SWERVE = new(3, "Swerve");

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
        string MiraId  = MainHUD.SelectedRobot.RobotGUID;
        int inputCount = 3; // for drive, intake, and eject
        if (ConfiguredDrivetrainType.Name.Equals("Swerve"))
            inputCount++; // for turn
        MainHUD.SelectedRobot.GetAllReservedInputs().ForEach(input => {
            if (!input.displayName.Contains("Arcade") &&
                (!input.displayName.Contains("Swerve") || input.displayName.Contains("Reset Forward")) &&
                !input.displayName.Contains("Tank"))
                inputCount++;
        });
        (string, string)[] inputs = new(string key, string input)[inputCount];
        int i                     = 0;
        switch (ConfiguredDrivetrainType.Name) {
            case "Arcade":
                string f  = GetTooltipOutput(MiraId + "Arcade Forward", "W");
                string b  = GetTooltipOutput(MiraId + "Arcade Backward", "S");
                string l  = GetTooltipOutput(MiraId + "Arcade Left", "A");
                string r  = GetTooltipOutput(MiraId + "Arcade Right", "D");
                inputs[0] = (f + b + l + r, "Drive");
                i++;
                break;
            case "Tank":
                f         = GetTooltipOutput(MiraId + "Tank Left-Forward", "W");
                b         = GetTooltipOutput(MiraId + "Tank Left-Reverse", "S");
                l         = GetTooltipOutput(MiraId + "Tank Right-Forward", "I");
                r         = GetTooltipOutput(MiraId + "Tank Right-Reverse", "K");
                inputs[0] = (f + b + l + r, "Drive");
                i++;
                break;
            case "Swerve":
                f         = GetTooltipOutput(MiraId + "Swerve Forward", "W");
                b         = GetTooltipOutput(MiraId + "Swerve Backward", "S");
                l         = GetTooltipOutput(MiraId + "Swerve Left", "A");
                r         = GetTooltipOutput(MiraId + "Swerve Right", "D");
                inputs[0] = (f + b + l + r, "Drive");
                i++;
                string lturn = GetTooltipOutput(MiraId + "Swerve Turn Left", "LeftArrow");
                string rturn = GetTooltipOutput(MiraId + "Swerve Turn Right", "RightArrow");
                inputs[1]    = (lturn + " " + rturn, "Turn");
                i++;
                break;
        }
        foreach (var inputKey in MainHUD.SelectedRobot.GetAllReservedInputs()) {
            if (!inputKey.displayName.Contains("Arcade") &&
                (!inputKey.displayName.Contains("Swerve") || inputKey.displayName.Contains("Reset Forward")) &&
                !inputKey.displayName.Contains("Tank")) {
                inputs[i] = (InputManager.MappedValueInputs[inputKey.key].Name, inputKey.displayName);
                i++;
            }
        }
        inputs[i] = (((Digital) TryGetSavedInput(
                          INTAKE_GAMEPIECES, new Digital("E", context: SimulationRunner.RUNNING_SIM_CONTEXT)))
                         .Name,
            "Intake");
        i++;
        inputs[i] = (((Digital) TryGetSavedInput(
                          OUTTAKE_GAMEPIECES, new Digital("Q", context: SimulationRunner.RUNNING_SIM_CONTEXT)))
                         .Name,
            "Eject");
        TooltipManager.CreateTooltip(inputs);
    }

    private string GetTooltipOutput(string key, string defaultInput) {
        Analog input = InputManager.MappedValueInputs.ContainsKey(key)
                           ? InputManager.GetAnalog(key)
                           : SimulationPreferences.GetRobotInput(MainHUD.SelectedRobot.RobotGUID, key);
        return input != null ? input.Name : defaultInput;
    }
}

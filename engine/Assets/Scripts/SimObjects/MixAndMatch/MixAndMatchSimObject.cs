using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis;
using Synthesis.Import;
using Synthesis.Physics;
using SynthesisAPI.Simulation;
using UnityEngine;
using Bounds    = UnityEngine.Bounds;
using Joint     = UnityEngine.Joint;
using MVector3  = Mirabuf.Vector3;
using Transform = UnityEngine.Transform;
using Vector3   = UnityEngine.Vector3;
using Synthesis.Gizmo;
using Synthesis.PreferenceManager;
using Synthesis.UI;
using SynthesisAPI.EventBus;
using Synthesis.WS.Translation;
using SynthesisAPI.Controller;

#nullable enable

public class MixAndMatchSimObject : SimObject, IPhysicsOverridable, IGizmo {
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
    public static MixAndMatchSimObject GetCurrentlyPossessedRobot() => (_currentlyPossessedRobot == string.Empty
                                                                    ? null
                                                                    : _spawnedRobots[_currentlyPossessedRobot])!;

    private static Dictionary<string, MixAndMatchSimObject> _spawnedRobots = new(); // Open
    public static Dictionary<string, MixAndMatchSimObject>.ValueCollection SpawnedRobots => _spawnedRobots.Values;

    public static int ControllableJointCounter = 0;

    private CameraController cam;
    private OrbitCameraMode orbit;
    private ICameraMode previousMode;

    private IEnumerable<WheelDriver>? _wheelDrivers;

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
    
    private Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> _jointMap;
    private List<Rigidbody> _allRigidbodies;
    public IReadOnlyCollection<Rigidbody> AllRigidbodies => _allRigidbodies.AsReadOnly();

    private Queue<GamepieceSimObject> _gamepiecesInPossession = new Queue<GamepieceSimObject>();
    public bool PickingUpGamepieces { get; private set; }

    public MixAndMatchSimObject(string name, ControllableState state, MirabufLive miraLive, GameObject groundedNode,
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
        SimulationPreferences.LoadFromMirabufLive(MiraLive);

        _allRigidbodies = new List<Rigidbody>(RobotNode.transform.GetComponentsInChildren<Rigidbody>());
        PhysicsManager.Register(this);

        // tags every mesh collider component in the robot with a tag of robot
        RobotNode.tag = "robot";
        RobotNode.GetComponentsInChildren<MeshCollider>().ForEach(g => g.tag = "robot");
        
        _simulationTranslationLayer =
            SimulationPreferences.GetRobotSimTranslationLayer(MiraLive.MiraAssembly.Info.GUID) ??
            new RioTranslationLayer();
        // _simulationTranslationLayer = new RioTranslationLayer();

        cam = Camera.main.GetComponent<CameraController>();

        _allRigidbodies.ForEach(x => {
            var rc     = x.gameObject.AddComponent<HighlightComponent>();
            rc.Color   = ColorManager.TryGetColor(ColorManager.SYNTHESIS_HIGHLIGHT_HOVER);
            rc.enabled = false;
        });
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
        PhysicsManager.Unregister(this);
        if (CurrentlyPossessedRobot.Equals(this._name)) {
            CurrentlyPossessedRobot = string.Empty;
        }
        MonoBehaviour.Destroy(GroundedNode.transform.parent.gameObject);
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

    public static void SpawnMixAndMatch(params string[] files)
    {
        List<(string filePath, Vector3 position, Quaternion rotation, bool spawnGizmo)> parts = new();
        files.ForEach(f =>
        {
            parts.Add((f, Vector3.zero, Quaternion.identity, true));
        });

        SpawnMixAndMatch(parts.ToArray());
    }
    
    public static void SpawnMixAndMatch((string filePath, Vector3 position, Quaternion rotation, bool spawnGizmo)[] parts) {
        parts.ForEach(p =>
        {
            var mira = Importer.MirabufAssemblyImport(p.filePath);
            SimObject simObject = mira.Sim;
            
            Debug.Log(simObject.GetType());
            
            mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);

            mira.MainObject.transform.position = p.position;
            mira.MainObject.transform.rotation = p.rotation;
        });
    }

    private Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)> _preFreezeStates = new();
    
    private bool _isFrozen = false;
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

    public void Update(TransformData data) {
        // GroundedNode.transform.rotation = data.Rotation;

        /*
        GroundedNode.transform.position -=
        GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center); GroundedNode.transform.rotation
        = Quaternion.identity;

        Matrix4x4 transformation = Matrix4x4.identity;

        // transformation =
        Matrix4x4.TRS(-GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center),
        Quaternion.identity, Vector3.one) * transformation; transformation = Matrix4x4.TRS(Vector3.zero, data.Rotation,
        Vector3.one) * transformation;
        // transformation *= Matrix4x4.TRS(data.Position, Quaternion.identity, Vector3.one);

        // Apply Gizmo
        GroundedNode.transform.rotation = transformation.rotation;
        GroundedNode.transform.position -=
        GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center); GroundedNode.transform.position
        += transformation.GetPosition(mod: false);
        */

        RobotNode.transform.rotation = Quaternion.identity;
        RobotNode.transform.position = Vector3.zero;

        RobotNode.transform.rotation = data.Rotation * Quaternion.Inverse(GroundedNode.transform.rotation);
        RobotNode.transform.position =
            data.Position - GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);

        // GroundedNode.transform.RotateAround()
        // GroundedNode.transform.position = data.Position - GroundedBounds.center;
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Joint;
using Synthesis;
using Synthesis.Import;
using Synthesis.Util;
using Synthesis.Physics;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Bounds = UnityEngine.Bounds;
using Joint = UnityEngine.Joint;
using MVector3 = Mirabuf.Vector3;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;
using Synthesis.Gizmo;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.InputManager;
using SynthesisAPI.EventBus;
using Synthesis.WS.Translation;
using static Synthesis.WS.Translation.RioTranslationLayer;

public class RobotSimObject : SimObject, IPhysicsOverridable, IGizmo {

    public const string INTAKE_GAMEPIECES = "input/intake";

    private static string _currentlyPossessedRobot = string.Empty;
    public static string CurrentlyPossessedRobot {
        get => _currentlyPossessedRobot;
        private set {
            if (value != _currentlyPossessedRobot) {
                var old = _currentlyPossessedRobot;
                _currentlyPossessedRobot = value;
                
                EventBus.Push(new NewRobotEvent { NewBot = value, OldBot = old });
            }
        }
    }
    public static RobotSimObject GetCurrentlyPossessedRobot()
        => CurrentlyPossessedRobot == string.Empty ? null : SimulationManager._simObjects[CurrentlyPossessedRobot] as RobotSimObject;

    public static int ControllableJointCounter = 0;

    private CameraController cam;
    private OrbitCameraMode orbit;
    private ICameraMode previousMode;

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
            _simBehaviour.Translation = _simulationTranslationLayer;

            SimulationPreferences.SetRobotSimTranslationLayer(MiraLive.MiraAssembly.Info.GUID, _simulationTranslationLayer);
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

    public SimBehaviour DriveBehaviour { get; private set; }

    private (List<JointInstance> leftWheels, List<JointInstance> rightWheels) _tankTrackWheels = (null, null);

    private Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> _jointMap;
    private List<Rigidbody> _allRigidbodies;

    // SHOOTING/PICKUP
    private GameObject _intakeTrigger;
    private IntakeTriggerData? _intakeData;
    public IntakeTriggerData? IntakeData {
        get => _intakeData;
        set {
            _intakeData = value;
            if (value.HasValue) {
                _intakeTrigger.SetActive(true);
                _intakeTrigger.transform.parent = RobotNode.transform.Find(_intakeData.Value.NodeName);
                _intakeTrigger.transform.localPosition = _intakeData.Value.RelativePosition.ToVector3();
                _intakeTrigger.GetComponent<SphereCollider>().radius = _intakeData.Value.TriggerSize * 0.5f;
            } else {
                _intakeTrigger.SetActive(false);
            }

            SimulationPreferences.SetRobotIntakeTriggerData(MiraLive.MiraAssembly.Info.GUID, _intakeData);
            PreferenceManager.Save();
        }
    }

    private GameObject _trajectoryPointer;
    private ShotTrajectoryData? _trajectoryData;
    public ShotTrajectoryData? TrajectoryData {
        get => _trajectoryData;
        set {
            _trajectoryData = value;
            if (value.HasValue) {
                _trajectoryPointer.transform.parent = RobotNode.transform.Find(_trajectoryData.Value.NodeName);
                _trajectoryPointer.transform.localPosition = _trajectoryData.Value.RelativePosition.ToVector3();
                _trajectoryPointer.transform.localRotation = _trajectoryData.Value.RelativeRotation.ToQuaternion();
            }

            SimulationPreferences.SetRobotTrajectoryData(MiraLive.MiraAssembly.Info.GUID, _trajectoryData);
            PreferenceManager.Save();
        }
    }

    private Queue<GamepieceSimObject> _gamepiecesInPossession = new Queue<GamepieceSimObject>();
    public bool PickingUpGamepieces { get; private set; }

    public RobotSimObject(string name, ControllableState state, MirabufLive miraLive,
            GameObject groundedNode, Dictionary<string, (Joint a, Joint b)> jointMap)
            : base(name, state) {
        MiraLive = miraLive;
        GroundedNode = groundedNode;
        _jointMap = jointMap;
        RobotNode = groundedNode.transform.parent.gameObject;
        RobotBounds = GetBounds(RobotNode.transform);
        GroundedBounds = GetBounds(GroundedNode.transform);
        DebugJointAxes.DebugBounds.Add((GroundedBounds, () => GroundedNode.transform.localToWorldMatrix));
        SimulationPreferences.LoadFromMirabufLive(miraLive);

        _allRigidbodies = new List<Rigidbody>(RobotNode.transform.GetComponentsInChildren<Rigidbody>());
        PhysicsManager.Register(this);

        //tags every mesh collider component in the robot with a tag of robot
        RobotNode.tag = "robot";
        RobotNode.GetComponentsInChildren<MeshCollider>().ForEach(g => g.tag = "robot");

        _intakeTrigger = new GameObject("INTAKE_TRIGGER");
        var trig = _intakeTrigger.AddComponent<SphereCollider>();
        trig.isTrigger = true;
        trig.radius = 0.01f;
        trig.tag = "robot";
        trig.transform.parent = GroundedNode.transform;
        trig.transform.localPosition = Vector3.zero;
        trig.transform.localRotation = Quaternion.identity;
        _trajectoryPointer = new GameObject("TRAJECTORY_POINTER");
        _trajectoryPointer.transform.parent = GroundedNode.transform;
        _trajectoryPointer.transform.position = Vector3.zero;
        _trajectoryPointer.transform.rotation = Quaternion.identity;

        IntakeData = SimulationPreferences.GetRobotIntakeTriggerData(MiraLive.MiraAssembly.Info.GUID);
        TrajectoryData = SimulationPreferences.GetRobotTrajectoryData(MiraLive.MiraAssembly.Info.GUID);
        _simulationTranslationLayer = SimulationPreferences.GetRobotSimTranslationLayer(MiraLive.MiraAssembly.Info.GUID) ?? new RioTranslationLayer();
        // _simulationTranslationLayer = new RioTranslationLayer();

        cam = Camera.main.GetComponent<CameraController>();
    }

    public static void Setup() {
        InputManager.AssignValueInput(INTAKE_GAMEPIECES, TryGetSavedInput(INTAKE_GAMEPIECES, new Digital("E", context: SimulationRunner.RUNNING_SIM_CONTEXT)));

        SimulationRunner.OnUpdate += () => {
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
                bool pickup = InputManager.MappedValueInputs[INTAKE_GAMEPIECES].Value == 1.0F;
                RobotSimObject.GetCurrentlyPossessedRobot().PickingUpGamepieces = pickup;
            }
        };
    }

    private static Analog TryGetSavedInput(string key, Analog defaultInput) {
        if (PreferenceManager.ContainsPreference(key)) {
            var input = (Digital)PreferenceManager.GetPreference<InputData[]>(key)[0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        return defaultInput;
    }

    public void Possess() {
        CurrentlyPossessedRobot = this.Name;
        OrbitCameraMode.FocusPoint =
            () => GroundedNode != null && GroundedBounds != null ? GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center) : Vector3.zero;
    }

    public override void Destroy() {
        ClearGamepieces();
        PhysicsManager.Unregister(this);
        if (CurrentlyPossessedRobot.Equals(this._name)) {
            CurrentlyPossessedRobot = string.Empty;
            // cam.CameraMode = previousMode;
        }
        MonoBehaviour.Destroy(GroundedNode.transform.parent.gameObject);
    }

    public void ClearGamepieces()
    {
        if (_gamepiecesInPossession.Count == 0)
            return;
        _trajectoryPointer.transform.GetChild(0).transform.parent = FieldSimObject.CurrentField.FieldObject.transform;
        _gamepiecesInPossession.Clear();
        // TODO: Should robot handle this or is it expected that whatever calls this will have specific intention to do something else
    }

    public void CollectGamepiece(GamepieceSimObject gp) {
        if (!_intakeData.HasValue || !_trajectoryData.HasValue)
            return;
        
        if (_gamepiecesInPossession.Count >= _intakeData.Value.StorageCapacity)
            return;

        var rb = gp.GamepieceObject.GetComponent<Rigidbody>();
        rb.detectCollisions = false;
        rb.isKinematic = true;
        gp.GamepieceObject.SetActive(false);

        _gamepiecesInPossession.Enqueue(gp);

        gp.IsCurrentlyPossessed = true;
        if (_gamepiecesInPossession.Count == 1)
            UpdateShownGamepiece();
    }

    public void ShootGamepiece() {
        if (_gamepiecesInPossession.Count == 0)
            return;

        // Shoot Gamepiece
        var gp = _gamepiecesInPossession.Dequeue();
        var rb = gp.GamepieceObject.GetComponent<Rigidbody>();
        rb.detectCollisions = true;
        rb.isKinematic = false;
        gp.GamepieceObject.transform.parent = FieldSimObject.CurrentField.FieldObject.transform;
        rb.AddForce(_trajectoryPointer.transform.rotation * Vector3.forward * _trajectoryData.Value.EjectionSpeed, ForceMode.VelocityChange);
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
        gp.GamepieceObject.transform.parent = _trajectoryPointer.transform;
        gp.GamepieceObject.transform.localRotation = Quaternion.identity;
        gp.GamepieceObject.transform.localPosition = Vector3.zero;
        gp.GamepieceObject.transform.localPosition -= gp.GamepieceBounds.center;
        // gp.GamepieceObject.transform.position = _trajectoryPointer.transform.position
        //     - gp.GamepieceObject.transform.localToWorldMatrix.MultiplyPoint(gp.GamepieceBounds.center);
        
    }

    private static Bounds GetBounds(Transform top) {
        Vector3 min = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue), max = new Vector3(float.MinValue,float.MinValue,float.MinValue);
        top.GetComponentsInChildren<Renderer>().ForEach(x => {
            var b = x.bounds;
            if (min.x > b.min.x) min.x = b.min.x;
            if (min.y > b.min.y) min.y = b.min.y;
            if (min.z > b.min.z) min.z = b.min.z;
            if (max.x < b.max.x) max.x = b.max.x;
            if (max.y < b.max.y) max.y = b.max.y;
            if (max.z < b.max.z) max.z = b.max.z;
        });
        return new Bounds(((max + min) / 2f) - top.position, max - min);
    }

    private (List<JointInstance> leftWheels, List<JointInstance> rightWheels) GetLeftRightWheels() {
        if (_tankTrackWheels.leftWheels == null) {
            var wheelsInstances = MiraLive.MiraAssembly.Data.Joints.JointInstances.Where(instance =>
                instance.Value.Info.Name != "grounded"
                && MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData != null
                && MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
                    .TryGetValue("wheel", out var isWheel)
                && isWheel == "true").ToList();

            var leftWheels = new List<JointInstance>();
            var rightWheels = new List<JointInstance>();

            Dictionary<JointInstance, float> wheelDotProducts = new Dictionary<JointInstance, float>();
            foreach (var wheelInstance in wheelsInstances)
            {
                _state.CurrentSignals[wheelInstance.Value.SignalReference].Value = Value.ForNumber(0.0);
                var jointAnchor =
                    (wheelInstance.Value.Offset ?? new Vector3()) +
                    MiraLive.MiraAssembly.Data.Joints.JointDefinitions[wheelInstance.Value.JointReference].Origin ?? new Vector3();
                // jointAnchor = jointAnchor;
                wheelDotProducts[wheelInstance.Value] = Vector3.Dot(Vector3.right, jointAnchor);
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
            wheelsInstances.ForEach(x => {
                var def = MiraLive.MiraAssembly.Data.Joints.JointDefinitions[x.Value.JointReference];
                var jointAxis = new Vector3(def.Rotational.RotationalFreedom.Axis.X, def.Rotational.RotationalFreedom.Axis.Y, def.Rotational.RotationalFreedom.Axis.Z);
                var globalAxis = GroundedNode.transform.rotation
                    * jointAxis.normalized;
                var cross = Vector3.Cross(GroundedNode.transform.up, globalAxis);
                if (MiraLive.MiraAssembly.Info.Version < 5) {
                    if (Vector3.Dot(GroundedNode.transform.forward, cross) > 0) {
                        var ogAxis = jointAxis;

                        
                            ogAxis.x *= -1;
                            ogAxis.y *= -1;
                            ogAxis.z *= -1;
                            // Modify assembly for if a new behaviour evaluates this again
                            // def.Rotational.RotationalFreedom.Axis = ogAxis; // I think this is irrelevant after the last few lines
                            def.Rotational.RotationalFreedom.Axis = new MVector3() { X = jointAxis.x, Y = jointAxis.y, Z = jointAxis.z };
                        
                        var joints = _jointMap[x.Key];
                        (joints.a as HingeJoint).axis = ogAxis;
                        (joints.b as HingeJoint).axis = ogAxis;
                    }
                } else {
                    if (Vector3.Dot(GroundedNode.transform.forward, cross) < 0) {
                        jointAxis.x = -jointAxis.x;
                        var ogAxis = jointAxis;
                        ogAxis.x *= -1;
                        ogAxis.y *= -1;
                        ogAxis.z *= -1;
                        // Modify assembly for if a new behaviour evaluates this again
                        // def.Rotational.RotationalFreedom.Axis = ogAxis; // I think this is irrelevant after the last few lines
                        def.Rotational.RotationalFreedom.Axis = new MVector3() { X = -jointAxis.x, Y = jointAxis.y, Z = jointAxis.z };
                        
                        var joints = _jointMap[x.Key];
                        (joints.a as HingeJoint).axis = ogAxis;
                        (joints.b as HingeJoint).axis = ogAxis;
                    }
                }
            });
            _tankTrackWheels = (leftWheels, rightWheels);
        }

        return _tankTrackWheels;
    }

    public void ConfigureDefaultBehaviours() {
        // ConfigureArcadeDrivetrain();
        ConfigureSwerveDrivetrain();
		ConfigureArmBehaviours();
		ConfigureSliderBehaviours();
        ConfigureTestSimulationBehaviours();
        _simBehaviour.Enabled = false;
    }

    public void ConfigureTestSimulationBehaviours() {
        _simBehaviour = new WSSimBehavior(this.Name, _simulationTranslationLayer);
        SimulationManager.AddBehaviour(this.Name, _simBehaviour);
    }

    // Account for passive joints
    public void ConfigureArmBehaviours() {
        // I pity the poor dev that has to look at this
        var nonWheelInstances = MiraLive.MiraAssembly.Data.Joints.JointInstances.Where(instance =>
                instance.Value.Info.Name != "grounded"
                && (
                    MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData == null
                    || !MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
                        .TryGetValue("wheel", out var isWheel)
                    || isWheel == "false")
                && instance.Value.HasSignal()
                && MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].JointMotionType == JointMotion.Revolute).ToList();
        // nonWheelInstances.ForEach(x => {
        //     var genArmBehaviour = new GeneralArmBehaviour(this.Name, x.Value.SignalReference);
        //     SimulationManager.AddBehaviour(this.Name, genArmBehaviour);
        // });
    }

    public void ConfigureSliderBehaviours() {
        var sliderInstances = MiraLive.MiraAssembly.Data.Joints.JointInstances.Where(instance => 
                instance.Value.Info.Name != "grounded"
                && MiraLive.MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].JointMotionType == JointMotion.Slider
                && instance.Value.HasSignal()).ToList();
        sliderInstances.ForEach(x => {
            var sliderBehaviour = new GeneralSliderBehaviour(this.Name, x.Value.SignalReference);
            SimulationManager.AddBehaviour(this.Name, sliderBehaviour);
        });
    }

    public void ConfigureArcadeDrivetrain() {
        
        var wheels = GetLeftRightWheels();

        var arcadeBehaviour = new ArcadeDriveBehaviour(
            this.Name,
            wheels.leftWheels.Select(j => j.SignalReference).ToList(),
            wheels.rightWheels.Select(j => j.SignalReference).ToList()
        );  
        DriveBehaviour = arcadeBehaviour;

        SimulationManager.AddBehaviour(this.Name, arcadeBehaviour);
    }

    public void ConfigureSwerveDrivetrain() {
        var swerveBehaviour = new SwerveDriveBehaviour(
            this,
            SimulationManager.Drivers[base._name].OfType<RotationalDriver>().Where(x => !x.IsWheel),
            Array.Empty<string>()
        );
        DriveBehaviour = swerveBehaviour;
        
        SimulationManager.AddBehaviour(this.Name, swerveBehaviour);
    }

    public static void SpawnRobot(string filePath) {
        SpawnRobot(filePath, new Vector3(0f, 0.5f, 0f), Quaternion.identity);
    }
    public static void SpawnRobot(string filePath, Vector3 position, Quaternion rotation) {

        // GizmoManager.ExitGizmo();

        var mira = Importer.MirabufAssemblyImport(filePath);
        RobotSimObject simObject = mira.Sim as RobotSimObject;
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        
        
        mira.MainObject.transform.position = position;
        mira.MainObject.transform.rotation = rotation;

        
        //TEMPORARY: CREATING INSTAKE AT FRONT OF THE ROBOT
        // GameObject intake = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // intake.transform.SetParent(simObject.GroundedNode.transform);
        
        // intake.transform.localPosition = new Vector3(0, 0.2f, 0.3f);
        // intake.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        // intake.transform.localRotation = Quaternion.identity;
        
        // intake.GetComponent<Collider>().isTrigger = true;
        // intake.GetComponent<MeshRenderer>().enabled = false;
        // intake.tag = "robot";
        // Shooting.intakeObject = intake;
        


        // TODO: Event call?

        simObject.Possess();

        GizmoManager.SpawnGizmo(simObject);
        // GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, mira.MainObject.transform, mira.MainObject.transform.position);
    }

    private Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)> _preFreezeStates = new Dictionary<Rigidbody, (bool isKine, Vector3 vel, Vector3 angVel)>();
    private bool _isFrozen = false;
    public bool isFrozen()
        => _isFrozen;

    public void Freeze() {
        if (_isFrozen)
            return;

        _allRigidbodies.ForEach(x => {
            _preFreezeStates[x] = (x.isKinematic, x.velocity, x.angularVelocity);
            x.isKinematic = true;
            x.detectCollisions = false;
            x.velocity = Vector3.zero;
            x.angularVelocity = Vector3.zero;
        });

        _isFrozen = true;
    }
    public void Unfreeze() {
        if (!_isFrozen)
            return;

        _allRigidbodies.ForEach(x => {
            var originalState = _preFreezeStates[x];
            x.isKinematic = originalState.isKine;
            x.detectCollisions = true;
            // I think replay might take care of this
            // if (x.velocity != Vector3.zero || x.angularVelocity != Vector3.zero);
        });
        _preFreezeStates.Clear();

        _isFrozen = false;
    }

    public List<Rigidbody> GetAllRigidbodies()
        => _allRigidbodies;

    public GameObject GetRootGameObject() {
        return RobotNode;
    }

    public TransformData GetGizmoData() {

        return new TransformData {
            Position = GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center),
            Rotation = GroundedNode.transform.rotation
        };
    }

    public void Update(TransformData data) {
        // GroundedNode.transform.rotation = data.Rotation;

        /*
        GroundedNode.transform.position -= GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);
        GroundedNode.transform.rotation = Quaternion.identity;

        Matrix4x4 transformation = Matrix4x4.identity;

        // transformation = Matrix4x4.TRS(-GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center), Quaternion.identity, Vector3.one) * transformation;
        transformation = Matrix4x4.TRS(Vector3.zero, data.Rotation, Vector3.one) * transformation;
        // transformation *= Matrix4x4.TRS(data.Position, Quaternion.identity, Vector3.one);

        // Apply Gizmo
        GroundedNode.transform.rotation = transformation.rotation;
        GroundedNode.transform.position -= GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);
        GroundedNode.transform.position += transformation.GetPosition(mod: false);
        */

        RobotNode.transform.rotation = Quaternion.identity;
        RobotNode.transform.position = Vector3.zero;

        RobotNode.transform.rotation = data.Rotation * Quaternion.Inverse(GroundedNode.transform.rotation);
        RobotNode.transform.position = data.Position - GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);

        // GroundedNode.transform.RotateAround()
        // GroundedNode.transform.position = data.Position - GroundedBounds.center;
    }

    public void End(TransformData data)
    {
        PracticeMode.SetInitialState(RobotNode);
    }

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

    public class NewRobotEvent : IEvent {
        public string NewBot;
        public string OldBot;
    }
}

using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Signal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Bounds = UnityEngine.Bounds;
using Vector3 = UnityEngine.Vector3;
using MVector3 = Mirabuf.Vector3;
using Transform = UnityEngine.Transform;
using Synthesis.Import;
using Synthesis.Util;

public class RobotSimObject : SimObject {

    public static string CurrentlyPossessedRobot { get; private set; }

    public static int ControllableJointCounter = 0;

    public Assembly MiraAssembly { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public Bounds GroundedBounds { get; private set; }
    public GameObject RobotNode { get; private set; } // Doesn't work??
    public Bounds RobotBounds { get; private set; }

    public SimBehaviour DriveBehaviour { get; private set; }

    private (List<JointInstance> leftWheels, List<JointInstance> rightWheels) _tankTrackWheels = (null, null);

    private Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> _jointMap;

    public RobotSimObject(string name, ControllableState state, Assembly assembly,
            GameObject groundedNode, Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> jointMap)
            : base(name, state) {
        MiraAssembly = assembly;
        GroundedNode = groundedNode;
        _jointMap = jointMap;
        RobotNode = groundedNode.transform.parent.gameObject;
        RobotBounds = GetBounds(RobotNode.transform);
        GroundedBounds = GetBounds(GroundedNode.transform);
        DebugJointAxes.DebugBounds.Add((GroundedBounds, () => GroundedNode.transform.localToWorldMatrix));
    }

    public void Possess() {
        CurrentlyPossessedRobot = this.Name;
        Camera.main.GetComponent<CameraController>().FocusPoint =
            () => GroundedNode.transform.localToWorldMatrix.MultiplyPoint(GroundedBounds.center);
    }

    public override void Destroy() {
        if (CurrentlyPossessedRobot.Equals(this._name))
            CurrentlyPossessedRobot = string.Empty;
        MonoBehaviour.Destroy(GroundedNode.transform.parent.gameObject);
    }

    private Bounds GetBounds(Transform top) {
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
        return new UnityEngine.Bounds(((max + min) / 2f) - top.position, max - min);
    }

    private (List<JointInstance> leftWheels, List<JointInstance> rightWheels) GetLeftRightWheels() {
        if (_tankTrackWheels.leftWheels == null) {
            var wheelsInstances = MiraAssembly.Data.Joints.JointInstances.Where(instance =>
                instance.Value.Info.Name != "grounded"
                && MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData != null
                && MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
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
                    MiraAssembly.Data.Joints.JointDefinitions[wheelInstance.Value.JointReference].Origin ?? new Vector3();
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
                var def = MiraAssembly.Data.Joints.JointDefinitions[x.Value.JointReference];
                var globalAxis = GroundedNode.transform.rotation
                    * ((Vector3)def.Rotational.RotationalFreedom.Axis).normalized;
                var cross = Vector3.Cross(GroundedNode.transform.up, globalAxis);
                if (Vector3.Dot(GroundedNode.transform.forward, cross) > 0) {
                    var ogAxis = def.Rotational.RotationalFreedom.Axis;
                    ogAxis.X *= -1;
                    ogAxis.Y *= -1;
                    ogAxis.Z *= -1;
                    // Modify assembly for if a new behaviour evaluates this again
                    def.Rotational.RotationalFreedom.Axis = ogAxis; // I think this is irrelevant after the last few lines
                    var joints = _jointMap[x.Key];
                    (joints.a as UnityEngine.HingeJoint).axis = ogAxis;
                    (joints.b as UnityEngine.HingeJoint).axis = ogAxis;
                }
            });
            _tankTrackWheels = (leftWheels, rightWheels);
        }

        return _tankTrackWheels;
    }

    // Account for passive joints
    public void ConfigureArmBehaviours() {
        // I pity the poor dev that has to look at this
        var nonWheelInstances = MiraAssembly.Data.Joints.JointInstances.Where(instance =>
                instance.Value.Info.Name != "grounded"
                && (
                    MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData == null
                    || !MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
                        .TryGetValue("wheel", out var isWheel)
                    || isWheel == "false")
                && instance.Value.HasSignal()
                && MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].JointMotionType == JointMotion.Revolute).ToList();
        nonWheelInstances.ForEach(x => {
            var genArmBehaviour = new GeneralArmBehaviour(this.Name, x.Value.SignalReference);
            SimulationManager.AddBehaviour(this.Name, genArmBehaviour);
        });
    }

    public void ConfigureSliderBehaviours() {
        var sliderInstances = MiraAssembly.Data.Joints.JointInstances.Where(instance => 
                instance.Value.Info.Name != "grounded"
                && MiraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].JointMotionType == JointMotion.Slider
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
        if (DriveBehaviour == null)
            
        DriveBehaviour = arcadeBehaviour;

        SimulationManager.AddBehaviour(this.Name, arcadeBehaviour);
    }
    
    // public void ConfigureTankDrivetrain() {
    //     var wheelsInstances = _miraAssembly.Data.Joints.JointInstances.Where(instance =>
    //         instance.Value.Info.Name != "grounded"
    //         && _miraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData != null
    //         && _miraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
    //             .TryGetValue("wheel", out var isWheel)
    //         && isWheel == "true").ToList();

    //     var leftWheels = new List<JointInstance>();
    //     var rightWheels = new List<JointInstance>();

    //     foreach (var wheelInstance in wheelsInstances)
    //     {
    //         _state.CurrentSignals[wheelInstance.Value.SignalReference].Value = Value.ForNumber(0.0);
    //         var jointAnchor =
    //             (wheelInstance.Value.Offset ?? new Vector3()) +
    //             _miraAssembly.Data.Joints.JointDefinitions[wheelInstance.Value.JointReference].Origin ?? new Vector3();
    //         jointAnchor = _groundedNode.transform.rotation * jointAnchor;
    //         if (Vector3.Dot(Vector3.right, jointAnchor) > 0)
    //         {
    //             rightWheels.Add(wheelInstance.Value);
    //         }
    //         else
    //         {
    //             leftWheels.Add(wheelInstance.Value);
    //         }
    //     }

    //     var arcadeBehaviour = new ArcadeDrive(
    //         _miraAssembly.Info.Name,
    //         leftWheels.Select(j => j.SignalReference).ToList(),
    //         rightWheels.Select(j => j.SignalReference).ToList(),
    //         reversedSideJoints: false
    //     );
    //     _driveBehaviour = arcadeBehaviour;

    //     SimulationManager.AddBehaviour(_miraAssembly.Info.Name, arcadeBehaviour);
    // }

    public static void SpawnRobot(string filePath) {
        SpawnRobot(filePath, new Vector3(0f, 0.5f, 0f), Quaternion.identity);
    }
    public static void SpawnRobot(string filePath, Vector3 position, Quaternion rotation) {

        GizmoManager.ExitGizmo();

        var mira = Importer.MirabufAssemblyImport(filePath);
        RobotSimObject simObject = mira.Sim as RobotSimObject;
        mira.RobotObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.RobotObject.transform.position = position;
        mira.RobotObject.transform.rotation = rotation;

        // Event call maybe?

        // Camera.main.GetComponent<CameraController>().FocusPoint =
        //     () => simObject.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(simObject.GroundedBounds.center);
        simObject.Possess();
        GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, mira.RobotObject.transform, mira.RobotObject.transform.position);
    }
}

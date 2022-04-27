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

public class RobotSimObject : SimObject {

    private Assembly _miraAssembly;
    public Assembly MiraAssembly => _miraAssembly;
    private GameObject _groundedNode;
    public GameObject GroundedNode => _groundedNode;
    private Bounds _groundedBounds;
    public Bounds GroundedBounds => _groundedBounds;
    private GameObject _robotNode;
    public GameObject RobotNode => _robotNode;
    private Bounds _robotBounds;
    public Bounds RobotBounds => _robotBounds;

    private SimBehaviour _driveBehaviour;
    public SimBehaviour DriveBehaviour;

    private Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> _jointMap;

    public RobotSimObject(string name, ControllableState state, Assembly assembly,
            GameObject groundedNode, Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> jointMap)
            : base(name, state) {
        _miraAssembly = assembly;
        _groundedNode = groundedNode;
        _jointMap = jointMap;
        _robotNode = groundedNode.transform.parent.gameObject;
        _robotBounds = GetBounds(_robotNode.transform);
        _groundedBounds = GetBounds(_groundedNode.transform);
        DebugJointAxes.DebugBounds.Add((_groundedBounds, () => _groundedNode.transform.localToWorldMatrix));
    }

    public Bounds GetBounds(Transform top) {
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

    public void ConfigureArcadeDrivetrain() {
        var wheelsInstances = _miraAssembly.Data.Joints.JointInstances.Where(instance =>
            instance.Value.Info.Name != "grounded"
            && _miraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData != null
            && _miraAssembly.Data.Joints.JointDefinitions[instance.Value.JointReference].UserData.Data
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
                _miraAssembly.Data.Joints.JointDefinitions[wheelInstance.Value.JointReference].Origin ?? new Vector3();
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
            var def = _miraAssembly.Data.Joints.JointDefinitions[x.Value.JointReference];
            var globalAxis = _groundedNode.transform.rotation
                * ((Vector3)def.Rotational.RotationalFreedom.Axis).normalized;
            var cross = Vector3.Cross(_groundedNode.transform.up, globalAxis);
            if (Vector3.Dot(_groundedNode.transform.forward, cross) < 0) {
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

        var arcadeBehaviour = new ArcadeDrive(
            _miraAssembly.Info.Name,
            leftWheels.Select(j => j.SignalReference).ToList(),
            rightWheels.Select(j => j.SignalReference).ToList(),
            reversedSideJoints: false
        );
        _driveBehaviour = arcadeBehaviour;

        SimulationManager.AddBehaviour(_miraAssembly.Info.Name, arcadeBehaviour);
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
}

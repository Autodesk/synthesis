using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Behaviors;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Signal;
using Synthesis.ModelManager.Models;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Joint = Mirabuf.Joint.Joint;
using Logger = SynthesisAPI.Utilities.Logger;
using Vector3 = UnityEngine.Vector3;

public class RobotInstance : MonoBehaviour
{

    private Signals? _robotLayout;
    public Info?    Info;

    private bool _init;
    private bool _robotSetup;

    [CanBeNull] private  MapField<string, JointInstance> _instances;
    [CanBeNull] private  MapField<string, Joint>         _joints;
    [CanBeNull] private  Signals                         _signals;
    private              bool                            _reverseSideMotors;

    public void Init(Info info, MapField<string, JointInstance> instances, MapField<string, Joint> joints, Signals layout, bool reversedSideMotors)
    {
        if (_robotLayout == null)
        {
            _robotLayout = layout;
        }
        else
        {
            throw new Exception($"Layout already set for {Info.Name}");
        }

        if (!_init)
        {
            Info = info;
            _instances = instances;
            _joints = joints;
            _signals = layout;
            _reverseSideMotors = reversedSideMotors;
            _init = true;
        }
        else
        {
            throw new Exception($"Info already set for {Info.Name}");
        }
    }

    public void ConfigureDrivebase(Vector3 position, Quaternion rotation, DrivetrainMeta drivetrainMeta)
    {
        if (_init && !_robotSetup)
        {
            if (_signals != null && _instances != null)
            {
                var wheelsInstances = _instances.Where(instance =>
                    instance.Value.Info.Name != "grounded"
                    && _joints?[instance.Value.JointReference].UserData != null
                    && _joints[instance.Value.JointReference].UserData.Data
                        .TryGetValue("wheel", out var isWheel)
                    && isWheel == "true").ToList();

                var leftWheels = new List<JointInstance>();
                var rightWheels = new List<JointInstance>();

                foreach (var wheelInstance in wheelsInstances)
                {
                    SimulationManager.SimulationObjects[Info?.Name].State
                        .CurrentSignals[wheelInstance.Value.SignalReference].Value = Value.ForNumber(0.0);
                    var jointAnchor =
                        (wheelInstance.Value.Offset ?? new Vector3()) +
                        _joints?[wheelInstance.Value.JointReference].Origin ?? new Vector3();
                    jointAnchor = rotation * jointAnchor;
                    jointAnchor.Y = 0;
                    if (Vector3.Dot(Vector3.right, jointAnchor) > 0)
                    {
                        rightWheels.Add(wheelInstance.Value);
                    }
                    else
                    {
                        leftWheels.Add(wheelInstance.Value);
                    }
                }

                SimulationManager.AddBehaviour(Info?.Name, new ArcadeDrive(
                    Info?.Name,
                    leftWheels.Select(j => j.SignalReference).ToList(),
                    rightWheels.Select(j => j.SignalReference).ToList(),
                    reversedSideJoints: _reverseSideMotors));

            }
            else
            {
                Logger.Log($"No joints or signals found for {Info?.Name}. Skipping.");
            }
        }
        else
        {
            if (!_init)
            {
                Logger.Log($"Please initialize robot instance before attempting to setup drive base",LogLevel.Error);
            }
            else
            {
                Logger.Log($"Drivebase already configured; ignoring", LogLevel.Warning);
            }
        }
    }

    void Start()
    {
        Logger.Log($"Successfully initialized {Info?.Name}", LogLevel.Debug);
    }
}


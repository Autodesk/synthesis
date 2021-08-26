using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Signal;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

public class RobotInstance : MonoBehaviour
{

    private Signals? _robotLayout;
    public Info?    Info;

    private List<string> _driveMotors;

    public void SetLayout(Info info, Signals layout, List<string> driveMotors)
    {
        if (_robotLayout == null)
        {
            _robotLayout = layout;
        }
        else
        {
            throw new Exception($"Layout already set for {Info.Name}");
        }

        if (Info == null)
        {
            Info = info;
        }
        else
        {
            throw new Exception($"Info already set for {Info.Name}");
        }

        _driveMotors = driveMotors;

    }

    void Start()
    {
        Logger.Log($"Successfully initialized {Info?.Name}", LogLevel.Debug);
    }
}


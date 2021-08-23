using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Signal;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Simulation;
using UnityEngine;

public class RobotInstance : MonoBehaviour
{

    private Signals? _robotLayout;
    private Info?    _info;

    private List<string> _driveMotors;

    public double speed = 0.0;

    public bool enable;

    public void SetLayout(Info info, Signals layout, List<string> driveMotors)
    {
        if (_robotLayout == null)
        {
            _robotLayout = layout;
        }
        else
        {
            throw new Exception($"Layout already set for {_info.Name}");
        }

        if (_info == null)
        {
            _info = info;
        }
        else
        {
            throw new Exception($"Info already set for {_info.Name}");
        }

        _driveMotors = driveMotors;

    }

    void Start()
    {
        
    }

    void Update()
    {
        if (enable)
        {
            foreach (var wheel in _driveMotors)
            {
                SimulationManager.SimulationObjects[_info.Name].State.CurrentSignals[wheel].Value =
                    Value.ForNumber(speed);
            }
        }
    }

}


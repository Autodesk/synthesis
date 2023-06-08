using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis;
using SynthesisAPI.Simulation;

/// <summary>
/// To counteract weird reactions from the FixedJoints, this behaviour adds a modifier
/// to the forces the CustomWheels use. It's not ideal but without this, all robots with
/// 6 or more wheels would shake uncontrollably.
/// </summary>
public class WheelPhysicsBehaviour : SimBehaviour {

    private RobotSimObject _robot;
    private IEnumerable<WheelDriver> _wheels;

    public WheelPhysicsBehaviour(string simObjectId, RobotSimObject robot): base(simObjectId) {
        _robot = robot;
        _wheels = SimulationManager.Drivers[_robot.Name].OfType<WheelDriver>();

        float radius = _wheels.Average(x => x.Radius);
        _wheels.ForEach(x => x.Radius = radius);
    }
    
    public override void Update() {
        int wheelsInContact = _wheels.Count(x => x.HasContacts);
        float mod = wheelsInContact <= 4 ? 1f : (float)Math.Pow(0.7, (double)(wheelsInContact - 4));
        _wheels.ForEach(x => x.WheelsPhysicsUpdate(mod));
    }
}

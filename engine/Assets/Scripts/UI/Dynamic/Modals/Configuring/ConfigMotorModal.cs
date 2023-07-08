using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;
using Synthesis;

public class ConfigMotorModal : ModalDynamic {
    const float MODAL_HEIGHT = 500f;
    const float MODAL_WIDTH  = 600f;
    const float PADDING      = 16f;
    const float NAME_WIDTH   = 180f;
    const float SCROLL_WIDTH = 10f;

    private ScrollView _scrollView;
    private float _scrollViewWidth;

    private static RobotSimObject _robot;
    private bool _robotISSwerve;
    private ConfigMotor[] _otherMotors;
    private ConfigMotor[] _driveMotors;
    private ConfigMotor[] _turnMotors;
    private WheelDriver[] _driveDrivers;
    private RotationalDriver[] _turnDrivers;

    public ConfigMotorModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: PADDING, rightPadding: PADDING);
        return u;
    };

    public override void Create() {
        _robot         = RobotSimObject.GetCurrentlyPossessedRobot();
        int motorCount = 0;
        _robotISSwerve = _robot.ConfiguredDrivetrainType.Equals(RobotSimObject.DrivetrainType.SWERVE);

        var i = 0;
        // set up drivetrain arrays
        if (_robotISSwerve) {
            _driveMotors  = new ConfigMotor[_robot.modules.Length];
            _driveDrivers = new WheelDriver[_robot.modules.Length];
            for (i = 0; i < _robot.modules.Length; i++) {
                _driveMotors[i]        = new ConfigMotor();
                _driveMotors[i].driver = _robot.modules[i].driver;
                _driveDrivers[i]       = _robot.modules[i].driver;
            }

            _turnMotors  = new ConfigMotor[_robot.modules.Length];
            _turnDrivers = new RotationalDriver[_robot.modules.Length];
            for (i = 0; i < _robot.modules.Length; i++) {
                _turnMotors[i]        = new ConfigMotor();
                _turnMotors[i].driver = _robot.modules[i].azimuth;
                _turnDrivers[i]       = _robot.modules[i].azimuth;
            }

            motorCount -= _turnMotors.Length;
        } else {
            int wheelCount = _robot.GetLeftRightWheels()!.Value.leftWheels.Count +
                             _robot.GetLeftRightWheels()!.Value.rightWheels.Count;
            _driveMotors  = new ConfigMotor[wheelCount];
            _driveDrivers = new WheelDriver[wheelCount];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                _driveMotors[i]        = new ConfigMotor();
                _driveMotors[i].driver = x;
                _driveDrivers[i]       = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                _driveMotors[i]        = new ConfigMotor();
                _driveMotors[i].driver = x;
                _driveDrivers[i]       = x;
                i++;
            });
        }

        // grab original target velocity for drivetrain
        motorCount += SimulationManager.Drivers[_robot.Name].Count - _driveMotors.Length;
        _driveMotors[0].origVel = (_driveMotors[0].driver as WheelDriver).Motor.targetVelocity;
        if (_robotISSwerve) {
            _turnMotors[0].origVel = (_turnMotors[0].driver as RotationalDriver).Motor.targetVelocity;
        }

        Driver[] drivers = new Driver[SimulationManager.Drivers[_robot.Name].Count];
        i                = 0;
        SimulationManager.Drivers[_robot.Name].ForEach(x => { drivers[i] = x; });

        // set up other motors array
        _otherMotors = new ConfigMotor[motorCount];
        i            = 0;
        SimulationManager.Drivers[_robot.Name].ForEach(x => {
            if (Array.IndexOf(_driveDrivers, x) == -1 && (!_robotISSwerve || Array.IndexOf(_turnDrivers, x) == -1)) {
                _otherMotors[i]        = new ConfigMotor();
                _otherMotors[i].driver = x;
                i++;
            }
        });

        // UI
        Title.SetText("Motor Configuration").SetWidth<Label>(MODAL_WIDTH - PADDING * 4);
        Description.SetText("Change motor settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.Save();
            DynamicUIManager.CloseActiveModal();

            // Save to Mira
            if (_driveMotors[0].velChanged) {
                _driveMotors.ForEach(x => { SaveToMira(x.driver as WheelDriver); });
            }
            if (_robotISSwerve && _turnMotors[0].velChanged) {
                _turnMotors.ForEach(x => { SaveToMira(x.driver as RotationalDriver); });
            }
            _otherMotors.ForEach(x => {
                if (x.velChanged) {
                    if (x.driver is WheelDriver) {
                        SaveToMira(x.driver as WheelDriver);
                    } else if (x.driver is RotationalDriver) {
                        SaveToMira(x.driver as RotationalDriver);
                    }
                }
            });
        });

        MiddleButton.SetWidth<Button>(132)
            .SetAnchor<Button>(new Vector2(0.55f, 0), new Vector2(0.55f, 0))
            .StepIntoLabel(b => { b.SetText("Session Save"); })
            .AddOnClickedEvent(x => { DynamicUIManager.CloseActiveModal(); });

        CancelButton.AddOnClickedEvent(b => {
            // change the target velocities back
            if (_driveMotors[0].velChanged) {
                ChangeDriveVelocity(_driveMotors[0].origVel);
            }
            if (_robotISSwerve && _turnMotors[0].velChanged) {
                ChangeTurnVelocity(_turnMotors[0].origVel);
            }
            _otherMotors.ForEach(x => {
                if (x.velChanged) {
                    x.setTargetVelocity(x.origVel);
                }
            });
        });

        (Content nameLabelContent, Content velLabelContent) =
            MainContent.CreateSubContent(new Vector2(MODAL_WIDTH - SCROLL_WIDTH, 20f))
                .SetTopStretch<Content>(PADDING, PADDING, 0)
                .SplitLeftRight(NAME_WIDTH, PADDING);

        nameLabelContent.CreateLabel().SetText("Motor");
        velLabelContent.CreateLabel().SetText("Target Velocity");

        _scrollView =
            MainContent.CreateScrollView()
                .SetRightStretch<ScrollView>()
                .SetTopStretch<ScrollView>(0, 0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
                .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;

        CreateEntry("Drive", (_driveMotors[0].driver as WheelDriver).Motor.targetVelocity, x => ChangeDriveVelocity(x));
        if (_robotISSwerve) {
            CreateEntry(
                "Turn", (_turnMotors[0].driver as RotationalDriver).Motor.targetVelocity, x => ChangeTurnVelocity(x));
        }

        // original target velocities and entry in scrollview for each other motor
        for (i = 0; i < _otherMotors.Length; i++) {
            var driver = _otherMotors[i].driver;
            switch (driver) {
                case (RotationalDriver):
                    _otherMotors[i].origVel = (driver as RotationalDriver).Motor.targetVelocity;
                    break;
                case (WheelDriver):
                    _otherMotors[i].origVel = (driver as WheelDriver).Motor.targetVelocity;
                    break;
            }

            int j = i;
            CreateEntry(i.ToString(), _otherMotors[j].origVel, x => _otherMotors[j].setTargetVelocity(x));
        }
    }

    public override void Update() {}

    public override void Delete() {}

    private void CreateEntry(string name, float currVel, Action<float> onClick) {
        (Content nameContent, Content velContent) =
            _scrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth, 40f))
                .SetTopStretch<Content>(0, 0, 0)
                .ApplyTemplate<Content>(VerticalLayout)
                .SplitLeftRight(NAME_WIDTH, PADDING);
        nameContent.CreateLabel().SetText(name).SetTopStretch(0, PADDING, PADDING + _scrollView.HeightOfChildren);
        velContent.CreateSlider("", minValue: 0f, maxValue: 150f, currentValue: currVel)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren)
            .AddOnValueChangedEvent((s, v) => { onClick(v); });
    }

    private void ChangeDriveVelocity(float vel) {
        foreach (ConfigMotor motor in _driveMotors) {
            motor.setTargetVelocity(vel);
        }
    }

    private void ChangeTurnVelocity(float vel) {
        foreach (ConfigMotor motor in _turnMotors) {
            motor.setTargetVelocity(vel);
        }
    }

    public class ConfigMotor {
        public Driver driver { get; set; }
        public float origVel { get; set; }
        private float _force { get; set; }
        public bool velChanged { get; set; } = false;

        public ConfigMotor() {}

        public void setTargetVelocity(float v) {
            if (driver is RotationalDriver) {
                _force = (driver as RotationalDriver).Motor.force;
                (driver as RotationalDriver).Motor =
                    new JointMotor() { force = _force, freeSpin = false, targetVelocity = v };
            } else if (driver is WheelDriver) {
                _force = (driver as WheelDriver).Motor.force;
                (driver as WheelDriver).Motor =
                    new JointMotor() { force = _force, freeSpin = false, targetVelocity = v };
            }
            velChanged = true;
        }
    }

    private void SaveToMira(WheelDriver driver) {
        _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor {
            SimpleMotor = new Mirabuf.Motor.SimpleMotor {MaxVelocity = driver.Motor.targetVelocity,
                                                         StallTorque                                           = driver.Motor.force}
        };
    }

    private void SaveToMira(RotationalDriver driver) {
        _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor {
            SimpleMotor = new Mirabuf.Motor.SimpleMotor {MaxVelocity = driver.Motor.targetVelocity,
                                                         StallTorque                                           = driver.Motor.force}
        };
    }
}

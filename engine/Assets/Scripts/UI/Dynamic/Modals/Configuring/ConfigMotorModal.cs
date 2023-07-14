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
    private ConfigMotor[] _motors;
    private WheelDriver[] _driveDrivers;
    private RotationalDriver[] _turnDrivers;

    public ConfigMotorModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: PADDING, rightPadding: PADDING);
        return u;
    };

    public override void Create() {
        _robot         = MainHUD.ConfigRobot;
        _robotISSwerve = _robot.ConfiguredDrivetrainType.Equals(RobotSimObject.DrivetrainType.SWERVE);
        _motors        = new ConfigMotor[SimulationManager.Drivers[_robot.Name].Count];

        // set up drivetrain arrays
        var driveMotorCount = 0;
        if (_robotISSwerve) {
            driveMotorCount = _robot.modules.Length;
        } else {
            driveMotorCount = _robot.GetLeftRightWheels()!.Value.leftWheels.Count +
                             _robot.GetLeftRightWheels()!.Value.rightWheels.Count;
        }

        var i = 0;
        if (_robotISSwerve) {
            _driveDrivers = new WheelDriver[driveMotorCount];
            for (i = 0; i < driveMotorCount; i++) {
                _motors[i]        = new ConfigMotor(MotorType.Drive, i);
                _motors[i].driver = _robot.modules[i].driver;
                _driveDrivers[i]  = _robot.modules[i].driver;
            }

            _turnDrivers = new RotationalDriver[driveMotorCount];
            for (i = 0; i < driveMotorCount; i++) {
                _motors[i + driveMotorCount]        = new ConfigMotor(MotorType.Turn, i + driveMotorCount);
                _motors[i + driveMotorCount].driver = _robot.modules[i].azimuth;
                _turnDrivers[i]            = _robot.modules[i].azimuth;
            }
        } else {
            _driveDrivers = new WheelDriver[driveMotorCount];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                _motors[i]        = new ConfigMotor(MotorType.Drive, i);
                _motors[i].driver = x;
                _driveDrivers[i]  = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                _motors[i]        = new ConfigMotor(MotorType.Drive, i);
                _motors[i].driver = x;
                _driveDrivers[i]  = x;
                i++;
            });
        }

        // set up other motors array
        if (_robotISSwerve) {
            i = driveMotorCount * 2;
        } else {
            i = driveMotorCount;
        }

        SimulationManager.Drivers[_robot.Name].ForEach(x => {
            if (Array.IndexOf(_driveDrivers, x) == -1 && (!_robotISSwerve || Array.IndexOf(_turnDrivers, x) == -1)) {
                _motors[i]        = new ConfigMotor(MotorType.Other, i);
                _motors[i].driver = x;
                i++;
            }
        });

        // UI
        Title.SetText("Motor Configuration").SetWidth<Label>(MODAL_WIDTH - PADDING * 4);
        Description.SetText("Change motor settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            DynamicUIManager.CloseActiveModal();

            // Save to Mira
            _motors.ForEach(x => {
                if (x.velChanged) {
                    if (x.driver is WheelDriver) {
                        SaveToMira(x.driver as WheelDriver);
                    } else if (x.driver is RotationalDriver) {
                        SaveToMira(x.driver as RotationalDriver);
                    }
                }
            });

            _robot.MiraLive.Save();
        });

        MiddleButton.SetWidth<Button>(132)
            .SetAnchor<Button>(new Vector2(0.55f, 0), new Vector2(0.55f, 0))
            .StepIntoLabel(b => { b.SetText("Session Save"); })
            .AddOnClickedEvent(x => { DynamicUIManager.CloseActiveModal(); });

        CancelButton.AddOnClickedEvent(b => {
            // change the target velocities back
            _motors.ForEach(x => {
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

        CreateEntry("Drive", (_motors[0].driver as WheelDriver).Motor.targetVelocity, x => ChangeDriveVelocity(x));
        if (_robotISSwerve) {
            CreateEntry(
                "Turn", (_motors[driveMotorCount].driver as RotationalDriver).Motor.targetVelocity, x => ChangeTurnVelocity(x));
        }

        // original target velocities for each motor and entry in scrollview for other motors
        for (i = 0; i < _motors.Length; i++) {
            var driver = _motors[i].driver;
            switch (driver) {
                case (RotationalDriver):
                    _motors[i].origVel = (driver as RotationalDriver).Motor.targetVelocity;
                    break;
                case (WheelDriver):
                    _motors[i].origVel = (driver as WheelDriver).Motor.targetVelocity;
                    break;
            }

            if (_motors[i].motorType == MotorType.Other) {
                int j = i;
                CreateEntry(i.ToString(), _motors[j].origVel, x => _motors[j].setTargetVelocity(x));
            }
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
        foreach (ConfigMotor motor in _motors) {
            if (motor.motorType == MotorType.Drive)
                motor.setTargetVelocity(vel);
        }
    }

    private void ChangeTurnVelocity(float vel) {
        foreach (ConfigMotor motor in _motors) {
            if (motor.motorType == MotorType.Turn)
                motor.setTargetVelocity(vel);
        }
    }

    private class ConfigMotor {
        public int id;
        public Driver driver { get; set; }
        public float origVel { get; set; }
        private float _force { get; set; }
        public bool velChanged { get; set; } = false;
        public MotorType motorType { get; set; }

        public ConfigMotor(MotorType t, int i) {
            motorType = t;
            id = i;        
        }

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

    private enum MotorType {
        Drive,
        Turn,
        Other
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

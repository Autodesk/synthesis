using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;
using Synthesis;
using Utilities.ColorManager;

public class ConfigMotorModal : ModalDynamic {
    const float MODAL_HEIGHT = 500f;
    const float MODAL_WIDTH  = 800f;
    const float PADDING      = 16f;
    const float NAME_WIDTH   = 260f;
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
                _motors[i]        = new ConfigMotor(MotorType.Drive);
                _motors[i].driver = _robot.modules[i].driver;
                _driveDrivers[i]  = _robot.modules[i].driver;
            }

            _turnDrivers = new RotationalDriver[driveMotorCount];
            for (i = 0; i < driveMotorCount; i++) {
                _motors[i + driveMotorCount]        = new ConfigMotor(MotorType.Turn);
                _motors[i + driveMotorCount].driver = _robot.modules[i].azimuth;
                _turnDrivers[i]                     = _robot.modules[i].azimuth;
            }
        } else {
            _driveDrivers = new WheelDriver[driveMotorCount];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                _motors[i]        = new ConfigMotor(MotorType.Drive);
                _motors[i].driver = x;
                _driveDrivers[i]  = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                _motors[i]        = new ConfigMotor(MotorType.Drive);
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
                _motors[i]        = new ConfigMotor(MotorType.Other);
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
                if (x.changed) {
                    SaveToMira(x.driver);
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
                if (x.changed) {
                    x.setForce(x.origForce);
                    x.setTargetVelocity(x.origVel);
                }
            });
        });

        (Content nameLabelContent, Content velLabelContent) =
            MainContent.CreateSubContent(new Vector2(MODAL_WIDTH - SCROLL_WIDTH, 20f))
                .SetTopStretch<Content>(PADDING, PADDING, 0)
                .SplitLeftRight(NAME_WIDTH + PADDING, PADDING);

        nameLabelContent.CreateLabel().SetText("Motor");
        velLabelContent.CreateLabel().SetText("Target Velocity");

        _scrollView =
            MainContent.CreateScrollView()
                .SetRightStretch<ScrollView>()
                .SetTopStretch<ScrollView>(0, 0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
                .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50)
                .ApplyTemplate(VerticalLayout);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;

        CreateEntry("Drive", (_motors[0].driver as WheelDriver).Motor.force, (_motors[0].driver as WheelDriver).Motor.targetVelocity, x => ChangeDriveForce(x), x => ChangeDriveVelocity(x));
        if (_robotISSwerve) {
            CreateEntry("Turn", (_motors[driveMotorCount].driver as RotationalDriver).Motor.force, (_motors[driveMotorCount].driver as RotationalDriver).Motor.targetVelocity,
                x => ChangeTurnForce(x),
                x => ChangeTurnVelocity(x), "RPM", 10.0f);
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
                case (LinearDriver):
                    _motors[i].origVel = (driver as LinearDriver).MaxSpeed;
                    break;
            }

            if (_motors[i].motorType == MotorType.Other) {
                int j = i;
                var u = "RPM";
                if (_motors[i].driver is LinearDriver)
                    u = "M/S";
                CreateEntry(GetName(_motors[i].driver), _motors[j].origForce, _motors[j].origVel, x => _motors[j].setForce(x), x => _motors[j].setTargetVelocity(x), u);
            }
        }
    }

    public override void Update() {}

    public override void Delete() {}

    private void CreateEntry(
        string name, float currForce, float currVel, Action<float> onForce, Action<float> onVelocity, string velUnits = "RPM", float max = 150.0f) {
        (Content nameContent, Content motorContent) =
            _scrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth, PADDING + PADDING + PADDING + 80f))
                .SetTopStretch<Content>(0, 0, 0)
                .SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background)
                .ApplyTemplate(VerticalLayout)
                .SplitLeftRight(NAME_WIDTH, PADDING);
        if (currVel < 5.0f && max > 50.0f)
            max = 50.0f;
        nameContent.CreateLabel().SetText(name).SetTopStretch(PADDING, PADDING, PADDING + 40f + _scrollView.HeightOfChildren);
        motorContent.CreateSubContent(new Vector2(_scrollViewWidth - NAME_WIDTH - PADDING, 40f))
            .SetTopStretch<Content>(0, 0, PADDING + PADDING)
            .CreateSlider($"Target Velocity ({velUnits})", minValue: 0f, maxValue: max, currentValue: currVel)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren + 40f)
            .AddOnValueChangedEvent((s, v) => { onVelocity(v); });
        motorContent.CreateSubContent(new Vector2(_scrollViewWidth - NAME_WIDTH - PADDING, 40f))
            .SetTopStretch<Content>(0, 0, PADDING)
            .CreateSlider("Stall Torque (Nm)", minValue: 0f, maxValue: 6f, currentValue: currForce)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren)
            .AddOnValueChangedEvent((s, f) => { onForce(f); });
    }

    private void ChangeDriveForce(float force) {
        foreach (ConfigMotor motor in _motors) {
            if (motor.motorType == MotorType.Drive)
                motor.setForce(force);
        }
    }

    private void ChangeTurnForce(float force) {
        foreach (ConfigMotor motor in _motors) {
            if (motor.motorType == MotorType.Turn)
                motor.setForce(force);
        }
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
        public Driver driver;
        public float origForce;
        public float origVel;
        private float _force;
        private float _vel;
        public bool changed = false;
        public MotorType motorType;

        public ConfigMotor(MotorType t) {
            motorType = t;
        }

        public void setForce(float f) {
            switch (driver) {
                case RotationalDriver:
                    _vel = (driver as RotationalDriver).Motor.targetVelocity;
                    (driver as RotationalDriver).Motor =
                        new JointMotor() { force = f, freeSpin = false, targetVelocity = _vel};
                    break;
                case WheelDriver:
                    _vel = (driver as WheelDriver).Motor.targetVelocity;
                    (driver as WheelDriver).Motor =
                        new JointMotor() { force = f, freeSpin = false, targetVelocity = _vel };
                    break;
                case LinearDriver:
                    _vel                            = (driver as LinearDriver).Motor.targetVelocity;
                    (driver as LinearDriver).Motor  = new JointMotor() { force = 50, freeSpin = false,
                           targetVelocity = _vel };
                    break;
            }
            changed = true;
        }

        public void setTargetVelocity(float v) {
            switch (driver) {
                case RotationalDriver:
                    _force = (driver as RotationalDriver).Motor.force;
                    (driver as RotationalDriver).Motor =
                        new JointMotor() { force = _force, freeSpin = false, targetVelocity = v };
                    break;
                case WheelDriver:
                    _force = (driver as WheelDriver).Motor.force;
                    (driver as WheelDriver).Motor =
                        new JointMotor() { force = _force, freeSpin = false, targetVelocity = v };
                    break;
                case LinearDriver:
                    (driver as LinearDriver).MaxSpeed = v;
                    _force                            = (driver as LinearDriver).Motor.force;
                    (driver as LinearDriver).Motor    = new JointMotor() { force = _force, freeSpin = false,
                           targetVelocity = v * LinearDriver.LINEAR_TO_MOTOR_VELOCITY };
                    break;
            }
            changed = true;
        }
    }

    private enum MotorType {
        Drive,
        Turn,
        Other
    }

    private string GetName(dynamic driver) {
        return driver.Name;
    }

    private void SaveToMira(dynamic driver) {
        _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor {
    // clang-format off
            SimpleMotor = new Mirabuf.Motor.SimpleMotor {MaxVelocity = driver.Motor.targetVelocity,
                                                         StallTorque = driver.Motor.force}
    // clang-format on
        };
    }
}

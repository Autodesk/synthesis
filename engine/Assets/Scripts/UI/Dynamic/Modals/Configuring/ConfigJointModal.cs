using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;
using Synthesis;
using Utilities.ColorManager;
using Synthesis.PreferenceManager;


public class ConfigJointModal : ModalDynamic {
    const float MODAL_HEIGHT = 500f;
    const float MODAL_WIDTH  = 800f;
    const float PADDING      = 8f;
    const float NAME_WIDTH   = 260f;
    const float SCROLL_WIDTH = 10f;

    private ScrollView _scrollView;
    private float _scrollViewWidth;

    private static RobotSimObject _robot;
    private bool _robotISSwerve;
    private ConfigJoint[] _joints;
    private WheelDriver[] _driveDrivers;
    private RotationalDriver[] _turnDrivers;

    public ConfigJointModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: PADDING, rightPadding: PADDING);
        return u;
    };

    public override void Create() {
        _robot         = MainHUD.SelectedRobot;
        _robotISSwerve = _robot.ConfiguredDrivetrainType.Equals(RobotSimObject.DrivetrainType.SWERVE);
        _joints        = new ConfigJoint[SimulationManager.Drivers[_robot.Name].Count];

        // set up drivetrain arrays
        var driveCount = 0;
        if (_robotISSwerve) {
            driveCount = _robot.modules.Length;
        } else {
            driveCount = _robot.GetLeftRightWheels()!.Value.leftWheels.Count +
                              _robot.GetLeftRightWheels()!.Value.rightWheels.Count;
        }

        var i = 0;
        if (_robotISSwerve) {
            _driveDrivers = new WheelDriver[driveCount];
            for (i = 0; i < driveCount; i++) {
                _joints[i] = new ConfigJoint(JointType.Drive) {
                    driver = _robot.modules[i].driver
                };
                _driveDrivers[i]  = _robot.modules[i].driver;
            }

            _turnDrivers = new RotationalDriver[driveCount];
            for (i = 0; i < driveCount; i++) {
                _joints[i + driveCount] = new ConfigJoint(JointType.Turn) {
                    driver = _robot.modules[i].azimuth
                };
                _turnDrivers[i] = _robot.modules[i].azimuth;
            }
        } else {
            _driveDrivers = new WheelDriver[driveCount];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                _joints[i] = new ConfigJoint(JointType.Drive) {
                    driver = x
                };
                _driveDrivers[i]  = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                _joints[i] = new ConfigJoint(JointType.Drive) {
                    driver = x
                };
                _driveDrivers[i]  = x;
                i++;
            });
        }

        // set up other joints array
        if (_robotISSwerve) {
            i = driveCount * 2;
        } else {
            i = driveCount;
        }

        SimulationManager.Drivers[_robot.Name].ForEach(x => {
            if (Array.IndexOf(_driveDrivers, x) == -1 && (!_robotISSwerve || Array.IndexOf(_turnDrivers, x) == -1)) {
                _joints[i] = new ConfigJoint(JointType.Other) {
                    driver = x
                };
                i++;
            }
        });

        // UI
        Title.SetText("Joint Configuration").SetWidth<Label>(MODAL_WIDTH - PADDING * 4);
        Description.SetText("Change joint settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            DynamicUIManager.CloseActiveModal();

            // Save to Mira
            _joints.ForEach(x => {
                if (x.changed) {
                    SaveMotor(x.driver);
                }
            });

            PreferenceManager.Save();
        });

        MiddleButton.SetWidth<Button>(132)
            .SetAnchor<Button>(new Vector2(0.55f, 0), new Vector2(0.55f, 0))
            .StepIntoLabel(b => { b.SetText("Session Save"); })
            .AddOnClickedEvent(x => { DynamicUIManager.CloseActiveModal(); });

        CancelButton.AddOnClickedEvent(b => {
            // change the target velocities back
            _joints.ForEach(x => {
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

        nameLabelContent.CreateLabel().SetText("Joint");
        velLabelContent.CreateLabel().SetText("Target Velocity");

        _scrollView =
            MainContent.CreateScrollView()
                // .SetRightStretch<ScrollView>()
                // .SetTopStretch<ScrollView>(0, 0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
                .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50)
                .ApplyTemplate(VerticalLayout);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;

        CreateEntry("Drive", (_joints[0].driver as WheelDriver).Motor.force, (_joints[0].driver as WheelDriver).Motor.targetVelocity, x => ChangeDriveForce(x), x => ChangeDriveVelocity(x));
        if (_robotISSwerve) {
            CreateEntry("Turn", (_joints[driveCount].driver as RotationalDriver).Motor.force, (_joints[driveCount].driver as RotationalDriver).Motor.targetVelocity,
                x => ChangeTurnForce(x),
                x => ChangeTurnVelocity(x), "RPM", 10.0f);
        }

        // original target velocities for each joint and entry in scrollview for other joints
        for (i = 0; i < _joints.Length; i++) {
            var driver = _joints[i].driver;
            switch (driver) {
                case RotationalDriver:
                    _joints[i].origForce = (driver as RotationalDriver).Motor.force;
                    _joints[i].origVel = (driver as RotationalDriver).Motor.targetVelocity;
                    break;
                case WheelDriver:
                    _joints[i].origForce = (driver as WheelDriver).Motor.force;
                    _joints[i].origVel = (driver as WheelDriver).Motor.targetVelocity;
                    break;
                case LinearDriver:
                    _joints[i].origForce = (driver as LinearDriver).Motor.force;
                    _joints[i].origVel = (driver as LinearDriver).MaxSpeed;
                    break;
            }

            if (_joints[i].jointType == JointType.Other) {
                int j = i;
                var u = "RPM";
                if (_joints[i].driver is LinearDriver)
                    u = "CM/S";
                CreateEntry(GetName(_joints[i].driver), _joints[j].origForce, _joints[j].origVel, x => _joints[j].setForce(x), x => _joints[j].setTargetVelocity(x), u);
            }
        }
        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(-_scrollView.Content.RectOfChildren().yMin + PADDING);

    }

    public override void Update() {}

    public override void Delete() {}

    private void CreateEntry(
        string name, float currForce, float currVel, Action<float> onForce, Action<float> onVelocity, string velUnits = "RPM", float max = 150.0f) {
        Content entry =
            _scrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth - 20, PADDING + PADDING + PADDING + 80f))
                .SetTopStretch<Content>(0, 20, 0)
                .SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background)
                .ApplyTemplate(VerticalLayout);
        (Content nameContent, Content jointContent) = entry.SplitLeftRight(NAME_WIDTH, PADDING);
        if (currVel < 5.0f && max > 50.0f)
            max = 50.0f;
        nameContent.CreateLabel().SetText(name).SetTopStretch(PADDING, PADDING, PADDING / 2 + 40f + _scrollView.HeightOfChildren);
        Debug.Log($"curr {currForce}");
        jointContent.CreateSubContent(new Vector2(_scrollViewWidth - NAME_WIDTH - PADDING, 40f))
            .SetTopStretch<Content>(0, 0, PADDING + PADDING)
            .CreateSlider($"Target Velocity ({velUnits})", minValue: 0f, maxValue: max, currentValue: currVel)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren + 40f)
            .AddOnValueChangedEvent((s, v) => { onVelocity(v); });
        jointContent.CreateSubContent(new Vector2(_scrollViewWidth - NAME_WIDTH - PADDING, 40f))
            .SetTopStretch<Content>(0, 0, PADDING)
            .CreateSlider("Stall Torque (Nm)", minValue: 0f, maxValue: 50f, currentValue: currForce)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren)
            .AddOnValueChangedEvent((s, f) => { onForce(f); });
    }

    private void ChangeDriveForce(float force) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Drive)
                joint.setForce(force);
        }
    }

    private void ChangeTurnForce(float force) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Turn)
                joint.setForce(force);
        }
    }

    private void ChangeDriveVelocity(float vel) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Drive)
                joint.setTargetVelocity(vel);
        }
    }

    private void ChangeTurnVelocity(float vel) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Turn)
                joint.setTargetVelocity(vel);
        }
    }

    private class ConfigJoint {
        public Driver driver;
        public float origForce;
        public float origVel;
        private float _force;
        private float _vel;
        public bool changed = false;
        public JointType jointType;

        public ConfigJoint(JointType t) {
            jointType = t;
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
                    (driver as LinearDriver).Motor  = new JointMotor() { force = f, freeSpin = false,
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
                    (driver as LinearDriver).MaxSpeed = v / 100;
                    _force                            = (driver as LinearDriver).Motor.force;
                    (driver as LinearDriver).Motor    = new JointMotor() { force = _force, freeSpin = false,
                           targetVelocity = v / 100 * LinearDriver.LINEAR_TO_MOTOR_VELOCITY };
                    break;
            }
            changed = true;
        }
    }

    private enum JointType {
        Drive,
        Turn,
        Other
    }

    private string GetName(dynamic driver) {
        return driver.Name;
    }

    private void SaveMotor(dynamic driver) {
        var motor = new JointMotor() { targetVelocity = driver.Motor.targetVelocity, force = driver.Motor.force };

        SimulationPreferences.SetRobotJointMotor(_robot.RobotGUID, driver.MotorRef, motor);
    }
}

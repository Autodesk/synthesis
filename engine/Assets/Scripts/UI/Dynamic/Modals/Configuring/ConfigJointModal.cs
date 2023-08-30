using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;
using Synthesis;
using Utilities.ColorManager;
using Synthesis.PreferenceManager;

public class ConfigJointModal : ModalDynamic {
    const float MODAL_HEIGHT     = 500f;
    const float MODAL_WIDTH      = 800f;
    const float PADDING          = 8f;
    const float NAME_WIDTH       = 260f;
    const float SCROLL_WIDTH     = 10f;
    const float RPM_TO_RADPERSEC = Mathf.PI / 30f;

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
                _joints[i]       = new ConfigJoint(JointType.Drive) { driver = _robot.modules[i].driver };
                _driveDrivers[i] = _robot.modules[i].driver;
            }

            _turnDrivers = new RotationalDriver[driveCount];
            for (i = 0; i < driveCount; i++) {
                _joints[i + driveCount] = new ConfigJoint(JointType.Turn) { driver = _robot.modules[i].azimuth };
                _turnDrivers[i]         = _robot.modules[i].azimuth;
            }
        } else {
            _driveDrivers = new WheelDriver[driveCount];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                _joints[i]       = new ConfigJoint(JointType.Drive) { driver = x };
                _driveDrivers[i] = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                _joints[i]       = new ConfigJoint(JointType.Drive) { driver = x };
                _driveDrivers[i] = x;
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
                _joints[i] = new ConfigJoint(JointType.Other) { driver = x };
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
                    x.setMaxAcceleration(x.origAcc);
                    x.setMaxVelocity(x.origVel);
                }
            });
        });

        (Content nameLabelContent, Content jointLabelContent) =
            MainContent.CreateSubContent(new Vector2(MODAL_WIDTH - SCROLL_WIDTH, 20f))
                .SetTopStretch<Content>(PADDING, PADDING, 0)
                .SplitLeftRight(NAME_WIDTH + PADDING, PADDING);

        nameLabelContent.CreateLabel().SetText("Joint").SetTopStretch(PADDING * 2, PADDING);
        (Content velLabelContent, Content accLabelContent) =
            jointLabelContent.SplitLeftRight((MODAL_WIDTH - SCROLL_WIDTH - NAME_WIDTH - PADDING) / 2, PADDING);
        velLabelContent.CreateLabel().SetText("Max Velocity").SetTopStretch(PADDING * 3, PADDING);
        accLabelContent.CreateLabel().SetText("Max Acceleration").SetTopStretch(PADDING * 2, PADDING);

        _scrollView =
            MainContent
                .CreateScrollView()
                // .SetRightStretch<ScrollView>()
                // .SetTopStretch<ScrollView>(0, 0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
                .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50)
                .ApplyTemplate(VerticalLayout);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;

        if (_joints[0].driver is WheelDriver) {
            CreateEntry("Drive", (_joints[0].driver as WheelDriver).Motor.force / RPM_TO_RADPERSEC,
                (_joints[0].driver as WheelDriver).Motor.targetVelocity / RPM_TO_RADPERSEC, false, x => ChangeDriveAcc(x),
                x => ChangeDriveVelocity(x));
            if (_robotISSwerve) {
                CreateEntry("Turn", (_joints[driveCount].driver as RotationalDriver).Motor.force,
                    (_joints[driveCount].driver as RotationalDriver).Motor.targetVelocity, true, x => ChangeTurnAcc(x),
                    x => ChangeTurnVelocity(x), "RPM");
            }
        }

        // original target velocities for each joint and entry in scrollview for other joints
        for (i = 0; i < _joints.Length; i++) {
            var driver = _joints[i].driver;
            switch (driver) {
                case RotationalDriver:
                    _joints[i].origAcc = (driver as RotationalDriver).Motor.force;
                    _joints[i].origVel = (driver as RotationalDriver).Motor.targetVelocity;
                    break;
                case WheelDriver:
                    _joints[i].origAcc = (driver as WheelDriver).Motor.force / RPM_TO_RADPERSEC;
                    _joints[i].origVel = (driver as WheelDriver).Motor.targetVelocity / RPM_TO_RADPERSEC;
                    break;
                case LinearDriver:
                    _joints[i].origAcc = (driver as LinearDriver).Motor.force * 100;
                    _joints[i].origVel = (driver as LinearDriver).Motor.targetVelocity * 100;
                    break;
            }

            if (_joints[i].jointType == JointType.Other) {
                int j = i;
                string u;
                switch (_joints[i].driver) {
                    case LinearDriver:
                        u = "CM/S";
                        break;
                    case RotationalDriver:
                        u = "RAD/S";
                        break;
                    default:
                        u = "RPM";
                        break;
                }
                CreateEntry(GetName(_joints[i].driver), _joints[j].origAcc, _joints[j].origVel, !(_joints[i].driver is WheelDriver),
                    x => _joints[j].setMaxAcceleration(x), x => _joints[j].setMaxVelocity(x), u);
            }
        }
        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(
            -_scrollView.Content.RectOfChildren().yMin + PADDING);
    }

    public override void Update() {}

    public override void Delete() {}

    private void CreateEntry(string name, float currAcc, float currVel, bool includeAcc, Action<float> onAcc, Action<float> onVel,
        string velUnits = "RPM") {
        Content entry =
            _scrollView.Content.CreateSubContent(new Vector2(_scrollViewWidth - 20, PADDING + PADDING + PADDING + 50f))
                .SetTopStretch<Content>(0, 20, 0)
                .SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background)
                .ApplyTemplate(VerticalLayout);
        (Content nameContent, Content jointContent) = entry.SplitLeftRight(NAME_WIDTH, PADDING);
        nameContent.CreateLabel().SetText(name).SetTopStretch(
            PADDING, PADDING, PADDING / 2 + 25 + _scrollView.HeightOfChildren);

        (Content velContent, Content accContent) =
            jointContent.SplitLeftRight((_scrollViewWidth - NAME_WIDTH - PADDING) / 2, PADDING);

        velContent.CreateNumberInputField()
            .StepIntoLabel(l => l.SetText(velUnits))
            .StepIntoHint(h => h.SetText("Velocity"))
            .SetValue((int) currVel)
            .ApplyTemplate(VerticalLayout)
            .AddOnValueChangedEvent((s, v) => { onVel(v); });
        if (includeAcc) 
            accContent.CreateNumberInputField()
                .StepIntoLabel(l => l.SetText($"{velUnits}/S"))
                .StepIntoHint(h => h.SetText("Acceleration"))
                .SetValue((int) currAcc)
                .ApplyTemplate(VerticalLayout)
                .AddOnValueChangedEvent((s, f) => { onAcc(f); });
    }

    private void ChangeDriveAcc(float acc) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Drive)
                joint.setMaxAcceleration(acc);
        }
    }

    private void ChangeTurnAcc(float acc) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Turn)
                joint.setMaxAcceleration(acc);
        }
    }

    private void ChangeDriveVelocity(float vel) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Drive)
                joint.setMaxVelocity(vel);
        }
    }

    private void ChangeTurnVelocity(float vel) {
        foreach (ConfigJoint joint in _joints) {
            if (joint.jointType == JointType.Turn)
                joint.setMaxVelocity(vel);
        }
    }

    private class ConfigJoint {
        public Driver driver;
        public float origAcc;
        public float origVel;
        private float _acc;
        private float _vel;
        public bool changed = false;
        public JointType jointType;

        public ConfigJoint(JointType t) {
            jointType = t;
        }

        public void setMaxAcceleration(float a) {
            switch (driver) {
                case RotationalDriver:
                    _vel = (driver as RotationalDriver).Motor.targetVelocity;
                    (driver as RotationalDriver).Motor =
                        new JointMotor() { force = a, freeSpin = false, targetVelocity = _vel };
                    break;
                case WheelDriver:
                    _vel = (driver as WheelDriver).Motor.targetVelocity;
                    (driver as WheelDriver).Motor =
                        new JointMotor() { force = a * RPM_TO_RADPERSEC, freeSpin = false, targetVelocity = _vel };
                    break;
                case LinearDriver:
                    _vel = (driver as LinearDriver).Motor.targetVelocity;
                    (driver as LinearDriver).Motor =
                        new JointMotor() { force = a / 100, freeSpin = false, targetVelocity = _vel };
                    break;
            }
            changed = true;
        }

        public void setMaxVelocity(float v) {
            switch (driver) {
                case RotationalDriver:
                    _acc = (driver as RotationalDriver).Motor.force;
                    (driver as RotationalDriver).Motor =
                        new JointMotor() { force = _acc, freeSpin = false, targetVelocity = v };
                    break;
                case WheelDriver:
                    _acc = (driver as WheelDriver).Motor.force;
                    (driver as WheelDriver).Motor =
                        new JointMotor() { force = _acc, freeSpin = false, targetVelocity = v * RPM_TO_RADPERSEC };
                    break;
                case LinearDriver:
                    _acc = (driver as LinearDriver).Motor.force;
                    (driver as LinearDriver).Motor =
                        new JointMotor() { force = _acc, freeSpin = false, targetVelocity = v / 100 };
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

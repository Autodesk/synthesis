using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;
using Synthesis;


public class ConfigMotorModal : ModalDynamic {
    const float MODAL_HEIGHT = 500f;
    const float MODAL_WIDTH = 600f;
    const float PADDING = 16f;
    const float MOTOR_HEIGHT = 40f;
    const float NAME_WIDTH = 180f;
    const float SCROLL_WIDTH = 10f;

    private ScrollView _scrollView;
    private float _scrollViewWidth;

    private Slider velSlider;

    private static RobotSimObject _robot;
    private float force;
    private CMotor[] motors;
    private WheelDriver[] driveWheels;
    private RotationalDriver[] turnWheels;
    private float origDVel;
    private bool driveChanged = false;
    private bool turnChanged = false;

    

    public ConfigMotorModal(): base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: PADDING, rightPadding: PADDING);
        return u;
    };

    public override void Create() {
        _robot = RobotSimObject.GetCurrentlyPossessedRobot();
        int motorCount = 0;

        var i = 0;
        if (_robot.ConfiguredDrivetrainType.Equals(RobotSimObject.DrivetrainType.SWERVE)) {
            driveWheels = new WheelDriver[_robot.modules.Length];
            for (i = 0; i < _robot.modules.Length; i++) {
                driveWheels[i] = _robot.modules[i].driver;
            }
            
            turnWheels = new RotationalDriver[_robot.modules.Length];
            for (i = 0; i < _robot.modules.Length; i++) {
                turnWheels[i] = _robot.modules[i].azimuth;
            }

            motorCount -= turnWheels.Length;
        } else {
            driveWheels = new WheelDriver[_robot.GetLeftRightWheels()!.Value.leftWheels.Count + _robot.GetLeftRightWheels()!.Value.rightWheels.Count];
            _robot.GetLeftRightWheels()!.Value.leftWheels.ForEach(x => {
                driveWheels[i] = x;
                i++;
            });
            _robot.GetLeftRightWheels()!.Value.rightWheels.ForEach(x => {
                driveWheels[i] = x;
                i++;
            });

            turnWheels = new RotationalDriver[0];
        }
        motorCount += SimulationManager.Drivers[_robot.Name].Count - driveWheels.Length;
        origDVel = driveWheels[0].Motor.targetVelocity;

        motors = new CMotor[motorCount];
        for (i = 0; i < motorCount; i++) {
            motors[i] = new CMotor(i);
        }

        i = 0;
        SimulationManager.Drivers[_robot.Name].ForEach(x => { 
            if (Array.IndexOf(driveWheels, x) == -1 && Array.IndexOf(turnWheels, x) == -1) {
                motors[i].driver = x;
                i++;
            }
        });


        Title.SetText("Motor Configuration").SetWidth<Label>(MODAL_WIDTH - PADDING * 4);
        Description.SetText("Change motor settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.Save();
            DynamicUIManager.CloseActiveModal();
        });

        CancelButton.AddOnClickedEvent(b => {
            if (driveWheels[0].Motor.targetVelocity != origDVel) {
                ChangeDriveVel(origDVel);
            }

            motors.ForEach(x => {
                if (x.velChanged) {
                    x.setTarVel(x.origVel);
                }
            });
        });

        (Content nameLabelContent, Content velLabelContent) = MainContent.CreateSubContent(new Vector2(MODAL_WIDTH - SCROLL_WIDTH, 20f))
            .SetTopStretch<Content>(PADDING, PADDING, 0)
            .SplitLeftRight(NAME_WIDTH, PADDING);

        nameLabelContent.CreateLabel().SetText("Motor");
        velLabelContent.CreateLabel().SetText("Target Velocity");
        
        _scrollView = MainContent.CreateScrollView().SetRightStretch<ScrollView>()
            .SetTopStretch<ScrollView>(0,0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
            .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;
    
        CreateEntry("Drive", driveWheels[0].Motor.targetVelocity, x => ChangeDriveVel(x));
        if (_robot.ConfiguredDrivetrainType.Equals(RobotSimObject.DrivetrainType.SWERVE))
            CreateEntry("Turn", turnWheels[0].Motor.targetVelocity, x => ChangeTurnVel(x));

        for (i = 0; i < motors.Length; i++) {
            var driver = motors[i].driver;
            switch(driver) {
                case(RotationalDriver): 
                    motors[i].origVel = (driver as RotationalDriver).Motor.targetVelocity;
                    break;
                case(WheelDriver):
                    motors[i].origVel = (driver as WheelDriver).Motor.targetVelocity;
                    break;
            }

            int j = i;
            CreateEntry(i.ToString(), motors[j].origVel, x => motors[j].setTarVel(x));
        }
    }

    public override void Update() { }
    public override void Delete() { }

    private void CreateEntry(string name, float currVel, Action<float> onClick) {
        (Content nameContent, Content velContent) = _scrollView.Content
                .CreateSubContent(new Vector2(_scrollViewWidth, MOTOR_HEIGHT))
                .SetTopStretch<Content>(0,0,0)
                .ApplyTemplate<Content>(VerticalLayout)
                .SplitLeftRight(NAME_WIDTH, PADDING);
            nameContent.CreateLabel().SetText(name)
                .SetTopStretch(0, PADDING, PADDING + _scrollView.HeightOfChildren);
            velContent.CreateSlider("", minValue: 0f, maxValue: 180f, currentValue: currVel)
                .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren)
                .AddOnValueChangedEvent((s, v) => {
                    onClick(v);
            });
    } 

    private void ChangeDriveVel(float vel) {
        force = driveWheels[0].Motor.force;


        foreach (WheelDriver driver in driveWheels) {

            driver.Motor = new JointMotor() {
                force =  force,
                freeSpin = false,
                targetVelocity = vel
            };

            _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
                MaxVelocity = vel,
                StallTorque = force
            }};
        }
    }

    private void ChangeTurnVel(float vel)  {
        force = turnWheels[0].Motor.force;

        foreach (RotationalDriver driver in turnWheels) {

            driver.Motor = new JointMotor() {
                force =  force,
                freeSpin = false,
                targetVelocity = vel
            };

            _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
                MaxVelocity = vel,
                StallTorque = force
            }};
            
        }
    }

    public class CMotor {
        public int id {get; set;}
        public Driver driver {get; set;}
        public float origVel {get; set;}
        private float _force {get; set;}
        public bool velChanged {get; set;} = false;

        public CMotor(int i) {
            id = i;
        }

        public void setTarVel(float v) {
            if (driver is RotationalDriver) {
                _force = (driver as RotationalDriver).Motor.force;
                (driver as RotationalDriver).Motor = new JointMotor() {
                    force = _force,
                    freeSpin = false,
                    targetVelocity = v
                };

                _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[(driver as RotationalDriver).MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
                    MaxVelocity = v,
                    StallTorque = _force
                }};

                velChanged = true;
            } else if (driver is WheelDriver) {
                _force = (driver as WheelDriver).Motor.force;
                (driver as WheelDriver).Motor = new JointMotor() {
                    force = _force,
                    freeSpin = false,
                    targetVelocity = v
                };

                _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[(driver as WheelDriver).MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
                    MaxVelocity = v,
                    StallTorque = _force
                }};

                velChanged = true;
            } else {
                Debug.Log("Driver not supported");
            }
        }
    }

    private void SaveToMira(WheelDriver driver, float v) {
        float _force = driver.Motor.force;
        _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
            MaxVelocity = v,
            StallTorque = _force
        }};
    }

    private void SaveToMira(RotationalDriver driver, float v) {
        float _force = driver.Motor.force;
        _robot.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions[driver.MotorRef] = new Mirabuf.Motor.Motor{ SimpleMotor = new Mirabuf.Motor.SimpleMotor{
            MaxVelocity = v,
            StallTorque = _force
        }};
    }

}


/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <WPILib.h>
#include <iostream>

class Robot: public frc::IterativeRobot{
    frc::Spark m_leftMotor{0};
    frc::Spark m_rightMotor{1};
    frc::DifferentialDrive m_robotDrive{m_leftMotor, m_rightMotor};
    frc::Joystick m_stick{0};
    frc::Timer auto_timer;

    bool driveMode = false;

public:
    void RobotInit(){}
    void DisabledInit(){}
    void TeleopInit(){}
    void TestInit(){}
    void RobotPeriodic(){}
    void DisabledPeriodic(){}
    void TestPeriodic(){}

    void AutonomousInit(){
        auto_timer.Start();
    }

    void AutonomousPeriodic(){
        if(!auto_timer.HasPeriodPassed(5)){
            m_robotDrive.TankDrive(1.0,1.0);
        }
    }

    void TeleopPeriodic(){
        //std::cout<<"Joystick forward:"<<(-m_stick.GetY())<<" rotate:"<<m_stick.GetX()<<"\n";
        if(!driveMode)
            m_robotDrive.ArcadeDrive(-m_stick.GetRawAxis(1), m_stick.GetRawAxis(4));
        else
            m_robotDrive.TankDrive(m_stick.GetRawAxis(1), m_stick.GetRawAxis(5));
        if(m_stick.GetRawButton(2)) {
            driveMode = !driveMode;
        }
        frc::Wait(0.005);
    }
};

START_ROBOT_CLASS(Robot)

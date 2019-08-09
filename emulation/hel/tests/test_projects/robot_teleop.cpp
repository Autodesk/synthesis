/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <frc/WPILib.h>
#include <iostream>
//#include "ctre/Phoenix.h"

class Robot: public frc::TimedRobot{
    frc::Spark m_leftMotor{0};
    frc::Spark m_rightMotor{1};
    frc::DifferentialDrive m_robotDrive{m_leftMotor, m_rightMotor};
    frc::Joystick m_stick{0};
    frc::Encoder encoder1{0,1};
    frc::Encoder encoder2{2,3};
    frc::Timer auto_timer;
    bool run_auto = true;

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
        std::cout<<"AutonomousInit\n";
        encoder1.Reset();
    }

    void AutonomousPeriodic(){
        std::cout<<"Encoder raw: "<<encoder1.GetRaw()<<"\n";
        if(abs(encoder1.GetRaw()) < 20){
            m_robotDrive.TankDrive(1.0,1.0);
        } else{
            m_robotDrive.TankDrive(0,0);
        }
    }

    void TeleopPeriodic(){
        double start = frc::Timer::GetFPGATimestamp();
        //std::cout << "1: "<< m_stick.GetRawAxis(1) << " 4: " << m_stick.GetRawAxis(4)<< "\n";
        if(!driveMode)
            m_robotDrive.ArcadeDrive(-m_stick.GetRawAxis(1), m_stick.GetRawAxis(5));
        else
            m_robotDrive.TankDrive(m_stick.GetRawAxis(1), m_stick.GetRawAxis(6));
        if(m_stick.GetRawButton(2)) {
            driveMode = !driveMode;
        }
        frc::Wait(0.005);
        std::cout<<"Loop time: "<<(frc::Timer::GetFPGATimestamp() - start)<<" s\n";
    }
};

START_ROBOT_CLASS(Robot)

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

public:
    void RobotInit(){}

    void TeleopInit(){}

    void TeleopPeriodic(){
        //std::cout<<"Joystick forward:"<<(-m_stick.GetY())<<" rotate:"<<m_stick.GetX()<<"\n";
        m_robotDrive.ArcadeDrive(-m_stick.GetY(), m_stick.GetX());

        frc::Wait(0.005);
    }
};

START_ROBOT_CLASS(Robot)

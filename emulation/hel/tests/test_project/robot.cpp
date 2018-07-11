/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <Drive/DifferentialDrive.h>
#include <IterativeRobot.h>
#include <Joystick.h>
#include <Spark.h>
#include <cstdlib>
#include <ctime>
#include <iostream>
#include <Timer.h>
/**
 * This is a demo program showing the use of the DifferentialDrive class.
 * Runs the motors with arcade steering.
 */
class Robot : public frc::IterativeRobot {
	frc::Spark m_leftMotor{0};
	frc::Spark m_rightMotor{1};
	frc::DifferentialDrive m_robotDrive{m_leftMotor, m_rightMotor};
	frc::Joystick m_stick{0};

    float newPWMR = 0.0;
    float newPWML = 0.0;

   public:
	void TeleopPeriodic() {
        std::srand(std::time(nullptr));
        // drive with arcade style
        newPWML = ((float)((std::rand())/((RAND_MAX + 1u)/2000)-1000)/1000.0f);
        newPWMR = ((float)((std::rand())/((RAND_MAX + 1u)/2000)-1000)/1000.0f);
        //m_robotDrive.ArcadeDrive(m_stick.GetY(), m_stick.GetX());

        m_leftMotor.Set(newPWML);
        m_rightMotor.Set(newPWMR);

        std::cout << "Excepted Left: " << newPWML << "\nExcepted Right: " << newPWMR << "\n";
        std::cout << "Actual Left: " << m_leftMotor.Get() << "\nActual Right: " << m_rightMotor.Get() << "\n";

        Wait(0.045);
    }
};

START_ROBOT_CLASS(Robot)

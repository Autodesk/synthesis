/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <WPILib.h>
#include <cstdlib>
#include <ctime>
#include <iostream>
#include <unistd.h>
/**
 * This is a demo program showing the use of the DifferentialDrive class.
 * Runs the motors with arcade steering.
 */
class Robot : public frc::IterativeRobot {
    frc::Spark m_leftMotor{0};
    frc::Spark m_rightMotor{1};
    frc::DifferentialDrive m_robotDrive{m_leftMotor, m_rightMotor};
    frc::Joystick m_stick{0};
    frc::DigitalOutput dio{11};
    frc::Relay r{0};

    bool current_state = false;

    float newPWMR = 0.0;
    float newPWML = 0.0;

public:
    void TeleopInit() {
        std::srand(std::time(nullptr));
    }
    void TeleopPeriodic() {
        // drive with arcade style
        auto x = std::rand() % 2000 + (-1000);
        auto y = std::rand()% 2000 + (-1000);

        m_robotDrive.ArcadeDrive(x/1000.0f, y/1000.0f);


        //std::cout << "Left Speed: " << m_leftMotor.GetSpeed() << "\nRight Speed: " << m_rightMotor.GetSpeed() << "\n";
        dio.Set(current_state);
        std::cout << "Current State: " << current_state << "\n\n\n\n\n";
        r.Set(frc::Relay::Value::kForward);
        current_state = !current_state;
        usleep(45000);
    }
};

START_ROBOT_CLASS(Robot)

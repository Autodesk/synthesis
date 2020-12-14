/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <frc/WPILib.h>
#include <cstdlib>
#include <ctime>
#include <iostream>
#include <unistd.h>
//#include "ctre/Phoenix.h"

class Robot : public frc::TimedRobot {
    // ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon{1};

    bool current_state = false;

    double power = -1.0; //(std::rand() % 2000 - 1000) / 1000.0;

public:
    void RobotInit(){}

    void RobotPeriodic(){}

    void TeleopInit() {
        std::srand(std::time(nullptr));
    }

    void TeleopPeriodic() {
        if(power >= 1.0){
            power = -1.0;
        } else {
            power += 0.005;
        }
        std::cout<<"Setting power to : "<<power<<"\n";
        //talon.Set(power);

        usleep(45000);
    }
};

START_ROBOT_CLASS(Robot)

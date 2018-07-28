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
#include "ctre/Phoenix.h"

class Robot : public frc::IterativeRobot {
    ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon{1};

    bool current_state = false;

public:
    void RobotInit(){}

    void RobotPeriodic(){}

    void TeleopInit() {
        std::srand(std::time(nullptr));
    }

    void TeleopPeriodic() {
        double power = 1.0; //(std::rand() % 2000 - 1000) / 1000.0;

        talon.Set(power);

        //exit(0);
        //std::cout<<"Power: "<<power<<" Talon:"<<talon.GetBusVoltage()<<"\n";

        usleep(45000);
    }
};

START_ROBOT_CLASS(Robot)

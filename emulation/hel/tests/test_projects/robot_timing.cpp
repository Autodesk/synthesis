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
#include <chrono>

#define NOW std::chrono::duration_cast<std::chrono::nanoseconds>(std::chrono::high_resolution_clock::now().time_since_epoch()).count()

#define TIME_IT(X) \
	start = NOW;\
	X;\
	printf("\"%s\" took:%f milliseconds\n",#X,(NOW - start)/1E6)


class Robot : public frc::TimedRobot {
    frc::Spark spark{0};
    frc::Joystick joy{0};
    frc::DigitalOutput dio{11};
    frc::Relay relay{0};
    frc::AnalogOutput ao{1};
    frc::AnalogInput ai{0};
    frc::Encoder encoder{0,1};
    frc::Solenoid solenoid{0};
    //ctre::phoenix::motorcontrol::can::WPI_TalonSRX talon{1};
    long long unsigned start = NOW;
public:
    void RobotInit(){}

    void RobotPeriodic(){}

    void TeleopInit(){}

    void TeleopPeriodic() {
		TIME_IT();
    TIME_IT(dio.Set(true));
		TIME_IT(spark.Set(0.56));
		TIME_IT(relay.Set(frc::Relay::Value::kForward));
		TIME_IT(ao.SetVoltage(1.0));
		TIME_IT(solenoid.Set(true));
    //TIME_IT(talon.Set(0.3));

    TIME_IT(usleep(45000));
		printf("\n");
    }
};

START_ROBOT_CLASS(Robot)

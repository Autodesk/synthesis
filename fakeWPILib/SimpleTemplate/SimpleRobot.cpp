#include "WPILib.h"

class RobotDemo : public SimpleRobot 
{
private:
	RobotDrive myRobot;
	Joystick stick;

public:
	RobotDemo(void) : myRobot(1, 2), stick(1)
	{
		myRobot.SetExpiration(0.1);
	}

	void Autonomous(void) 
	{
		myRobot.SetSafetyEnabled(false);
		myRobot.Drive(-0.5, 0.0);  // drive forwards half speed
		Wait(2.0);
		myRobot.Drive(0.0, 0.0);
	}

	float a;
	void OperatorControl(void)
	{
		myRobot.SetSafetyEnabled(true);
		while (IsEnabled() && IsOperatorControl()) 
		{
			myRobot.ArcadeDrive(-stick.GetY()/2, -stick.GetX()/2);
			Wait(0.005);
		}
	}
};

#include <emulator.h>
const int TEAM_ID = 9999;
START_ROBOT_CLASS(RobotDemo);
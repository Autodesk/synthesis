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

	void OperatorControl(void)
	{
		myRobot.SetSafetyEnabled(true);
		while (IsOperatorControl()) 
		{
			myRobot.ArcadeDrive(stick);
			Wait(0.005);
		}
	}

	void Test(void) 
	{
		LiveWindow::GetInstance()->SetEnabled(true);
		while(IsTest() && !IsDisabled()) {
			Wait(1);
		}
		LiveWindow::GetInstance()->SetEnabled(false);
	}
};

#include <emulator.h>
const int TEAM_ID = 1510;
START_ROBOT_CLASS(RobotDemo);
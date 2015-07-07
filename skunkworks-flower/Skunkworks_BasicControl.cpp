#include "WPILib.h"

class RobotDemo : public SimpleRobot 
{
public:
	static const  int driveLeft = 2;
	static const int driveRight = 1;
	static const int mainRotation = 5;
	static const int collector = 3;
	static const int shooter = 4;
	static const int latchA = 1, latchB = 2;

private:
	Joystick stick;
	RobotDrive drive;
	Talon mainRot;
	AnalogChannel mainRotPot;
	Talon collect;
	DoubleSolenoid latch;
	Talon shoot;
public:
	RobotDemo(void) : stick(1), drive(driveLeft, driveRight), mainRot(mainRotation), 
		mainRotPot(1,2), collect(collector), latch(latchA, latchB), shoot(shooter)
	{
		drive.SetSafetyEnabled(false);
		mainRotPot.SetVoltageForPID(false);
	}

	~RobotDemo() {
	}

	void Autonomous(void) 
	{
		drive.SetSafetyEnabled(false);
		drive.Drive(-0.5, 0.0);  // drive forwards half speed
		Wait(2.0);
		drive.Drive(0.0, 0.0);
	}

	void OperatorControl(void)
	{
		drive.SetSafetyEnabled(true);

		while (IsOperatorControl() && IsEnabled()) 
		{
			if (stick.GetRawAxis(2)) mainRot.Set(-1);
			else if (stick.GetRawAxis(3)) mainRot.Set(1);
			else mainRot.Set(0);

			drive.ArcadeDrive(stick.GetY()/1.5, -stick.GetX()/1.5);
			collect.Set(stick.GetRawButton(1) ? -1.0 : 0);
			latch.Set(stick.GetRawButton(2) ? DoubleSolenoid::kForward : DoubleSolenoid::kReverse);
			shoot.Set(stick.GetRawButton(9) ? 1.0: (stick.GetRawButton(10) ? -1.0 : 0.0));
			SmartDashboard::PutNumber("Output: ", mainRot.Get());
			SmartDashboard::PutNumber("Angle: ", mainRotPot.GetValue());
			Wait(0.005);
		}
	}
};

#include <emulator.h>
const int TEAM_ID = 9999; 
START_ROBOT_CLASS(RobotDemo);
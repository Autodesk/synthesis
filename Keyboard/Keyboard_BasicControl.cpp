#include "WPILib.h"
#include <windows.h>
#include <iostream>

class RobotDemo : public SimpleRobot 
{
public:
	static const int driveLeft = 2;
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
		drive.Drive(-1.0, 0.0);  // drive forwards half speed
	}

	void OperatorControl(void)
	{
		drive.SetSafetyEnabled(true);

		while (IsOperatorControl() && IsEnabled()) 
		{
			// The code:
			// `0 != (GetAsyncKeyState(VK_UP) & (1 << (sizeof(SHORT)*8)))`
			// checks if the most significant bit is set, showing that the
			// key is currently down
			bool keyArrowUp 	= 0 != (GetAsyncKeyState(VK_UP) & (1 << (sizeof(SHORT)*8))),
				 keyArrowRight 	= 0 != (GetAsyncKeyState(VK_RIGHT) & (1 << (sizeof(SHORT)*8))),
				 keyArrowLeft 	= 0 != (GetAsyncKeyState(VK_LEFT) & (1 << (sizeof(SHORT)*8))),
				 keyArrowDown 	= 0 != (GetAsyncKeyState(VK_DOWN) & (1 << (sizeof(SHORT)*8))),
				 keyZ 			= 0 != (GetAsyncKeyState(0x5A) & (1 << (sizeof(SHORT)*8))),
				 keyX 			= 0 != (GetAsyncKeyState(0x58) & (1 << (sizeof(SHORT)*8))),
				 keyC 			= 0 != (GetAsyncKeyState(0x43) & (1 << (sizeof(SHORT)*8))),
				 keyV 			= 0 != (GetAsyncKeyState(0x56) & (1 << (sizeof(SHORT)*8)));
			
		/*
			std::cout << "current key states:\n"
			     << keyArrowUp << '\n'
				 << keyArrowRight << '\n'
				 << keyArrowLeft << '\n'
				 << keyArrowDown << '\n'
				 << keyZ << '\n'
				 << keyX << '\n'
				 << keyC << '\n'
				 << keyV << '\n' << endl;
		*/

			if (stick.GetRawButton(11)) mainRot.Set(-1);
			else if (stick.GetRawButton(12)) mainRot.Set(1);
			else mainRot.Set(0);

			float xSpeed, ySpeed;
			ySpeed = keyArrowLeft ? 1.0 : 0.0 + keyArrowRight ? -1.0 : 0.0;
			xSpeed = keyArrowUp ? -1.0 : 0.0 + keyArrowDown ? 1.0 : 0.0;
			
			xSpeed *= 0.4;
			ySpeed *= 0.4;

			drive.ArcadeDrive(xSpeed, ySpeed);
			collect.Set(stick.GetRawButton(1) ? -1.0 : 0);
			latch.Set(stick.GetRawButton(2) ? DoubleSolenoid::kForward : DoubleSolenoid::kReverse);
			shoot.Set(stick.GetRawButton(9) ? 1.0: (stick.GetRawButton(10) ? -1.0 : 0.0));
			SmartDashboard::PutNumber("Output: ", mainRot.Get());
			SmartDashboard::PutNumber("Angle: ", mainRotPot.GetValue());
			Wait(0.005);
		}
	}
};

void MainFunction() {

}

#include <emulator.h>
const int TEAM_ID = 9999;
START_ROBOT_CLASS_WINMAIN(RobotDemo, MainFunction)

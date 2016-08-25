#include <WPILib.h>
#include <Windows.h> // for Sleep()

class MyRobot : public SampleRobot
{
private:
	RobotDrive myRobot;
	Joystick stick;
	//CANTalon talon;
public:
	MyRobot(void) : myRobot(0, 1), stick(0) {

	}

	void OperatorControl(void) {
		while (IsOperatorControl()) {
			myRobot.ArcadeDrive(stick);
			Sleep(10);
		}
	}
};

START_ROBOT_CLASS(MyRobot);

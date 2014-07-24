#include <WPILib.h>
#include <emulator.h>


#ifndef __ROBOTDEMO
#define __ROBOTDEMO
class RobotDemo : public SimpleRobot {
private:
	RobotDrive drive;
	Joystick joy;
public:
	RobotDemo(void): drive(1,2), joy(1) {
	}
	void Autonomous(void) {
		printf("Entering autonomous!\n");
		drive.SetSafetyEnabled(false);
	}
	void OperatorControl(void) {
		printf("Entering teleop!\n");
		drive.SetSafetyEnabled(true);
		while (IsOperatorControl() && !IsDisabled()) {
			drive.ArcadeDrive(joy);
			Sleep(15);
		}
	}
};
#endif

START_ROBOT_CLASS(RobotDemo)
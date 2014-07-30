#include <WPILib.h>
#include <AnalogPotentiometer.h>
#include <emulator.h>


#ifndef __ROBOTDEMO
#define __ROBOTDEMO
class RobotDemo : public SimpleRobot {
private:
	RobotDrive drive;
	Joystick joy;
public:
	RobotDemo(void): drive(1,2), joy(1) {
		// Does it all work?
		(new Accelerometer(2))->GetAcceleration();
		(new AnalogChannel(4))->GetValue();
		(new AnalogPotentiometer(3))->Get();
		(new AnalogTrigger(6))->GetTriggerState();
		// AnalogTriggerOutput can't be created
		(new Compressor(1,1))->GetPressureSwitchValue();
		(new Counter(2))->Get();
		// Dashboard can't be created
		(new DigitalInput(3))->Get();
		(new DigitalOutput(4))->Set(1);
		// DigitalSource can't be created
		(new DoubleSolenoid(1,2))->Set(DoubleSolenoid::kForward);
		// DriverStation* can't be created
		(new Encoder(5,6))->GetRaw();
		(new GearTooth(7))->Get();
		(new Gyro(1))->GetAngle();
		(new Jaguar(5))->Set(5);
		(new Joystick(1))->GetRawButton(1);
		(new Relay(2))->Set(Relay::kForward);
		(new Servo(6))->Set(1);
		(new Solenoid(3))->Set(true);
		(new Talon(3))->Set(1);
		(new Timer())->Get();
		(new Ultrasonic(8,9))->GetRangeMM();
		(new Victor(4))->Set(2);
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
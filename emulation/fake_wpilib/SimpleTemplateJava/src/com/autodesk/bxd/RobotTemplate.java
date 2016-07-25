package com.autodesk.bxd;

import edu.wpi.first.wpilibj.Joystick;
import edu.wpi.first.wpilibj.RobotDrive;
import edu.wpi.first.wpilibj.SimpleRobot;
import edu.wpi.first.wpilibj.Timer;

public class RobotTemplate extends SimpleRobot {

	RobotDrive chassis = new RobotDrive(1, 2);
	Joystick stick = new Joystick(1);

	public RobotTemplate() {

	}

	@Override
	public void autonomous() {
		chassis.setSafetyEnabled(false);
		chassis.drive(-0.5, 0.0);
		Timer.delay(2.0);
		chassis.drive(0, 0);
	}

	@Override
	public void operatorControl() {
		chassis.setSafetyEnabled(true);
		while (isOperatorControl() && isEnabled()) {
			chassis.arcadeDrive(stick);
			Timer.delay(0.01);
		}
	}

	public static void main(String[] args) {
		EmulatorControl.start(9999, RobotTemplate.class);
	}
}
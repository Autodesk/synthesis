#!/usr/bin/env python3
#
# Copyright (c) FIRST and other WPILib contributors.
# Open Source Software; you can modify and/or share it under the terms of
# the WPILib BSD license file in the root directory of this project.
#

import wpilib
import rev

def clamp(val, min, max):
    if val < min:
        return min
    elif val > max:
        return max
    return val

class MyRobot(wpilib.TimedRobot):
    """
    This is a demo program showing the use of the DifferentialDrive class.
    Runs the motors with arcade steering.
    """

    def robotInit(self):
        """Robot initialization function"""
        # self.left_drive = rev.SparkMax(1, rev.SparkLowLevel.MotorType.kBrushless)
        # self.right_drive = rev.SparkMax(2, rev.SparkLowLevel.MotorType.kBrushless)
        self.left_drive = wpilib.Spark(1)
        self.right_drive = wpilib.Spark(2)
        self.arm_motor = rev.SparkMax(3, rev.SparkLowLevel.MotorType.kBrushless)
        self.controller = wpilib.XboxController(0)

    def teleopPeriodic(self):
        forward = -self.controller.getLeftY()
        turn = self.controller.getRightX()
        if abs(forward) < 0.2:
            forward = 0.0
        if abs(turn) < 0.2:
            turn = 0.0

        left = clamp(forward + turn, -1.0, 1.0)
        right = clamp(forward - turn, -1.0, 1.0)

        arm = -self.controller.getLeftTriggerAxis() + self.controller.getRightTriggerAxis()
        if abs(arm) < 0.2:
            arm = 0.0

        self.left_drive.set(left)
        self.right_drive.set(right)
        self.arm_motor.set(arm)
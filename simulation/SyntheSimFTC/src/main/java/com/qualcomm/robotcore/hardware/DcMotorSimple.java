package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK interface (verified byte-identical
 * across RobotCore 7.0.0 -> 11.1.0 via javap).
 */
public interface DcMotorSimple extends HardwareDevice {
    enum Direction {
        FORWARD,
        REVERSE,
    }

    void setDirection(Direction direction);

    Direction getDirection();

    void setPower(double power);

    double getPower();
}

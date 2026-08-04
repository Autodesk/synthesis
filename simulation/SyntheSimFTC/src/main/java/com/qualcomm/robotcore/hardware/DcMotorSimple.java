package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK interface.
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

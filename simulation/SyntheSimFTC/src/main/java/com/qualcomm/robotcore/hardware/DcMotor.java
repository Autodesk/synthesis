package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK interface. Minimal encoder surface only
 * (position + velocity); RunMode/ZeroPowerBehavior/PID are not modeled yet.
 */
public interface DcMotor extends DcMotorSimple {
    int getCurrentPosition();

    double getVelocity();
}

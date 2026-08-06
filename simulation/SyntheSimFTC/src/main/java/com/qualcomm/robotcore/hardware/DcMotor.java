package com.qualcomm.robotcore.hardware;

public interface DcMotor extends DcMotorSimple {
    int getCurrentPosition();

    double getVelocity();
}

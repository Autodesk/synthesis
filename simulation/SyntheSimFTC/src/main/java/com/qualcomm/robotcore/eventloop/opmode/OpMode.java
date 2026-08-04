package com.qualcomm.robotcore.eventloop.opmode;

import com.qualcomm.robotcore.hardware.Gamepad;
import com.qualcomm.robotcore.hardware.HardwareMap;
import org.firstinspires.ftc.robotcore.external.Telemetry;

/**
 * Clean-room shim of the real FTC SDK class.
 */
public abstract class OpMode {
    public HardwareMap hardwareMap;
    public Gamepad gamepad1;
    public Gamepad gamepad2;
    public Telemetry telemetry;
    public double time;

    public void resetRuntime() {
        time = 0;
    }
}

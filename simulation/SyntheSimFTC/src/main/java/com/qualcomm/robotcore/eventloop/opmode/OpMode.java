package com.qualcomm.robotcore.eventloop.opmode;

import com.qualcomm.robotcore.hardware.Gamepad;
import com.qualcomm.robotcore.hardware.HardwareMap;
import org.firstinspires.ftc.robotcore.external.Telemetry;

/**
 * Clean-room shim of the real FTC SDK class, trimmed to LinearOpMode support
 * (no init()/loop() iterative-OpMode path yet -- not needed for the teleop
 * MVP, real signatures for those left out rather than stubbed).
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

package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK class, trimmed to the fields real
 * teleop code actually reads (confirmed stable/untouched across RobotCore
 * 7.0.0 -> 11.1.0 via javap: a/b/x/y, stick axes, triggers, dpad, bumpers).
 * LED effects, edge-detection helpers (aWasPressed, etc.) and trigger
 * thresholds are newer additive SDK surface, intentionally not included yet.
 */
public class Gamepad {
    public volatile float left_stick_x;
    public volatile float left_stick_y;
    public volatile float right_stick_x;
    public volatile float right_stick_y;

    public volatile boolean dpad_up;
    public volatile boolean dpad_down;
    public volatile boolean dpad_left;
    public volatile boolean dpad_right;

    public volatile boolean a;
    public volatile boolean b;
    public volatile boolean x;
    public volatile boolean y;

    public volatile boolean start;
    public volatile boolean back;
    public volatile boolean guide;

    public volatile boolean left_bumper;
    public volatile boolean right_bumper;
    public volatile boolean left_stick_button;
    public volatile boolean right_stick_button;

    public volatile float left_trigger;
    public volatile float right_trigger;
}

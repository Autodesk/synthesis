package com.autodesk.synthesis.ftc;

import com.qualcomm.robotcore.hardware.HardwareDevice;
import com.qualcomm.robotcore.hardware.Servo;

/**
 * Minimal synthetic FTC servo backed by the PWM contract used by Fission.
 */
public class SynthesisServo implements Servo {
    private final String deviceName;
    private final FTCWsBridge bridge;

    private volatile Direction direction = Direction.FORWARD;
    private volatile double position = 0.0;

    public SynthesisServo(String deviceName, FTCWsBridge bridge) {
        this.deviceName = deviceName;
        this.bridge = bridge;
        bridge.registerServo(deviceName, this);
    }

    @Override
    public void setDirection(Direction direction) {
        this.direction = direction;
    }

    @Override
    public Direction getDirection() {
        return direction;
    }

    @Override
    public void setPosition(double position) {
        this.position = Math.max(0.0, Math.min(1.0, position));
        double effectivePosition = direction == Direction.REVERSE ? 1.0 - this.position : this.position;
        bridge.sendServoPosition(deviceName, effectivePosition);
    }

    @Override
    public double getPosition() {
        return position;
    }

    @Override
    public HardwareDevice.Manufacturer getManufacturer() {
        return HardwareDevice.Manufacturer.Synthesis;
    }

    @Override
    public String getDeviceName() {
        return "Synthesis Servo";
    }

    @Override
    public String getConnectionInfo() {
        return "Synthesis simulation: " + deviceName;
    }

    @Override
    public int getVersion() {
        return 1;
    }

    @Override
    public void resetDeviceConfigurationForOpMode() {
        direction = Direction.FORWARD;
        position = 0.0;
    }

    @Override
    public void close() {
    }
}

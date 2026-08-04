package com.autodesk.synthesis.ftc;

import com.qualcomm.robotcore.hardware.DcMotorSimple;
import com.qualcomm.robotcore.hardware.HardwareDevice;

/**
 * Backs every DcMotor/DcMotorEx/DcMotorSimple request for a given
 * hardwareMap device name.
 */
public class SynthesisDcMotor implements DcMotorSimple {
    private final String deviceName;
    private final FTCWsBridge bridge;

    private volatile Direction direction = Direction.FORWARD;
    private volatile double power = 0.0;

    public SynthesisDcMotor(String deviceName, FTCWsBridge bridge) {
        this.deviceName = deviceName;
        this.bridge = bridge;
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
    public void setPower(double power) {
        this.power = power;
        double signedPower = direction == Direction.REVERSE ? -power : power;
        bridge.sendMotorPower(deviceName, signedPower);
    }

    @Override
    public double getPower() {
        return power;
    }

    @Override
    public HardwareDevice.Manufacturer getManufacturer() {
        return HardwareDevice.Manufacturer.Synthesis;
    }

    @Override
    public String getDeviceName() {
        return "Synthesis DcMotor";
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
        power = 0.0;
    }

    @Override
    public void close() {
    }
}

package com.autodesk.synthesis.ftc;

import com.google.gson.JsonObject;
import com.qualcomm.robotcore.hardware.DcMotor;
import com.qualcomm.robotcore.hardware.HardwareDevice;

/**
 * Backs every DcMotor/DcMotorEx/DcMotorSimple request for a given
 * hardwareMap device name.
 */
public class SynthesisDcMotor implements DcMotor {
    private final String deviceName;
    private final FTCWsBridge bridge;

    private volatile Direction direction = Direction.FORWARD;
    private volatile double power = 0.0;
    private volatile int currentPosition = 0;
    private volatile double velocity = 0.0;

    public SynthesisDcMotor(String deviceName, FTCWsBridge bridge) {
        this.deviceName = deviceName;
        this.bridge = bridge;
        bridge.registerDcMotor(deviceName, this);
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
    public int getCurrentPosition() {
        return currentPosition;
    }

    @Override
    public double getVelocity() {
        return velocity;
    }

    void applyEncoderUpdate(JsonObject data) {
        if (data.has(">position")) {
            currentPosition = data.get(">position").getAsInt();
        }
        if (data.has(">velocity")) {
            velocity = data.get(">velocity").getAsDouble();
        }
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

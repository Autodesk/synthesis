package com.autodesk.synthesis.ftc;

import com.qualcomm.robotcore.hardware.DcMotorSimple;
import com.qualcomm.robotcore.hardware.HardwareDevice;

/**
 * Backs every DcMotor/DcMotorEx/DcMotorSimple request for a given
 * hardwareMap device name -- HardwareMap caches by name only, so a file
 * requesting DcMotorEx and another requesting DcMotorSimple for the same
 * config name get the same instance, matching real FTC HardwareMap
 * semantics where the underlying device is one physical motor regardless of
 * which interface level team code asks for.
 *
 * Only implements DcMotorSimple for now (setPower/setDirection) -- that's
 * the full surface ExampleDozerArcadeDrive touches. Widen to DcMotorEx (encoder,
 * velocity, PIDF) is a separate, larger unit of work, not bundled in here.
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

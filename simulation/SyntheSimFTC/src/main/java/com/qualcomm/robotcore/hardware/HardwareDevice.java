package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK interface (verified byte-identical
 * across RobotCore 7.0.0 -> 11.1.0 via javap). Matches the real package and
 * method signatures so unmodified team OpMode code compiles against it.
 */
public interface HardwareDevice {
    enum Manufacturer {
        Unknown,
        Synthesis,
    }

    Manufacturer getManufacturer();

    String getDeviceName();

    String getConnectionInfo();

    int getVersion();

    void resetDeviceConfigurationForOpMode();

    void close();
}

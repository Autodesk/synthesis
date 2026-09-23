package com.qualcomm.robotcore.hardware;

/**
 * Clean-room shim of the real FTC SDK interface.
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

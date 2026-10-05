package com.qualcomm.robotcore.hardware;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Clean-room shim of the real FTC SDK class. The real HardwareMap is populated
 * ahead of time from an on-device robot config XML that never exists in a
 * team's source tree (it lives only on the Control Hub). We have no such
 * config to read, so devices are created lazily on first {@link #get} via an
 * injected {@link DeviceFactory}, then cached by name so a later {@code get()}
 * with a narrower or wider requested type (e.g. DcMotor vs DcMotorEx vs
 * DcMotorSimple) returns the same backing instance.
 */
public class HardwareMap {
    @FunctionalInterface
    public interface DeviceFactory {
        HardwareDevice create(Class<?> classOrInterface, String deviceName);
    }

    private final Map<String, HardwareDevice> devices = new ConcurrentHashMap<>();
    private final DeviceFactory deviceFactory;

    public HardwareMap(DeviceFactory deviceFactory) {
        this.deviceFactory = deviceFactory;
    }

    public <T> T get(Class<? extends T> classOrInterface, String deviceName) {
        HardwareDevice device = devices.computeIfAbsent(deviceName, name -> deviceFactory.create(classOrInterface, name));

        if (device == null) {
            throw new IllegalArgumentException(
                    "Unable to find a hardware device with name \"" + deviceName + "\" of type " + classOrInterface.getSimpleName());
        }
        if (!classOrInterface.isInstance(device)) {
            throw new IllegalArgumentException("Hardware device \"" + deviceName + "\" of type " + device.getClass().getSimpleName()
                    + " is not compatible with requested type " + classOrInterface.getSimpleName());
        }

        return classOrInterface.cast(device);
    }
}

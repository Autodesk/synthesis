package com.autodesk.synthesis;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimDouble;

/**
 * Accel class for easy implementation of documentation-compliant simulation data.
 * 
 * See https://github.com/wpilibsuite/allwpilib/blob/6478ba6e3fa317ee041b8a41e562d925602b6ea4/simulation/halsim_ws_core/doc/hardware_ws_api.md
 * for documentation on the WebSocket API Specification.
 */
public class Accel {

    private SimDevice m_device;

    private SimDouble m_range;
    private SimBoolean m_connected;
    private SimDouble m_X;
    private SimDouble m_Y;
    private SimDouble m_Z;
    private SimDouble m_velX;
    private SimDouble m_velY;
    private SimDouble m_velZ;

    /**
     * Creates an Accel sim device in accordance with the WebSocket API Specification.
     * 
     * @param name Name of the Accel. This is generally the class name of the originating accelerometer (i.e. "ADXL362").
     * @param deviceId ID of the Accel.
     */
    public Accel(String name, int deviceId) {
        m_device = SimDevice.create("Accel:" + name, deviceId);

        m_range = m_device.createDouble("range", Direction.kOutput, 0.0);
        m_connected = m_device.createBoolean("connected", Direction.kOutput, true);
        m_X = m_device.createDouble("x", Direction.kInput, 0.0);
        m_Y = m_device.createDouble("y", Direction.kInput, 0.0);
        m_Z = m_device.createDouble("z", Direction.kInput, 0.0);
        m_velX = m_device.createDouble("vx", Direction.kInput, 0.0);
        m_velY = m_device.createDouble("vy", Direction.kInput, 0.0);
        m_velZ = m_device.createDouble("vz", Direction.kInput, 0.0);
    }

    /**
     * Set the range of the accelerometer.
     *
     * @param range Range of the accelerometer
     */
    public void setRange(double range) {
        m_range.set(range);
    }

    /**
     * Set whether the accelerometer is connected.
     *
     * @param connected Whether the accelerometer is connected
     */
    public void setConnected(boolean connected) {
        m_connected.set(connected);
    }

    /**
     * Get the X of the accelerometer.
     *
     * @return X
     */
    public double getX() {
        return m_X.get();
    }

    /**
     * Get the Y of the accelerometer.
     *
     * @return Y
     */
    public double getY() {
        return m_Y.get();
    }

    /**
     * Get the Z of the accelerometer.
     *
     * @return Z
     */
    public double getZ() {
        return m_Z.get();
    }

    /**
     * Get the X velocity of the accelerometer.
     *
     * @return X
     */
    public double getVelX() {
        return m_velX.get();
    }

    /**
     * Get the Y velocity of the accelerometer.
     *
     * @return Y
     */
    public double getVelY() {
        return m_velY.get();
    }

    /**
     * Get the Z velocity of the accelerometer.
     *
     * @return Z
     */
    public double getVelZ() {
        return m_velZ.get();
    }
}

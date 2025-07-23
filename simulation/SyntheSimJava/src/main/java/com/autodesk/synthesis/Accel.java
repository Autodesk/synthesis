package com.autodesk.synthesis;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimDouble;

/**
 * Accelerometer class for easy implementation of documentation-compliant simulation data.
 * 
 * See https://github.com/wpilibsuite/allwpilib/blob/6478ba6e3fa317ee041b8a41e562d925602b6ea4/simulation/halsim_ws_core/doc/hardware_ws_api.md
 * for documentation on the WebSocket API Specification.
 */
public class Accel {
    private SimDevice m_device;
    private SimDouble m_range;
    private SimBoolean m_connected;
    private SimDouble m_x, m_y, m_z;

    /**
     * Creates a CANMotor sim device in accordance with the WebSocket API Specification.
     * 
     * @param name Name of the Accel. This is generally the class name of the originating gyro (i.e. "ADXRS450").
     * @param deviceId ID of the Gyro.
     */
    public Accel(String name, int deviceId) {
        m_device = SimDevice.create("Accel:" + name, deviceId);

        m_range = m_device.createDouble("range", Direction.kOutput, 0.0);
        m_connected = m_device.createBoolean("connected", Direction.kOutput, false);
        m_x = m_device.createDouble("x", Direction.kInput, 0);
        m_y = m_device.createDouble("y", Direction.kInput, 0);
        m_z = m_device.createDouble("z", Direction.kInput, 0);
    }

    /**
     * Set the range of the accel.
     *
     * @param range Range of the accel
     */
    public void setRange(double range) {
        if (Double.isNaN(range) || Double.isInfinite(range)) {
            range = 0.0;
        }

        m_range.set(range);
    }

    public void setConnected(boolean connected) {
        m_connected.set(connected);
    }

    /**
     * Get the x position of the accel.
     *
     * @return x
     */
    public double getX() {
        return m_x.get();
    }

    /**
     * Get the y position of the accel.
     *
     * @return y
     */
    public double getY() {
        return m_y.get();
    }

    /**
     * Get the z position of the accel.
     *
     * @return z
     */
    public double getZ() {
        return m_z.get();
    }

}

package com.autodesk.synthesis;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDevice.Direction;
import edu.wpi.first.hal.SimDouble;

/**
 * Gyro class for easy implementation of documentation-compliant simulation data.
 * 
 * See https://github.com/wpilibsuite/allwpilib/blob/6478ba6e3fa317ee041b8a41e562d925602b6ea4/simulation/halsim_ws_core/doc/hardware_ws_api.md
 * for documentation on the WebSocket API Specification.
 */
public class Gyro {

    private SimDevice m_device;

    private SimDouble m_range;
    private SimBoolean m_connected;
    private SimDouble m_angleX;
    private SimDouble m_angleY;
    private SimDouble m_angleZ;
    private SimDouble m_rateX;
    private SimDouble m_rateY;
    private SimDouble m_rateZ;

    /**
     * Creates a CANMotor sim device in accordance with the WebSocket API Specification.
     * 
     * @param name Name of the Gyro. This is generally the class name of the originating gyro (i.e. "ADXRS450").
     * @param deviceId ID of the Gyro.
     */
    public Gyro(String name, int deviceId) {
        m_device = SimDevice.create("Gyro:" + name, deviceId);

        m_range = m_device.createDouble("range", Direction.kOutput, 0.0);
        m_connected = m_device.createBoolean("connected", Direction.kOutput, false);
        m_angleX = m_device.createDouble("angle_x", Direction.kInput, 0.0);
        m_angleY = m_device.createDouble("angle_y", Direction.kInput, 0.0);
        m_angleZ = m_device.createDouble("angle_z", Direction.kInput, 0.0);
        m_rateX = m_device.createDouble("rate_x", Direction.kInput, 0.0);
        m_rateY = m_device.createDouble("rate_y", Direction.kInput, 0.0);
        m_rateZ = m_device.createDouble("rate_z", Direction.kInput, 0.0);
    }

    /**
     * Set the range of the gyro.
     *
     * @param range Range of the gyro
     */
    public void setRange(double range) {
        if (Double.isNaN(range) || Double.isInfinite(range)) {
            range = 0.0;
        }

        m_range.set(range);
    }

    /**
     * Set whether the gyro is connected.
     *
     * @param connected Whether the gyro is connected
     */
    public void setConnected(boolean connected) {
        m_connected.set(connected);
    }

    /**
     * Get the angleX of the gyro.
     *
     * @return angleX
     */
    public double getAngleX() {
        return m_angleX.get();
    }

    /**
     * Get the angleY of the gyro.
     *
     * @return angleY
     */
    public double getAngleY() {
        return m_angleY.get();
    }

    /**
     * Get the angleZ of the gyro.
     *
     * @return angleZ
     */
    public double getAngleZ() {
        return m_angleZ.get();
    }

    /**
     * Get the rateX of the gyro.
     *
     * @return rateX
     */
    public double getRateX() {
        return m_rateX.get();
    }

    /**
     * Get the rateY of the gyro.
     *
     * @return rateY
     */
    public double getRateY() {
        return m_rateY.get();
    }

    /**
     * Get the rateZ of the gyro.
     *
     * @return rateZ
     */
    public double getRateZ() {
        return m_rateZ.get();
    }
}

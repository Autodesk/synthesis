package com.autodesk.synthesis.wpilibj;

import com.autodesk.synthesis.Accel;

import edu.wpi.first.wpilibj.SPI;

/**
 * ADXL362 wrapper for simulation support.
 * Extends the original WPILib ADXL362 accelerometer class to provide
 * simulated accelerometer data during simulation.
 */
public class ADXL362 extends edu.wpi.first.wpilibj.ADXL362 {
    private Accel m_Accel;

    /**
     * Constructor for ADXL362 accelerometer.
     * 
     * @param port SPI port the accelerometer is connected to
     * @param range The range of the accelerometer
     */
    public ADXL362(SPI.Port port, Range range) {
        super(port, range);
        init("SPI", port.value, range);
    }

    /**
     * Constructor with default range.
     * 
     * @param port SPI port the accelerometer is connected to
     */
    public ADXL362(SPI.Port port) {
        this(port, Range.k2G);
    }

    private void init(String commType, int port, Range range) {
        this.m_Accel = new Accel("Accel: " + commType, port);
        this.m_Accel.setConnected(true);

        double rangeValue;
        switch (range) {
            case k2G:
                rangeValue = 2.0;
            case k4G:
                rangeValue = 4.0;
            case k8G:
                rangeValue = 8.0;
        }

        this.m_Accel.setRange(rangeValue);
    }

    /**
     * Get the acceleration in the X-axis.
     * 
     * @return X-axis acceleration in g-forces
     */
    @Override
    public double getX() {
        return m_Accel.getX();
    }

    /**
     * Get the acceleration in the Y-axis.
     * 
     * @return Y-axis acceleration in g-forces
     */
    @Override
    public double getY() {
        return m_Accel.getY();
    }

    /**
     * Get the acceleration in the Z-axis.
     * 
     * @return Z-axis acceleration in g-forces
     */
    @Override
    public double getZ() {
        return m_Accel.getZ();
    }
}

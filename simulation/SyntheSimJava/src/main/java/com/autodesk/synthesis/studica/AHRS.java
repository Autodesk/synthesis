package com.autodesk.synthesis.studica;

import com.autodesk.synthesis.Gyro;

/**
 * NavX AHRS wrapper to add proper WPILib HALSim support.
 */
public class AHRS extends com.studica.frc.AHRS {
    private Gyro m_gyro;

    public AHRS() {
        this(NavXComType.kMXP_SPI);
    }

    public AHRS(NavXComType comType) {
        super(comType);
        this.m_gyro = new Gyro("SYN AHRS", comType.ordinal());
    }

    @Override
    public double getAngle() {
        return m_gyro.getAngleZ();
    }

    @Override
    public float getYaw() {
        return (float) m_gyro.getAngleZ();
    }

    @Override
    public float getPitch() {
        return (float) m_gyro.getAngleX();
    }

    @Override
    public float getRoll() {
        return (float) m_gyro.getAngleY();
    }

    @Override
    public double getRate() {
        return m_gyro.getRateZ();
    }

    public double getAngleX() {
        return m_gyro.getAngleX();
    }

    public double getAngleY() {
        return m_gyro.getAngleY();
    }

    public double getAngleZ() {
        return m_gyro.getAngleZ();
    }

    public double getRateX() {
        return m_gyro.getRateX();
    }

    public double getRateY() {
        return m_gyro.getRateY();
    }

    public double getRateZ() {
        return m_gyro.getRateZ();
    }
}

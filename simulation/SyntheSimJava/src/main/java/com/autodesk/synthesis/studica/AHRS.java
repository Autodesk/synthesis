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
        return (float) (-m_gyro.getAngleZ() % 360 - 180);
    }

    @Override
    public float getPitch() {
        return (float) (m_gyro.getAngleX() % 360 - 180);
    }

    @Override
    public float getRoll() {
        return (float) (m_gyro.getAngleY() % 360 - 180);
    }

    @Override
    public double getRate() {
        return m_gyro.getRateZ();
    }
}

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
}

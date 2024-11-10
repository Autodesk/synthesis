package com.autodesk.synthesis.kauailabs;

import com.autodesk.synthesis.Gyro;

import edu.wpi.first.wpilibj.SPI;
import edu.wpi.first.wpilibj.I2C;
import edu.wpi.first.wpilibj.SerialPort;

/**
 * Outline for a NavX AHRS class.
 * TODO
 */
public class AHRS extends com.kauailabs.navx.frc.AHRS {
    private Gyro m_gyro;

    public AHRS() {
        this(SPI.Port.kMXP);
    }
    
    public AHRS(I2C.Port port) {
        super(port);
        init("I2C", port.value);
    }

    public AHRS(SPI.Port port) {
        super(port);
        init("SPI", port.value);
    }

    public AHRS(SerialPort.Port port) {
        super(port);
        init("SERIAL", port.value);
    }

    private void init(String commType, int port) {
        this.m_gyro = new Gyro("SYN AHRS " + commType, port);
    }
}

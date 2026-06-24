package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;

import com.revrobotics.AbsoluteEncoder;

/**
 * SparkAbsoluteEncoder wrapper to add proper WPILib HALSim support.
 * Position and velocity are sourced from the Synthesis sim encoder.
 * All other reads delegate to the real encoder.
 */
public class SparkAbsoluteEncoder implements AbsoluteEncoder {
    private CANEncoder m_simEncoder;
    private SparkMax m_sparkMax;

    public SparkAbsoluteEncoder(SparkMax sparkMax) {
        this.m_simEncoder = sparkMax.m_encoder;
        this.m_sparkMax = sparkMax;
    }

    @Override
    public double getPosition() {
        double invertFactor = m_sparkMax.m_absEncoderInverted ? -1.0 : 1.0;
        return m_simEncoder.getPosition() * m_sparkMax.m_absEncoderPositionFactor * invertFactor;
    }

    @Override
    public double getVelocity() {
        double invertFactor = m_sparkMax.m_absEncoderInverted ? -1.0 : 1.0;
        return m_simEncoder.getVelocity() * m_sparkMax.m_absEncoderVelocityFactor * invertFactor;
    }
}

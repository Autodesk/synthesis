package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;
import com.revrobotics.REVLibError;

/**
 * RelativeEncoder wrapper to add proper WPILib HALSim support.
 * Position and velocity are sourced from the Synthesis sim encoder.
 * Conversion factors and inversion are managed internally.
 */
public class RelativeEncoder implements com.revrobotics.RelativeEncoder {

    private SparkMax m_sparkMax;
    private CANEncoder m_encoder;
    private double m_zero = 0.0;

    public RelativeEncoder(SparkMax sparkMax, CANEncoder encoder) {
        m_sparkMax = sparkMax;
        m_encoder = encoder;
    }

    @Override
    public double getPosition() {
        double invertFactor = m_sparkMax.m_encoderInverted ? -1.0 : 1.0;
        return m_encoder.getPosition() * m_sparkMax.m_encoderPositionFactor * invertFactor - m_zero;
    }

    @Override
    public double getVelocity() {
        double invertFactor = m_sparkMax.m_encoderInverted ? -1.0 : 1.0;
        return m_encoder.getVelocity() * m_sparkMax.m_encoderVelocityFactor * invertFactor;
    }

    @Override
    public REVLibError setPosition(double position) {
        double invertFactor = m_sparkMax.m_encoderInverted ? -1.0 : 1.0;
        m_zero = m_encoder.getPosition() * m_sparkMax.m_encoderPositionFactor * invertFactor - position;
        return REVLibError.kOk;
    }
}

package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;
import com.revrobotics.REVLibError;

/**
 * RelativeEncoder wrapper to add proper WPILib HALSim support.
 * Position and velocity are sourced from the Synthesis sim encoder.
 * Conversion factors and inversion are managed internally.
 */
public class RelativeEncoder implements com.revrobotics.RelativeEncoder {

    private CANEncoder m_encoder;
    private double m_zero = 0.0;
    private double m_positionConversionFactor = 1.0;
    private double m_velocityConversionFactor = 1.0;
    private double m_invertedFactor = 1.0;

    public RelativeEncoder(com.revrobotics.RelativeEncoder original, CANEncoder encoder) {
        m_encoder = encoder;
    }

    @Override
    public double getPosition() {
        return m_encoder.getPosition() * m_positionConversionFactor * m_invertedFactor - m_zero;
    }

    @Override
    public double getVelocity() {
        return m_encoder.getVelocity() * m_velocityConversionFactor * m_invertedFactor;
    }

    @Override
    public REVLibError setPosition(double position) {
        m_zero = m_encoder.getPosition() * m_positionConversionFactor * m_invertedFactor - position;
        return REVLibError.kOk;
    }

    public void setPositionConversionFactor(double factor) {
        m_positionConversionFactor = factor;
    }

    public void setVelocityConversionFactor(double factor) {
        m_velocityConversionFactor = factor;
    }

    public double getPositionConversionFactor() {
        return m_positionConversionFactor;
    }

    public double getVelocityConversionFactor() {
        return m_velocityConversionFactor;
    }

    public void setInverted(boolean inverted) {
        m_invertedFactor = inverted ? -1.0 : 1.0;
    }

    public boolean getInverted() {
        return m_invertedFactor < 0.0;
    }
}

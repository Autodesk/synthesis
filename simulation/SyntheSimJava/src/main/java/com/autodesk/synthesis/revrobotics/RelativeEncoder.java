package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;
import com.revrobotics.REVLibError;

public class RelativeEncoder implements com.revrobotics.RelativeEncoder {

    private com.revrobotics.RelativeEncoder m_original;
    private CANEncoder m_encoder;
    private double m_zero = 0.0;
    private double m_positionConversionFactor = 1.0;
    private double m_velocityConversionFactor = 1.0;
    private double m_invertedFactor = 1.0;

    public RelativeEncoder(com.revrobotics.RelativeEncoder original, CANEncoder encoder) {
        m_original = original;
        m_encoder = encoder;

        m_positionConversionFactor = m_original.getPositionConversionFactor();
        m_velocityConversionFactor = m_original.getVelocityConversionFactor();
        m_invertedFactor = m_original.getInverted() ? -1.0 : 1.0;
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

    @Override
    public REVLibError setPositionConversionFactor(double factor) {
        m_positionConversionFactor = factor;
        return REVLibError.kOk;
    }

    @Override
    public REVLibError setVelocityConversionFactor(double factor) {
        m_velocityConversionFactor = factor;
        return REVLibError.kOk;
    }

    @Override
    public double getPositionConversionFactor() {
        return m_positionConversionFactor;
    }

    @Override
    public double getVelocityConversionFactor() {
        return m_velocityConversionFactor;
    }

    @Override
    public REVLibError setAverageDepth(int depth) {
        return m_original.setAverageDepth(depth);
    }

    @Override
    public int getAverageDepth() {
        return m_original.getAverageDepth();
    }

    @Override
    public REVLibError setMeasurementPeriod(int period_ms) {
        return m_original.setMeasurementPeriod(period_ms);
    }

    @Override
    public int getMeasurementPeriod() {
        return m_original.getMeasurementPeriod();
    }

    @Override
    public int getCountsPerRevolution() {
        return 1;
    }

    @Override
    public REVLibError setInverted(boolean inverted) {
        m_invertedFactor = inverted ? -1.0 : 1.0;
        return REVLibError.kOk;
    }

    @Override
    public boolean getInverted() {
        return m_invertedFactor < 0.0;
    }
    
}

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
    private com.revrobotics.spark.SparkAbsoluteEncoder m_realEncoder;

    public SparkAbsoluteEncoder(com.revrobotics.spark.SparkAbsoluteEncoder realEncoder, CANEncoder simEncoder) {
        this.m_realEncoder = realEncoder;
        this.m_simEncoder = simEncoder;
    }

    @Override
    public double getPosition() {
        return this.m_simEncoder.getPosition();
    }

    @Override
    public double getVelocity() {
        return this.m_simEncoder.getVelocity();
    }
}

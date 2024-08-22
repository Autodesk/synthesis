package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;

import com.revrobotics.AbsoluteEncoder;
import com.revrobotics.REVLibError;

/**
 * SparkAbsoluteEncoder wrapper to add proper WPILib HALSim support.
 */
public class SparkAbsoluteEncoder implements AbsoluteEncoder {
    private CANEncoder simEncoder;
    private com.revrobotics.SparkAbsoluteEncoder realEncoder;

    /*
     * A SparkAbsoluteEncoder class that returns the motors position and velocity from the simulated motor in fission, rather than the actual motor.
     * All other parameters are returned from the real motor, which likely won't exist, not sure what it does then but we'll just call it UB.
     */
    public SparkAbsoluteEncoder(com.revrobotics.SparkAbsoluteEncoder realEncoder, CANEncoder simEncoder) {
        this.realEncoder = realEncoder;
        this.simEncoder = simEncoder;
    }

    /**
     * Gets the average sampling depth for the real encoder
     *
     * @return The average sampling depth
     */
    public int getAverageDepth() {
        return this.realEncoder.getAverageDepth();
    }

    /**
     * Gets the phase of the real encoder 
     *
     * @return The phase of the real encoder
     */
    public boolean getInverted() {
        return this.realEncoder.getInverted();
    }

    /**
     * Gets the position of the simulated motor.
     * This returns the native units of 'rotations' by default, and can be changed by a scale factor using setPositionConversionFactor().
     *
     * @return Number of rotations of the motor
     */
    public double getPosition() {
        return this.simEncoder.getPosition();
    }

    /**
     * Sets the conversion factor for position of the real encoder. Multiplying by the native output units to give you position
     *
     * @return The conversion factor used by the encoder for position
     */
    public double getPositionConversionFactor() {
        return this.realEncoder.getPositionConversionFactor();
    }
    
    
    /**
     * Gets the velocity of the simulated motor. This returns the native units of 'rotations per second' by default, and can be changed by a scale factor using setVelocityConversionFactor().
     *
     * @return Number of rotations per second of the motor
     */
    public double getVelocity() {
        return this.simEncoder.getVelocity() * this.realEncoder.getVelocityConversionFactor();
    }


    /**
     * Gets the conversion factor for velocity of the real encoder.
     *
     * @return The conversion factor used by the encoder for position
     */
    public double getVelocityConversionFactor() {
        return this.realEncoder.getVelocityConversionFactor();
    }

    /**
     * Gets the zero offset in revolutions for the real encoder (the position that is reported as zero).
     *
     * @return The zero offset
     */
    public double getZeroOffset() {
        return this.realEncoder.getZeroOffset();
    }
    
    /**
     * Sets the average sampling depth for the real encoder.
     *
     * @param depth The average sampling depth
     *
     * @return A library error indicating failure or success
     */
    public REVLibError setAverageDepth(int depth) {
        return this.realEncoder.setAverageDepth(depth);
    }

    /**
     * Sets the phase of the real encoder 
     *
     * @param inverted Whether the real motor should be inverted
     *
     * @return A library error indicating failure or success
     */
    public REVLibError setInverted(boolean inverted) {
        return this.realEncoder.setInverted(inverted);
    }

    /**
     * Sets the conversion factor for position of the real encoder.
     *
     * @param factor The new position conversion factor
     *
     * @return A library error indicating failure or success
     */
    public REVLibError setPositionConversionFactor(double factor) {
        return this.realEncoder.setPositionConversionFactor(factor);
    }

    /**
     * Sets the conversion factor for velocity of the real encoder.
     *
     * @param factor The new velocity conversion factor
     *
     * @return A library error indicating failure or success
     */
    public REVLibError setVelocityConversionFactor(double factor) {
        return this.realEncoder.setVelocityConversionFactor(factor);
    }

    /**
     * Sets the zero offset of the real encoder (the position that is reported as zero).
     *
     * @param offset The new zero offset
     *
     * @return A library error indicating failure or success
     */
    public REVLibError setZeroOffset(double offset) {
        return this.realEncoder.setZeroOffset(offset);
    }
}

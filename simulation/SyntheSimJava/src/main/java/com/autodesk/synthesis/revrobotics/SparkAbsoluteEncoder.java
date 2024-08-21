package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;

import com.revrobotics.AbsoluteEncoder;
import com.revrobotics.REVLibError;

public class SparkAbsoluteEncoder implements AbsoluteEncoder {
    private CANEncoder simEncoder;
    private com.revrobotics.SparkAbsoluteEncoder realEncoder;

    /*
     * A SparkAbsoluteEncoder class that returns the motors position and velocity from the simulated motor in fission, rather than the actual motor
     * All other parameters are returned from the real motor, which likely won't exist, not sure what it does then but we'll just call it UB
     */
    public SparkAbsoluteEncoder(com.revrobotics.SparkAbsoluteEncoder realEncoder, CANEncoder simEncoder) {
        this.realEncoder = realEncoder;
        this.simEncoder = simEncoder;
    }

    /*
     * Get the average sampling depth for an absolute encoder
     * @return The average sampling depth
     */
    public int getAverageDepth() {
        return this.realEncoder.getAverageDepth();
    }

    /*
     * Get the phase of the AbsoluteEncoder
     * Returns: The phase of the encoder
    */
    public boolean getInverted() {
        return this.realEncoder.getInverted();
    }

    /*
     * Get the position of the motor. This returns the native units of 'rotations' by default, and can be changed by a scale factor using setPositionConversionFactor().
     * Returns: Number of rotations of the motor
    */
    public double getPosition() {
        return this.simEncoder.getPosition() * this.realEncoder.getPositionConversionFactor();
    }

    /*
     * Set the conversion factor for position of the encoder. Multiplied by the native output units to give you position
     * Returns: The conversion factor for position
    */
    public double getPositionConversionFactor() {
        return this.getPositionConversionFactor();
    }
    
    
    /*
     * Get the velocity of the motor. This returns the native units of 'rotations per second' by default, and can be changed by a scale factor using setVelocityConversionFactor().
     * Returns: Number of rotations per second of the motor
    */
    public double getVelocity() {
        return this.simEncoder.getVelocity() * this.realEncoder.getVelocityConversionFactor();
    }


    /*
     * Get the conversion factor for velocity of the encoder.
    */
    public double getVelocityConversionFactor() {
        return this.realEncoder.getVelocityConversionFactor();
    }

    /*
     * Gets the zero offset for an absolute encoder (the position that is reported as zero).
    */
    public double getZeroOffset() {
        return this.realEncoder.getZeroOffset();
    }
    
    /*
     * Set the average sampling depth for an absolute encoder.
    */
    public REVLibError setAverageDepth(int depth) {
        return this.realEncoder.setAverageDepth(depth);
    }

    /*
     * Set the phase of the AbsoluteEncoder so that it is set to be in phase with the motor itself
    */
    public REVLibError setInverted(boolean inverted) {
        return this.realEncoder.setInverted(inverted);
    }

    /*
    * Set the conversion factor for position of the encoder.
    */
    public REVLibError setPositionConversionFactor(double factor) {
        return this.realEncoder.setPositionConversionFactor(factor);
    }

    /*
     * Set the conversion factor for velocity of the encoder.
    */
    public REVLibError setVelocityConversionFactor(double factor) {
        return this.realEncoder.setVelocityConversionFactor(factor);
    }

    /*
     * Sets the zero offset of an absolute encoder (the position that is reported as zero).
    */
    public REVLibError setZeroOffset(double factor) {
        return this.realEncoder.setZeroOffset(factor);
    }
}

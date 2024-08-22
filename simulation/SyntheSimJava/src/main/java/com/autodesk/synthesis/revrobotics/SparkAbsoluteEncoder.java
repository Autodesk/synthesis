package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;

import com.revrobotics.AbsoluteEncoder;
import com.revrobotics.REVLibError;

public class SparkAbsoluteEncoder implements AbsoluteEncoder {
    private CANEncoder simEncoder;
    private com.revrobotics.SparkAbsoluteEncoder realEncoder;

    // We're prettys sure that it's impossible to make a child class of this parent class with a constructor, because the parent's constructor is private
    // Reflection didn't work since a child constructor __needs__ a super() call at the top of the body
    // Passing in the constuctor wouldn't work either, since it's just a function pointer and the child constructor would have no idea that it points to its super
    public SparkAbsoluteEncoder(com.revrobotics.SparkAbsoluteEncoder realEncoder, CANEncoder simEncoder) {
        this.realEncoder = realEncoder;
        this.simEncoder = simEncoder;
    }

    public int getAverageDepth() {
        return this.realEncoder.getAverageDepth();
    }

    public boolean getInverted() {
        return this.realEncoder.getInverted();
    }

    public double getPosition() {
        return this.simEncoder.getPosition() * this.realEncoder.getPositionConversionFactor();
    }

    // TODO: Remove conversion factors on the fission end
    public double getPositionConversionFactor() {
        return this.getPositionConversionFactor();
    }

    public double getVelocity() {
        return this.simEncoder.getVelocity() * this.realEncoder.getVelocityConversionFactor();
    }

    public double getVelocityConversionFactor() {
        return this.realEncoder.getVelocityConversionFactor();
    }

    public double getZeroOffset() {
        return this.realEncoder.getZeroOffset();
    }

    public REVLibError setAverageDepth(int depth) {
        return this.realEncoder.setAverageDepth(depth);
    }

    public REVLibError setInverted(boolean inverted) {
        return this.realEncoder.setInverted(inverted);
    }

    public REVLibError setPositionConversionFactor(double factor) {
        return this.realEncoder.setPositionConversionFactor(factor);
    }

    public REVLibError setVelocityConversionFactor(double factor) {
        return this.realEncoder.setVelocityConversionFactor(factor);
    }

    public REVLibError setZeroOffset(double factor) {
        return this.realEncoder.setZeroOffset(factor);
    }
}

package com.autodesk.synthesis.revrobotics;
import com.autodesk.synthesis.CANEncoder;
import com.revrobotics.CANSparkBase;

public class SparkAbsoluteEncoder extends com.revrobotics.SparkAbsoluteEncoder {
    private CANEncoder encoder;

    public SparkAbsoluteEncoder(CANEncoder encoder, CANSparkBase base, com.revrobotics.SparkAbsoluteEncoder.Type type) {
        this.encoder = encoder;
    }

    @Override
    public double getPosition() {
        return encoder.getPosition();
    }

    @Override
    public double getVelocity() {
        return encoder.getVelocity();
    }
}

package com.autodesk.synthesis.revrobotics;
import java.lang.reflect.Constructor;

import com.autodesk.synthesis.CANEncoder;
import com.revrobotics.CANSparkBase;

public class SparkAbsoluteEncoder extends com.revrobotics.SparkAbsoluteEncoder{
    private CANEncoder encoder;

    public SparkAbsoluteEncoder(CANEncoder encoder, com.revrobotics.SparkAbsoluteEncoder.Type type) throws Exception {
        try {
            Constructor<com.revrobotics.SparkAbsoluteEncoder> constructor = com.revrobotics.SparkAbsoluteEncoder.class.getConstructor(com.revrobotics.CANSparkBase.class, com.revrobotics.SparkAbsoluteEncoder.Type.class);
        } catch (NoSuchMethodException | SecurityException e) {
            e.printStackTrace();
        }

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

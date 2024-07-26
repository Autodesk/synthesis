package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;
import com.revrobotics.REVLibError;

/**
 * CANSparkMax wrapper to add proper WPILib HALSim support.
 */
public class CANSparkMax extends com.revrobotics.CANSparkMax {

    private CANMotor m_motor;
    private CANEncoder m_encoder;

    /**
     * Creates a new CANSparkMax, wrapped with simulation support.
     * 
     * @param deviceId  CAN Device ID.
     * @param motorType Motortype. For Simulation purposes, this is discarded at the
     *                  moment.
     */
    public CANSparkMax(int deviceId, MotorType motorType) {
        super(deviceId, motorType);

        m_motor = new CANMotor("SYN CANSparkMax", deviceId, 0.0, false, 0.3);
        m_encoder = new CANEncoder("SYN CANSparkMax/Encoder", deviceId);
    }

    @Override
    public void set(double percent) {
        super.set(percent);
        m_motor.setPercentOutput(percent);
    }

    public void test() {
        System.out.println("test");
    }

    public void setNeutralDeadband(double n) {
        m_motor.setNeutralDeadband(n);
    }

    @Override
    public REVLibError setIdleMode(com.revrobotics.CANSparkBase.IdleMode mode) {
        if (mode != null) {
            m_motor.setBrakeMode(mode.equals(com.revrobotics.CANSparkBase.IdleMode.kBrake));
        }

        return super.setIdleMode(mode);
    }
}

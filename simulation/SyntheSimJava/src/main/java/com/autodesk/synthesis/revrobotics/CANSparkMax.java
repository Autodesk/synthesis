package com.autodesk.synthesis.revrobotics;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;
// import com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder;
import com.revrobotics.CANSparkBase;
import com.revrobotics.REVLibError;

/**
 * CANSparkMax wrapper to add proper WPILib HALSim support.
 */
public class CANSparkMax extends com.revrobotics.CANSparkMax {

    private CANMotor m_motor;
    public CANEncoder m_encoder;

    /**
     * Creates a new CANSparkMax, wrapped with simulation support.
     * 
     * @param deviceId  CAN Device ID.
     * @param motorType Motortype. For Simulation purposes, this is discarded at the
     *                  moment.
     */
    public CANSparkMax(int deviceId, MotorType motorType) {
        super(deviceId, motorType);

        this.m_motor = new CANMotor("SYN CANSparkMax", deviceId, 0.0, false, 0.3);
        this.m_encoder = new CANEncoder("SYN CANSparkMax", deviceId);
    }

    @Override
    public void set(double percent) {
        super.set(percent);
        this.m_motor.setPercentOutput(percent);
    }

    public void setNeutralDeadband(double n) {
        this.m_motor.setNeutralDeadband(n);
    }

    @Override
    public REVLibError setIdleMode(com.revrobotics.CANSparkBase.IdleMode mode) {
        if (mode != null) {
            this.m_motor.setBrakeMode(mode.equals(com.revrobotics.CANSparkBase.IdleMode.kBrake));
        }

        return super.setIdleMode(mode);
    }

    // @Override
    // public SparkAbsoluteEncoder getAbsoluteEncoder() throws Exception{
    //     try {
    //         return new SparkAbsoluteEncoder(this.m_encoder, com.revrobotics.SparkAbsoluteEncoder.Type.kDutyCycle);
    //     } catch (Exception e) {
    //         // TODO Auto-generated catch block
    //         e.printStackTrace();
    //     }
    // }

    // @Override
    // public SparkAbsoluteEncoder getAbsoluteEncoder(com.revrobotics.SparkAbsoluteEncoder.Type type) throws Exception {
    //     try {
    //         return new SparkAbsoluteEncoder(this.m_encoder, type);
    //     } catch (Exception e) {
    //         // TODO Auto-generated catch block
    //         e.printStackTrace();
    //     }
    // }

    // TODO: Finish following
    // @Override
    // public REVLibError follow(CANSparkBase leader) {
    //     REVLibError err = super.follow(leader);
    //     // figure out how to have primitives follow
    //     this.m_motor.m_percentOuput = leader.x;
    //     return err;
    // }
}

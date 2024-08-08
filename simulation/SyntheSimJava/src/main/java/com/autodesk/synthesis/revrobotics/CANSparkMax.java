package com.autodesk.synthesis.revrobotics;

import java.util.ArrayList;

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
    private ArrayList<CANSparkMax> followers;

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
        this.followers = new ArrayList();
    }

    // setting a follower doesn't break the simulated follower - leader relationship
    @Override
    public void set(double percent) {
        super.set(percent);
        this.m_motor.setPercentOutput(percent);
        for (CANSparkMax follower : this.followers) {
            follower.set(percent);
        }
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

    /// Use instead on getAbsoluteEncoder(), everything else works exactly the same in every way but name
    public com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder getAbsoluteEncoderSim() {
        return new SparkAbsoluteEncoder(super.getAbsoluteEncoder(), this.m_encoder);
    }

    public void newFollower(CANSparkMax f) {
        this.followers.add(f);
    }

    // Must pass in a simulation-supported leader to have the simulated portion of this motor follow
    @Override
    public REVLibError follow(CANSparkBase leader) {
        REVLibError err = super.follow(leader);
        if (leader instanceof CANSparkMax) {
            ((CANSparkMax) leader).newFollower(this);
        }
        return err;
    }
}

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
     *
     * See original documentation for more information https://codedocs.revrobotics.com/java/com/revrobotics/cansparkmax
     */
    public CANSparkMax(int deviceId, MotorType motorType) {
        super(deviceId, motorType);

        this.m_motor = new CANMotor("SYN CANSparkMax", deviceId, 0.0, false, 0.3);
        this.m_encoder = new CANEncoder("SYN CANSparkMax", deviceId);
        this.followers = new ArrayList();
    }

    /* 
     * Sets the percent output of the motor
     * Setting a follower doesn't break the simulated follower - leader relationship, which it does for exclusively non-simulated motors
     *
     * See the original documentation for more information 
     */
    @Override
    public void set(double percent) {
        super.set(percent);
        this.m_motor.setPercentOutput(percent);
        for (CANSparkMax follower : this.followers) {
            follower.set(percent);
        }
    }

    void setNeutralDeadband(double n) {
        this.m_motor.setNeutralDeadband(n);
    }

    @Override
    public REVLibError setIdleMode(com.revrobotics.CANSparkBase.IdleMode mode) {
        if (mode != null) {
            this.m_motor.setBrakeMode(mode.equals(com.revrobotics.CANSparkBase.IdleMode.kBrake));
        }

        return super.setIdleMode(mode);
    }

    /* 
     * Returns a simulation-supported SparkAbsoluteEncoder containing the position and velocity of the motor in fission.
     * All information returned by this class besides position and velocity is from the real motor
     * Use instead on getAbsoluteEncoder(), everything except for the name of the method works exactly the same
     */
    public com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder getAbsoluteEncoderSim() {
        return new SparkAbsoluteEncoder(super.getAbsoluteEncoder(), this.m_encoder);
    }

    void newFollower(CANSparkMax f) {
        this.followers.add(f);
    }

    /* 
     * Causes a simulation-supported leader to follow another simulation-supported leader
     * The real versions of these motors will also follow each other
     *
     */
    @Override
    public REVLibError follow(CANSparkBase leader) {
        REVLibError err = super.follow(leader);
        if (leader instanceof CANSparkMax) {
            ((CANSparkMax) leader).newFollower(this);
        }
        return err;
    }
}

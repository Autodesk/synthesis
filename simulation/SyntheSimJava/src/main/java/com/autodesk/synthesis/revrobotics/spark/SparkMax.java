package com.autodesk.synthesis.revrobotics.spark;

import java.util.ArrayList;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;
import com.autodesk.synthesis.revrobotics.RelativeEncoder;
import com.autodesk.synthesis.revrobotics.SparkAbsoluteEncoder;
import com.revrobotics.REVLibError;
import com.revrobotics.PersistMode;
import com.revrobotics.ResetMode;
import com.revrobotics.spark.SparkBase;
import com.revrobotics.spark.SparkLowLevel;
import com.revrobotics.spark.config.SparkBaseConfig.IdleMode;
import com.revrobotics.spark.config.SparkMaxConfig;

/**
 * SparkMax wrapper to add proper WPILib HALSim support.
 *
 * See original documentation for more information https://codedocs.revrobotics.com/java/com/revrobotics/spark/sparkmax
 */
public class SparkMax extends com.revrobotics.spark.SparkMax {

    private CANMotor m_motor;
    public CANEncoder m_encoder;
    private ArrayList<SparkMax> m_followers;

    /**
     * Creates a new SparkMax, wrapped with simulation support.
     *
     * @param deviceId  CAN Device ID.
     * @param motorType Motor type.
     */
    public SparkMax(int deviceId, SparkLowLevel.MotorType motorType) {
        super(deviceId, motorType);

        this.m_motor = new CANMotor("SYN SparkMax", deviceId, 0.0, false, 0.3);
        this.m_encoder = new CANEncoder("SYN SparkMax", deviceId);
        this.m_followers = new ArrayList<SparkMax>();
    }

    /**
     * Sets the percent output of the real and simulated motors.
     * Propagates to all registered sim followers.
     *
     * @param percent The new percent output [-1.0, 1.0]
     */
    @Override
    public void set(double percent) {
        super.set(percent);
        this.m_motor.setPercentOutput(percent);
        for (SparkMax follower : this.m_followers) {
            follower.set(percent);
        }
    }

    /**
     * Sets the idle mode of the real and simulated motors.
     *
     * @param mode IdleMode.kBrake or IdleMode.kCoast
     * @return REVLibError indicating success or failure
     */
    public REVLibError setIdleMode(IdleMode mode) {
        if (mode != null) {
            this.m_motor.setBrakeMode(mode == IdleMode.kBrake);
        }
        SparkMaxConfig config = new SparkMaxConfig();
        config.idleMode(mode);
        return configure(config, ResetMode.kNoResetSafeParameters, PersistMode.kNoPersistParameters);
    }

    /**
     * Gets a simulation-supported RelativeEncoder.
     * Position and velocity come from Synthesis; all other reads delegate to the real encoder.
     *
     * @return The simulation-supported RelativeEncoder.
     */
    public RelativeEncoder getEncoderSim() {
        return new RelativeEncoder(super.getEncoder(), this.m_encoder);
    }

    /**
     * Gets a simulation-supported SparkAbsoluteEncoder.
     * Position and velocity come from Synthesis; all other reads delegate to the real encoder.
     *
     * @return The simulation-supported SparkAbsoluteEncoder.
     */
    public SparkAbsoluteEncoder getAbsoluteEncoderSim() {
        return new SparkAbsoluteEncoder(super.getAbsoluteEncoder(), this.m_encoder);
    }

    void addFollower(SparkMax follower) {
        this.m_followers.add(follower);
    }

    /**
     * Causes this motor to follow another simulation-supported SparkMax leader.
     * Both the real and simulated motors will follow the leader.
     *
     * @param leader The SparkMax for this motor to follow.
     * @return REVLibError indicating success or failure
     */
    public REVLibError followSim(SparkMax leader) {
        if (leader != null) {
            leader.addFollower(this);
        }
        SparkMaxConfig config = new SparkMaxConfig();
        config.follow(leader);
        return configure(config, ResetMode.kNoResetSafeParameters, PersistMode.kNoPersistParameters);
    }
}

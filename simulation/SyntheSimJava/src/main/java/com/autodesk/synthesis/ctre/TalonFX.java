package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;

/**
 * TalonFX wrapper to add proper WPILib HALSim support.
 *
 * In Phoenix6 26, set(), setNeutralMode(), getPosition(), and getVelocity() are all
 * final and cannot be overridden. Call syncSim() each robot periodic cycle to push
 * the current motor output and encoder state into Synthesis.
 */
public class TalonFX extends com.ctre.phoenix6.hardware.TalonFX {
    private CANMotor m_motor;
    private CANEncoder m_encoder;

    /**
     * Creates a new TalonFX, wrapped with simulation support.
     *
     * @param deviceNumber CAN Device ID.
     */
    public TalonFX(int deviceNumber) {
        super(deviceNumber);

        this.m_motor = new CANMotor("SYN TalonFX", deviceNumber, 0.0, false, 0.3);
        this.m_encoder = new CANEncoder("SYN TalonFX", deviceNumber);
    }

    /**
     * Syncs the current motor output and encoder state into Synthesis.
     * Call this once per robot periodic cycle (e.g. in robotPeriodic()).
     */
    public void syncSim() {
        m_motor.setPercentOutput(this.get());
        this.setPosition(m_encoder.getPosition());
    }
}

package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANEncoder;
import com.autodesk.synthesis.CANMotor;
import com.ctre.phoenix6.signals.NeutralModeValue;
import com.ctre.phoenix6.StatusSignal;
import com.ctre.phoenix6.configs.TalonFXConfigurator;
import com.ctre.phoenix6.hardware.DeviceIdentifier;

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
     * Sets the torque of the real and simulated motors
     *
     * @param percentOutput The torque
     */
    @Override
    public void set(double percentOutput) {
        super.set(percentOutput);
        this.m_motor.setPercentOutput(percentOutput);
    }

    /**
     * Sets both the real and simulated motors to neutral mode
     *
     * @param mode The neutral mode value
     *
     */
    @Override
    public void setNeutralMode(NeutralModeValue mode) {
        super.setNeutralMode(mode);

        this.m_motor.setBrakeMode(mode == NeutralModeValue.Brake);
    }

    /**
     * Gets and internal configurator for both the simulated and real motors
     *
     * @return The internal configurator for this Talon motor
     */
    @Override
    public TalonFXConfigurator getConfigurator() {
        DeviceIdentifier id = this.deviceIdentifier;
        return new com.autodesk.synthesis.ctre.TalonFXConfigurator(id, this);
    }

    // called internally by the configurator to set the deadband, not for user use
    public void setNeutralDeadband(double deadband) {
        this.m_motor.setNeutralDeadband(deadband);
    }

    /**
     * Gets the position of the simulated encoder
     *
     * @return The motor position in revolutions
     */
    @Override
    public StatusSignal<Double> getPosition() {
        Double pos = this.m_encoder.getPosition();
        super.setPosition(pos);
        return super.getPosition();
    }

    /**
     * Gets the velocity of the simulated motor according to the simulated encoder
     *
     * @return The motor velocity in revolutions per second
     */
    @Override
    public StatusSignal<Double> getVelocity() {
        Double velocity = this.m_encoder.getVelocity();
        super.set(velocity);
        return super.getVelocity();
    }
}

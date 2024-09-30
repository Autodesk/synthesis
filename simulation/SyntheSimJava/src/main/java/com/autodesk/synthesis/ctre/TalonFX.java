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

    /// I think we're getting percentOutput and speed mixed up
    @Override
    public void set(double percentOutput) {
        super.set(percentOutput);
        this.m_motor.setPercentOutput(percentOutput);
    }

    @Override
    public void setNeutralMode(NeutralModeValue mode) {
        super.setNeutralMode(mode);

        this.m_motor.setBrakeMode(mode == NeutralModeValue.Brake);
    }

    @Override
    public TalonFXConfigurator getConfigurator() {
        DeviceIdentifier id = this.deviceIdentifier;
        return new com.autodesk.synthesis.ctre.TalonFXConfigurator(id, this);
    }

    // called internally by the configurator to set the deadband, not for user use
    public void setNeutralDeadband(double deadband) {
        this.m_motor.setNeutralDeadband(deadband);
    }

    @Override
    public StatusSignal<Double> getPosition() {
        Double pos = this.m_encoder.getPosition();
        super.setPosition(pos);
        return super.getPosition();
    }

    /// I think this is a pointless method
    @Override
    public StatusSignal<Double> getVelocity() {
        Double velocity = this.m_encoder.getVelocity();
        super.set(velocity);
        return super.getVelocity();
    }
}

package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANMotor;
import com.ctre.phoenix6.signals.NeutralModeValue;
import com.ctre.phoenix6.configs.TalonFXConfigurator;
import com.ctre.phoenix6.hardware.DeviceIdentifier;

public class TalonFX extends com.ctre.phoenix6.hardware.TalonFX {
    private CANMotor m_motor;

    /**
     * Creates a new TalonFX, wrapped with simulation support.
     * 
     * @param deviceId CAN Devic ID.
     */
    public TalonFX(int deviceNumber) {
        super(deviceNumber);

        this.m_motor = new CANMotor("SYN TalonFX", deviceNumber, 0.0, false, 0.3);
    }

    @Override
    public void set(double speed) {
        super.set(speed);
        this.m_motor.setPercentOutput(speed);
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
}

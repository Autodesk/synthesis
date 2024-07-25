package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANMotor;
// import com.ctre.phoenix6.configs.TalonFXConfigurator;
// import com.ctre.phoenix6.configs.TalonFXConfiguration;
import com.ctre.phoenix6.signals.NeutralModeValue;

public class TalonFX extends com.ctre.phoenix6.hardware.TalonFX {
    private CANMotor m_motor;

    public TalonFX(int deviceNumber) {
        super(deviceNumber);

        m_motor = new CANMotor("SYN TalonFX", deviceNumber, 0.0, false, 0.3);
    }

    public void set(double speed) {
        super.set(speed);
        this.m_motor.setPercentOutput(speed);
    }

    public void setNeutralMode(NeutralModeValue mode) {
        super.setNeutralMode(mode);

        this.m_motor.setBrakeMode(mode == NeutralModeValue.Brake);
    }

    public void configureNeutralDeadband(double deadband) {
        //super.configureNeutralDeadband(deadband);
        // TODO: Find actual deadband config method
        this.m_motor.setNeutralDeadband(deadband);
    }
}

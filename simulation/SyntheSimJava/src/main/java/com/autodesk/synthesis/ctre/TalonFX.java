package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANMotor;
import com.ctre.phoenix.motorcontrol.NeutralMode;

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

    public void setNeutralMode(NeutralMode mode) {
        super.setNeutralMode(mode);

        this.m_motor.setBrakeMode(mode == NeutralMode.Brake);
    }

    public void configureNeutralDeadaband(double deadband) {
        this.m_motor.setNeutralDeadband(deadband);
    }
}

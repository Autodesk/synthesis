package com.autodesk.synthesis.ctre;

import com.autodesk.synthesis.CANMotor;

public class TalonFX extends com.ctre.phoenix6.hardware.TalonFX {
    private CANMotor m_motor;

    public TalonFX(int deviceId) {
        super(deviceId);

        m_motor = new CANMotor("SYN TalonFX", deviceId, 0.0, false, 0.3);
    }
}

package com.autodesk.synthesis.revrobotics;

import com.revrobotics.REVLibError;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDouble;
import edu.wpi.first.hal.SimDevice.Direction;

public class CANSparkMax extends com.revrobotics.CANSparkMax {

    private SimDevice m_device;
    private SimDouble m_percentOutput;
    private SimBoolean m_brakeMode;
    private SimDouble m_neutralDeadband;

    public CANSparkMax(int deviceId, MotorType motorType) {
        super(deviceId, motorType);

        m_device = SimDevice.create("SYN CANSparkMax", deviceId);
        m_percentOutput = m_device.createDouble("percentOutput", Direction.kOutput, 0.0);
        m_brakeMode = m_device.createBoolean("brakeMode", Direction.kOutput, false);
    }

    @Override
    public void set(double percent) {
        if (Double.isNaN(percent) || Double.isInfinite(percent)) {
            percent = 0.0;
        }

        m_percentOutput.set(Math.min(1.0, Math.max(-1.0, percent)));

        super.set(percent);
    }

    @Override
    public REVLibError setIdleMode(com.revrobotics.CANSparkBase.IdleMode mode) {
        if (mode != null) {
            m_brakeMode.set(mode.equals(com.revrobotics.CANSparkBase.IdleMode.kBrake));
        }

        return super.setIdleMode(mode);
    }

}

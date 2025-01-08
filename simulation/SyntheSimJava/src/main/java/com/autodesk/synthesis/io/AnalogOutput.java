package com.autodesk.synthesis.io;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDouble;
import edu.wpi.first.hal.SimDevice.Direction;

public class AnalogOutput extends edu.wpi.first.wpilibj.AnalogOutput {
    private SimDevice m_device;

    private SimBoolean m_init;
    private SimDouble m_voltage;

    public AnalogOutput(int channel) {
        super(channel);

        m_device = SimDevice.create("AI:SYN AO", channel);

        m_init = m_device.createBoolean("init", Direction.kOutput, true);
        m_voltage = m_device.createDouble("voltage", Direction.kOutput, 0.0);
    }

    @Override
    public void setVoltage(double voltage) {
        m_voltage.set(voltage);
    }

    @Override
    public double getVoltage() {
        return m_voltage.get();
    }
}

package com.autodesk.synthesis.io;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDouble;
import edu.wpi.first.hal.SimDevice.Direction;

public class DigitalOutput extends edu.wpi.first.wpilibj.DigitalOutput {
    private SimDevice m_device;

    private SimBoolean m_init;
    private SimBoolean m_input;
    private SimBoolean m_value;
    private SimDouble m_pulseLength; // unused but in HALSim spec

    public DigitalOutput(int channel) {
        super(channel);

        m_device = SimDevice.create("DIO:SYN DO", channel);

        m_init = m_device.createBoolean("init", Direction.kOutput, true);
        m_input = m_device.createBoolean("input", Direction.kOutput, true);
        m_value = m_device.createBoolean("value", Direction.kBidir, false);
        m_pulseLength = m_device.createDouble("pulse_length", Direction.kOutput, 0.0);

        this.setSimDevice(m_device);
    }

    @Override
    public boolean get() {
        return m_value.get();
    }

    @Override
    public void set(boolean value) {
        m_value.set(value);
    }
}

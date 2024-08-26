package com.autodesk.synthesis.io;

import edu.wpi.first.hal.SimBoolean;
import edu.wpi.first.hal.SimDevice;
import edu.wpi.first.hal.SimDouble;
import edu.wpi.first.hal.SimInt;
import edu.wpi.first.hal.SimDevice.Direction;

public class AnalogInput extends edu.wpi.first.wpilibj.AnalogInput {
    private SimDevice m_device;

    private SimBoolean m_init;
    private SimInt m_avgBits;
    private SimInt m_oversampleBits;
    private SimDouble m_voltage;
    private SimBoolean m_accumInit;
    private SimInt m_accumValue;
    private SimInt m_accumCount;
    private SimInt m_accumCenter;
    private SimInt m_accumDeadband;

    public AnalogInput(int channel) {
        super(channel);

        m_device = SimDevice.create("AI:SYN AI", channel);

        m_init = m_device.createBoolean("init", Direction.kOutput, true);
        m_avgBits = m_device.createInt("avg_bits", Direction.kOutput, 0);
        m_oversampleBits = m_device.createInt("oversample_bits", Direction.kOutput, 0);
        m_voltage = m_device.createDouble("voltage", Direction.kInput, 0.0);
        m_accumInit = m_device.createBoolean("accum_init", Direction.kOutput, false);
        m_accumValue = m_device.createInt("accum_value", Direction.kInput, 0);
        m_accumCount = m_device.createInt("accum_count", Direction.kInput, 0);
        m_accumCenter = m_device.createInt("accum_center", Direction.kOutput, 0);
        m_accumDeadband = m_device.createInt("accum_deadband", Direction.kOutput, 0);

        this.setSimDevice(m_device);
    }

    @Override
    public double getVoltage() {
        return m_voltage.get();
    }

    @Override
    public int getAverageBits() {
        return m_avgBits.get();
    }

    @Override
    public void setAverageBits(int bits) {
        m_avgBits.set(bits);
    }

    @Override
    public int getOversampleBits() {
        return m_oversampleBits.get();
    }

    @Override
    public void setOversampleBits(int bits) {
        m_oversampleBits.set(bits);
    }

    @Override
    public void initAccumulator() {
        super.initAccumulator();
        m_accumInit.set(true);
    }

    @Override
    public long getAccumulatorValue() {
        return m_accumValue.get();
    }

    @Override
    public long getAccumulatorCount() {
        return m_accumCount.get();
    }

    @Override
    public void setAccumulatorCenter(int center) {
        m_accumCenter.set(center);
    }

    @Override
    public void setAccumulatorDeadband(int deadband) {
        m_accumDeadband.set(deadband);
    }
}

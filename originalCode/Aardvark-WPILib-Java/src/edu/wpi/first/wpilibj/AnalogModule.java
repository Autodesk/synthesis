/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.AICalibration;
import edu.wpi.first.wpilibj.communication.ModulePresence;
import edu.wpi.first.wpilibj.fpga.tAI;

/**
 * Analog Module class.
 * Each module can independently sample its channels at a configurable rate.
 * There is a 64-bit hardware accumulator associated with channel 1 on each module.
 * The accumulator is attached to the output of the oversample and average engine so that the center
 * value can be specified in higher resolution resulting in less error.
 */
public class AnalogModule extends Module {

    /**
     * The time base used by the module
     */
    public static final int kTimebase = 40000000;
    /**
     * The default number of Oversample bits to use
     */
    public static final int kDefaultOversampleBits = 0;
    /**
     * The default number of averaging bits to use
     */
    public static final int kDefaultAverageBits = 7;
    /**
     * The default sample rate
     */
    public static final double kDefaultSampleRate = 50000.0;
    private tAI m_module;
    private boolean m_sampleRateSet;
    private long m_accumulatorOffset;
    private int m_numChannelsToActivate;
    private final Object syncRoot = new Object();

    /**
     * Get an instance of an Analog Module.
     *
     * Singleton analog module creation where a module is allocated on the first use
     * and the same module is returned on subsequent uses.
     *
     * @param moduleNumber The index of the analog module to get (1 or 2).
     * @return The AnalogModule.
     */
    public static synchronized AnalogModule getInstance(final int moduleNumber) {
        checkAnalogModule(moduleNumber);
        return (AnalogModule) getModule(ModulePresence.ModuleType.kAnalog, moduleNumber);
    }

    /**
     * Create a new instance of an analog module.
     *
     * Create an instance of the analog module object. Initialize all the parameters
     * to reasonable values on start.
     * Setting a global value on an analog module can be done only once unless subsequent
     * values are set the previously set value.
     * Analog modules are a singleton, so the constructor is never called outside of this class.
     *
     * @param moduleNumber The index of the analog module to create (1 or 2).
     */
    protected AnalogModule(final int moduleNumber) {
        super(ModulePresence.ModuleType.kAnalog, moduleNumber);

        m_module = new tAI(moduleNumber - 1);
        setNumChannelsToActivate(SensorBase.kAnalogChannels);
        setSampleRate(AnalogModule.kDefaultSampleRate);

        for (int i = 0; i < SensorBase.kAnalogChannels; i++) {
            m_module.writeScanList(i, i);
            setAverageBits(i + 1, kDefaultAverageBits);
            setOversampleBits(i + 1, kDefaultOversampleBits);
        }
    }

    /**
     * Set the sample rate on the module.
     *
     * This is a global setting for the module and effects all channels.
     *
     * @param samplesPerSecond
     *            The number of samples per channel per second.
     */
    public void setSampleRate(final double samplesPerSecond) {
        // TODO: This will change when variable size scan lists are implemented.
        // TODO: Need float comparison with epsilon.
        // wpi_assert(!sampleRateSet || GetSampleRate() == samplesPerSecond);
        m_sampleRateSet = true;

        // Compute the convert rate
        final int ticksPerSample = (int) ((double)AnalogModule.kTimebase / samplesPerSecond);
        int ticksPerConversion = ticksPerSample / getNumChannelsToActivate();
        // ticksPerConversion must be at least 80
        // wpi_assert(ticksPerConversion >= 80);
        if (ticksPerConversion < 80) {
            ticksPerConversion = 80;
        }

        // Atomically set the scan size and the convert rate so that the sample
        // rate is constant
        m_module.writeConfig_ScanSize(getNumChannelsToActivate());
        m_module.writeConfig_ConvertRate(ticksPerConversion);

        // Indicate that the scan size has been committed to hardware.
        setNumChannelsToActivate(0);
    }

    /**
     * Get the current sample rate on the module.
     *
     * This assumes one entry in the scan list. This is a global setting for the
     * module and effects all channels.
     *
     * @return Sample rate.
     */
    public double getSampleRate() {
        final long ticksPerConversion = m_module.readLoopTiming();
        final long ticksPerSample = ticksPerConversion * getNumActiveChannels();

        return (double)AnalogModule.kTimebase / (double)ticksPerSample;
    }

    /**
     * Return the number of channels on the module in use.
     *
     * @return Active channels.
     */
    private int getNumActiveChannels() {
        final int scanSize = m_module.readConfig_ScanSize();
        return scanSize == 0 ? 8 : scanSize;
    }

    /**
     * Get the number of active channels.
     *
     * This is an internal function to allow the atomic update of both the
     * number of active channels and the sample rate.
     *
     * When the number of channels changes, use the new value.  Otherwise,
     * return the curent value.
     *
     * @return Value to write to the active channels field.
     */
    private int getNumChannelsToActivate() {
        if (m_numChannelsToActivate == 0) {
            return getNumActiveChannels();
        }
        return m_numChannelsToActivate;
    }

    /**
     * Set the number of active channels.
     *
     * Store the number of active channels to set.  Don't actually commit to hardware
     * until SetSampleRate().
     *
     * @param channels Number of active channels.
     */
    private void setNumChannelsToActivate(final int channels) {
        m_numChannelsToActivate = channels;
    }

    /**
     * Set the number of averaging bits.
     *
     * This sets the number of averaging bits. The actual number of averaged
     * samples is 2**bits. The averaging is done automatically in the FPGA.
     *
     * @param channel
     *            Analog channel to configure.
     * @param bits
     *            Number of bits to average.
     */
    public void setAverageBits(final int channel, final int bits) {
        m_module.writeAverageBits(channel - 1, bits);
    }

    /**
     * Get the number of averaging bits.
     *
     * This gets the number of averaging bits from the FPGA. The actual number
     * of averaged samples is 2**bits. The averaging is done automatically in
     * the FPGA.
     *
     * @param channel Channel to address.
     * @return Bits to average.
     */
    public int getAverageBits(final int channel) {
        return m_module.readAverageBits(channel - 1);
    }

    /**
     * Set the number of oversample bits.
     *
     * This sets the number of oversample bits. The actual number of oversampled values is 2**bits.
     * Use oversampling to improve the resolution of your measurements at the expense of sampling rate.
     * The oversampling is done automatically in the FPGA.
     *
     * @param channel Analog channel to configure.
     * @param bits Number of bits to oversample.
     */
    public void setOversampleBits(final int channel, final int bits) {
        m_module.writeOversampleBits(channel - 1, bits);
    }

    /**
     * Get the number of oversample bits.
     *
     * This gets the number of oversample bits from the FPGA. The actual number
     * of oversampled values is 2**bits. The oversampling is done automatically
     * in the FPGA.
     *
     * @param channel Channel to address.
     * @return Bits to oversample.
     */
    public int getOversampleBits(final int channel) {
        return m_module.readOversampleBits(channel - 1);
    }

    /**
     * Get the raw analog value.
     *
     * Get the analog value as it is returned from the D/A converter.
     *
     * @param channel Channel number to read.
     * @return Raw value.
     */
    public int getValue(final int channel) {
        int value;
        SensorBase.checkAnalogChannel(channel);

        synchronized (syncRoot) {
            tAI.writeReadSelect_Channel(channel - 1);
            tAI.writeReadSelect_Module(m_moduleNumber - 1);
            tAI.writeReadSelect_Averaged(false);

            tAI.strobeLatchOutput();
            value = tAI.readOutput();
        }

        return (short) value;
    }

    /**
     * Get the raw averaged and oversampled value.
     *
     * @param channel Channel number to read.
     * @return The averaged and oversampled raw value.
     */
    public int getAverageValue(final int channel) {
        int value;
        SensorBase.checkAnalogChannel(channel);

        synchronized (syncRoot) {
            tAI.writeReadSelect_Channel(channel - 1);
            tAI.writeReadSelect_Module(m_moduleNumber - 1);
            tAI.writeReadSelect_Averaged(true);

            tAI.strobeLatchOutput();
            value = tAI.readOutput();
        }

        return value;
    }

    /**
     * Convert a voltage to a raw value for a specified channel.
     *
     * This process depends on the calibration of each channel, so the channel
     * must be specified.
     *
     * @todo This assumes raw values. Oversampling not supported as is.
     *
     * @param channel
     *            The channel to convert for.
     * @param voltage
     *            The voltage to convert.
     * @return The raw value for the channel.
     */
    public int voltsToValue(final int channel, final double voltage) {
        // wpi_assert(voltage >= -10.0 && voltage <= 10.0);
        final long LSBWeight = getLSBWeight(channel);
        final int offset = getOffset(channel);
        final int value = (int) ((voltage + offset * 1.0e-9) / (LSBWeight * 1.0e-9));

        return value;
    }

    /**
     * Get the voltage.
     *
     * Return the voltage from the A/D converter.
     *
     * @param channel The channel to read.
     * @return The scaled voltage from the channel.
     */
    public double getVoltage(final int channel) {
        final int value = getValue(channel);
        final long LSBWeight = getLSBWeight(channel);
        final int offset = getOffset(channel);
        final double voltage = (LSBWeight * 1.0e-9 * value) - (offset * 1.0e-9);

        return voltage;
    }

    /**
     * Get the averaged voltage.
     *
     * Return the averaged voltage from the A/D converter.
     *
     * @param channel The channel to read.
     * @return The scaled averaged and oversampled voltage from the channel.
     */
    public double getAverageVoltage(final int channel) {
        final int value = getAverageValue(channel);
        final long LSBWeight = getLSBWeight(channel);
        final int offset = getOffset(channel);
        final int oversampleBits = getOversampleBits(channel);
        final double voltage = ((LSBWeight * 1.0e-9 * value) / (double)(1 << oversampleBits)) - (offset * 1.0e-9);

        return voltage;
    }

    /**
     * Get the LSB Weight portion of the calibration for a channel.
     *
     * @param channel The channel to get calibration data for.
     * @return LSB Weight calibration point.
     */
    public long getLSBWeight(final int channel) {
        // TODO: add scaling to make this worth while.
       return AICalibration.getLSBWeight(m_module.getSystemIndex(), channel - 1);
    }

    /**
     * Get the Offset portion of the calibration for a channel.
     *
     * @param channel The channel to get calibration data for.
     * @return Offset calibration point.
     */
    public int getOffset(final int channel) {
        // TODO: add scaling to make this worth while.
        // TODO: actually use this function.
       return AICalibration.getOffset(m_module.getSystemIndex(), channel - 1);
    }
}

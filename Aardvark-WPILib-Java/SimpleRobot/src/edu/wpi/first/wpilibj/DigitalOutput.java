/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.fpga.tDIO;
import edu.wpi.first.wpilibj.livewindow.LiveWindowSendable;
import edu.wpi.first.wpilibj.parsing.IInputOutput;
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.tables.ITableListener;

/**
 * Class to write digital outputs.
 * This class will wrtie digital outputs. Other devices
 * that are implemented elsewhere will automatically
 * allocate digital inputs and outputs as required.
 */
public class DigitalOutput extends DigitalSource implements IInputOutput, LiveWindowSendable {

    private int m_channel;
    private int m_pwmGenerator;
    private DigitalModule m_module;

    private void initDigitalOutput(int moduleNumber, int channel) {
        m_channel = channel;
        m_pwmGenerator = ~0;
        m_module = DigitalModule.getInstance(moduleNumber);
        m_module.allocateDIO(m_channel, false);

        UsageReporting.report(UsageReporting.kResourceType_DigitalOutput, channel, moduleNumber-1);
    }

    /**
     * Create an instance of a digital output.
     * Create an instance of a digital output given a module number and channel.
     * @param moduleNumber The number of the digital module to use
     * @param channel the port to use for the digital output
     */
    public DigitalOutput(int moduleNumber, int channel) {
        initDigitalOutput(moduleNumber, channel);
    }

    /**
     * Create an instance of a digital output.
     * Create a digital output given a channel. The default module is used.
     * @param channel the port to use for the digital output
     */
    public DigitalOutput(int channel) {
        initDigitalOutput(getDefaultDigitalModule(), channel);
    }

    /**
     * Free the resources associated with a digital output.
     */
    public void free() {
        disablePWM();
        m_module.freeDIO(m_channel);
    }

    /**
     * Set the value of a digital output.
     * @param value true is on, off is false
     */
    public void set(boolean value) {
        m_module.setDIO(m_channel, value);
    }

    /**
     * @return The GPIO channel number that this object represents.
     */
    public int getChannel() {
	return m_channel;
    }

    /**
     * Output a single pulse on the digital output line.
     * Send a single pulse on the digital output line where the pulse diration is specified in seconds.
     * Maximum pulse length is 0.0016 seconds.
     * @param length The pulselength in seconds
     */
    public void pulse(double length) {
        m_module.pulse(m_channel, (char) (1e9 * length / (tDIO.readLoopTiming() * 25)));
    }

    /**
     * Determine if the pulse is still going.
     * Determine if a previously started pulse is still going.
     * @return true if pulsing
     */
    public boolean isPulsing() {
        return m_module.isPulsing(m_channel);
    }

    /**
     * Change the PWM frequency of the PWM output on a Digital Output line.
     *
     * The valid range is from 0.6 Hz to 19 kHz.  The frequency resolution is logarithmic.
     *
     * There is only one PWM frequency per digital module.
     *
     * @param rate The frequency to output all digital output PWM signals on this module.
     */
    public void setPWMRate(double rate)
{
	m_module.setDO_PWMRate(rate);
    }

    /**
     * Enable a PWM Output on this line.
     *
     * Allocate one of the 4 DO PWM generator resources from this module.
     *
     * Supply the initial duty-cycle to output so as to avoid a glitch when first starting.
     *
     * The resolution of the duty cycle is 8-bit for low frequencies (1kHz or less)
     * but is reduced the higher the frequency of the PWM signal is.
     *
     * @param initialDutyCycle The duty-cycle to start generating. [0..1]
     */
    public void enablePWM(double initialDutyCycle) {
	if (m_pwmGenerator != ~0) return;
        m_pwmGenerator = m_module.allocateDO_PWM();
        m_module.setDO_PWMDutyCycle(m_pwmGenerator, initialDutyCycle);
        m_module.setDO_PWMOutputChannel(m_pwmGenerator, m_channel);
    }

    /**
     * Change this line from a PWM output back to a static Digital Output line.
     *
     * Free up one of the 4 DO PWM generator resources that were in use.
     */
    public void disablePWM() {
	// Disable the output by routing to a dead bit.
	m_module.setDO_PWMOutputChannel(m_pwmGenerator, kDigitalChannels);
        m_module.freeDO_PWM(m_pwmGenerator);
        m_pwmGenerator = ~0;
    }

    /**
     * Change the duty-cycle that is being generated on the line.
     *
     * The resolution of the duty cycle is 8-bit for low frequencies (1kHz or less)
     * but is reduced the higher the frequency of the PWM signal is.
     *
     * @param dutyCycle The duty-cycle to change to. [0..1]
     */
    public void updateDutyCycle(double dutyCycle) {
	m_module.setDO_PWMDutyCycle(m_pwmGenerator, dutyCycle);
    }

    /**
     * @return The value to be written to the channel field of a routing mux.
     */
    public int getChannelForRouting() {
	return DigitalModule.remapDigitalChannel(getChannel() - 1);
    }

    /**
     * @return The value to be written to the module field of a routing mux.
     */
    public int getModuleForRouting() {
	return m_module.getModuleNumber() - 1;
    }

    /**
     * @return The value to be written to the analog trigger field of a routing mux.
     */
    public boolean getAnalogTriggerForRouting() {
	return false;
    }
    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Digital Output";
    }
    private ITable m_table;
    private ITableListener m_table_listener;
    
    /**
     * {@inheritDoc}
     */
    public void initTable(ITable subtable) {
        m_table = subtable;
        updateTable();
    }
    
    /**
     * {@inheritDoc}
     */
    public ITable getTable(){
        return m_table;
    }
    
    /**
     * {@inheritDoc}
     */
    public void updateTable() {
        // TODO: Put current value.
    }
    
    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {
        m_table_listener = new ITableListener() {
            public void valueChanged(ITable itable, String key, Object value, boolean bln) {
                set(((Boolean) value).booleanValue());
            }
        };
        m_table.addTableListener("Value", m_table_listener, true);
    }
    
    /**
     * {@inheritDoc}
     */
    public void stopLiveWindowMode() {
        // TODO: Broken, should only remove the listener from "Value" only.
        m_table.removeTableListener(m_table_listener);
    }
}

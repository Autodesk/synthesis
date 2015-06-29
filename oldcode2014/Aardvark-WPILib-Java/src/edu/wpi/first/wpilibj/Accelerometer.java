/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;
import edu.wpi.first.wpilibj.livewindow.LiveWindowSendable;
import edu.wpi.first.wpilibj.parsing.ISensor;
import edu.wpi.first.wpilibj.tables.ITable;

/**
 * Handle operation of the accelerometer.
 * The accelerometer reads acceleration directly through the sensor. Many sensors have
 * multiple axis and can be treated as multiple devices. Each is calibrated by finding
 * the center value over a period of time.
 */
public class Accelerometer extends SensorBase implements PIDSource, ISensor, LiveWindowSendable {

    private AnalogChannel m_analogChannel;
    private double m_voltsPerG = 1.0;
    private double m_zeroGVoltage = 2.5;
    private boolean m_allocatedChannel;
    
    /**
     * Common initialization
     */
    private void initAccelerometer() {
        UsageReporting.report(UsageReporting.kResourceType_Accelerometer, m_analogChannel.getChannel(), m_analogChannel.getModuleNumber()-1);
        LiveWindow.addSensor("Accelerometer", m_analogChannel.getModuleNumber(), m_analogChannel.getChannel(), this);
    }

    /**
     * Create a new instance of an accelerometer.
     *
     * The accelerometer is assumed to be in the first analog module in the given analog channel. The
     * constructor allocates desired analog channel.
     * @param channel the port that the accelerometer is on on the default module
     */
    public Accelerometer(final int channel) {
        m_allocatedChannel = true;
        m_analogChannel = new AnalogChannel(channel);
        initAccelerometer();
    }

    /**
     * Create new instance of accelerometer.
     *
     * Make a new instance of the accelerometer given a module and channel. The constructor allocates
     * the desired analog channel from the specified module
     * @param slot the slot that the module is in
     * @param channel the port that the Accelerometer is on on the module
     */
    public Accelerometer(final int slot, final int channel) {
        m_allocatedChannel = true;
        m_analogChannel = new AnalogChannel(slot, channel);
        initAccelerometer();
    }

    /**
     * Create a new instance of Accelerometer from an existing AnalogChannel.
     * Make a new instance of accelerometer given an AnalogChannel. This is particularly
     * useful if the port is going to be read as an analog channel as well as through
     * the Accelerometer class.
     * @param channel an already initialized analog channel
     */
    public Accelerometer(AnalogChannel channel) {
        m_allocatedChannel = false;
        if (channel == null)
            throw new NullPointerException("Analog Channel given was null");
        m_analogChannel = channel;
        initAccelerometer();
    }

    /**
     * Delete the analog components used for the accelerometer.
     */
    public void free() {
        if (m_analogChannel != null && m_allocatedChannel) {
            m_analogChannel.free();
        }
        m_analogChannel = null;
    }

    /**
     * Return the acceleration in Gs.
     *
     * The acceleration is returned units of Gs.
     *
     * @return The current acceleration of the sensor in Gs.
     */
    public double getAcceleration() {
        return (m_analogChannel.getAverageVoltage() - m_zeroGVoltage) / m_voltsPerG;
    }

    /**
     * Set the accelerometer sensitivity.
     *
     * This sets the sensitivity of the accelerometer used for calculating the acceleration.
     * The sensitivity varies by accelerometer model.
     *
     * @param sensitivity The sensitivity of accelerometer in Volts per G.
     */
    public void setSensitivity(double sensitivity) {
        m_voltsPerG = sensitivity;
    }

    /**
     * Set the voltage that corresponds to 0 G.
     *
     * The zero G voltage varies by accelerometer model.
     *
     * @param zero The zero G voltage.
     */
    public void setZero(double zero) {
        m_zeroGVoltage = zero;
    }

    /**
     * Get the Acceleration for the PID Source parent.
     *
     * @return The current acceleration in Gs.
     */
    public double pidGet() {
        return getAcceleration();
    }

    public String getSmartDashboardType(){
        return "Accelerometer";
    }
    /*
     * Live Window code, only does anything if live window is activated.
     */
    private ITable m_table;
    
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
        if (m_table != null) {
            m_table.putNumber("Value", getAcceleration());
        }
    }
    
    /**
     * Analog Channels don't have to do anything special when entering the LiveWindow.
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {}
    
    /**
     * Analog Channels don't have to do anything special when exiting the LiveWindow.
     * {@inheritDoc}
     */
    public void stopLiveWindowMode() {}
}

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
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.tables.ITableListener;
import edu.wpi.first.wpilibj.util.AllocationException;
import edu.wpi.first.wpilibj.util.CheckedAllocationException;

/**
 * Solenoid class for running high voltage Digital Output (9472 module).
 *
 * The Solenoid class is typically used for pneumatics solenoids, but could be used
 * for any device within the current spec of the 9472 module.
 */
public class Solenoid extends SolenoidBase implements LiveWindowSendable {

    private int m_channel; ///< The channel on the module to control.

    /**
     * Common function to implement constructor behavior.
     */
    private synchronized void initSolenoid() {
        checkSolenoidModule(m_moduleNumber);
        checkSolenoidChannel(m_channel);

        try {
            m_allocated.allocate((m_moduleNumber - 1) * kSolenoidChannels + m_channel - 1);
        } catch (CheckedAllocationException e) {
            throw new AllocationException(
                    "Solenoid channel " + m_channel + " on module " + m_moduleNumber + " is already allocated");
        }

        LiveWindow.addActuator("Solenoid", m_moduleNumber, m_channel, this);
        UsageReporting.report(UsageReporting.kResourceType_Solenoid, m_channel, m_moduleNumber - 1);
    }

    /**
     * Constructor.
     *
     * @param channel The channel on the module to control.
     */
    public Solenoid(final int channel) {
        super(getDefaultSolenoidModule());
        m_channel = channel;
        initSolenoid();
    }

    /**
     * Constructor.
     *
     * @param moduleNumber The module number of the solenoid module to use.
     * @param channel The channel on the module to control.
     */
    public Solenoid(final int moduleNumber, final int channel) {
        super(moduleNumber);
        m_channel = channel;
        initSolenoid();
    }

    /**
     * Destructor.
     */
    public synchronized void free() {
        m_allocated.free((m_moduleNumber - 1) * kSolenoidChannels + m_channel - 1);
    }

    /**
     * Set the value of a solenoid.
     *
     * @param on Turn the solenoid output off or on.
     */
    public void set(boolean on) {
        byte value = (byte)(on ? 0xFF : 0x00);
        byte mask = (byte)(1 << (m_channel - 1));

        set(value, mask);
    }

    /**
     * Read the current value of the solenoid.
     *
     * @return The current value of the solenoid.
     */
    public boolean get() {
	int value = getAll() & ( 1 << (m_channel - 1));
	return (value != 0);
    }

    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Solenoid";
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
        if (m_table != null) {
            m_table.putBoolean("Value", get());
        }
    }
    

    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {
        set(false); // Stop for safety
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
        set(false); // Stop for safety
        // TODO: Broken, should only remove the listener from "Value" only.
        m_table.removeTableListener(m_table_listener);
    }
}

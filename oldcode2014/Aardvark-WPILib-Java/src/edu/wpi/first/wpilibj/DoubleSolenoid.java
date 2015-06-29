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
 * DoubleSolenoid class for running 2 channels of high voltage Digital Output
 * (9472 module).
 * 
 * The DoubleSolenoid class is typically used for pneumatics solenoids that
 * have two positions controlled by two separate channels.
 */
public class DoubleSolenoid extends SolenoidBase implements LiveWindowSendable {

    /**
     * Possible values for a DoubleSolenoid
     */
    public static class Value {

        public final int value;
        public static final int kOff_val = 0;
        public static final int kForward_val = 1;
        public static final int kReverse_val = 2;
        public static final Value kOff = new Value(kOff_val);
        public static final Value kForward = new Value(kForward_val);
        public static final Value kReverse = new Value(kReverse_val);

        private Value(int value) {
            this.value = value;
        }
    }
    private int m_forwardChannel; ///< The forward channel on the module to control.
    private int m_reverseChannel; ///< The reverse channel on the module to control.
    private byte m_forwardMask; ///< The mask for the forward channel.
    private byte m_reverseMask; ///< The mask for the reverse channel.

    /**
     * Common function to implement constructor behavior.
     */
    private synchronized void initSolenoid() {
        checkSolenoidModule(m_moduleNumber);
        checkSolenoidChannel(m_forwardChannel);
        checkSolenoidChannel(m_reverseChannel);

        try {
            m_allocated.allocate((m_moduleNumber - 1) * kSolenoidChannels + m_forwardChannel - 1);
        } catch (CheckedAllocationException e) {
            throw new AllocationException(
                    "Solenoid channel " + m_forwardChannel + " on module " + m_moduleNumber + " is already allocated");
        }
        try {
            m_allocated.allocate((m_moduleNumber - 1) * kSolenoidChannels + m_reverseChannel - 1);
        } catch (CheckedAllocationException e) {
            throw new AllocationException(
                    "Solenoid channel " + m_reverseChannel + " on module " + m_moduleNumber + " is already allocated");
        }
        m_forwardMask = (byte) (1 << (m_forwardChannel - 1));
        m_reverseMask = (byte) (1 << (m_reverseChannel - 1));

        UsageReporting.report(UsageReporting.kResourceType_Solenoid, m_forwardChannel, m_moduleNumber-1);
        UsageReporting.report(UsageReporting.kResourceType_Solenoid, m_reverseChannel, m_moduleNumber-1);
        LiveWindow.addActuator("DoubleSolenoid", m_moduleNumber, m_forwardChannel, this);
    }

    /**
     * Constructor.
     *
     * @param forwardChannel The forward channel on the module to control.
     * @param reverseChannel The reverse channel on the module to control.
     */
    public DoubleSolenoid(final int forwardChannel, final int reverseChannel) {
        super(getDefaultSolenoidModule());
        m_forwardChannel = forwardChannel;
        m_reverseChannel = reverseChannel;
        initSolenoid();
    }

    /**
     * Constructor.
     *
     * @param moduleNumber The module number of the solenoid module to use.
     * @param forwardChannel The forward channel on the module to control.
     * @param reverseChannel The reverse channel on the module to control.
     */
    public DoubleSolenoid(final int moduleNumber, final int forwardChannel, final int reverseChannel) {
        super(moduleNumber);
        m_forwardChannel = forwardChannel;
        m_reverseChannel = reverseChannel;
        initSolenoid();
    }

    /**
     * Destructor.
     */
    public synchronized void free() {
        m_allocated.free((m_moduleNumber - 1) * kSolenoidChannels + m_forwardChannel - 1);
        m_allocated.free((m_moduleNumber - 1) * kSolenoidChannels + m_reverseChannel - 1);
    }

    /**
     * Set the value of a solenoid.
     *
     * @param value Move the solenoid to forward, reverse, or don't move it.
     */
    public void set(final Value value) {
        byte rawValue = 0;

        switch (value.value) {
            case Value.kOff_val:
                rawValue = 0x00;
                break;
            case Value.kForward_val:
                rawValue = m_forwardMask;
                break;
            case Value.kReverse_val:
                rawValue = m_reverseMask;
                break;
        }

        set(rawValue, m_forwardMask | m_reverseMask);
    }

    /**
     * Read the current value of the solenoid.
     *
     * @return The current value of the solenoid.
     */
    public Value get() {
        byte value = getAll();

        if ((value & m_forwardMask) != 0) return Value.kForward;
        if ((value & m_reverseMask) != 0) return Value.kReverse;
        return Value.kOff;
    }

    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Double Solenoid";
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
			//TODO: this is bad
            m_table.putString("Value", (get() == Value.kForward ? "Forward" : (get() == Value.kReverse ? "Reverse" : "Off")));
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {
        set(Value.kOff); // Stop for safety
        m_table_listener = new ITableListener() {
            public void valueChanged(ITable itable, String key, Object value, boolean bln) {
				//TODO: this is bad also
                if (value.toString().equals("Reverse"))
					set(Value.kReverse);
				else if (value.toString().equals("Forward"))
					set(Value.kForward);
				else
					set(Value.kOff);
            }
        };
        m_table.addTableListener("Value", m_table_listener, true);
    }
    
    /**
     * {@inheritDoc}
     */
    public void stopLiveWindowMode() {
        set(Value.kOff); // Stop for safety
        // TODO: Broken, should only remove the listener from "Value" only.
        m_table.removeTableListener(m_table_listener);
    }
}

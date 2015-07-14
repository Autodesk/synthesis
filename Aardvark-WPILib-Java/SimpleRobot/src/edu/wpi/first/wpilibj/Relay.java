/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.fpga.tDIO;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;
import edu.wpi.first.wpilibj.livewindow.LiveWindowSendable;
import edu.wpi.first.wpilibj.parsing.IDeviceController;
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.tables.ITableListener;
import edu.wpi.first.wpilibj.util.AllocationException;
import edu.wpi.first.wpilibj.util.CheckedAllocationException;

/**
 * Class for VEX Robotics Spike style relay outputs.
 * Relays are intended to be connected to Spikes or similar relays. The relay channels controls
 * a pair of pins that are either both off, one on, the other on, or both on. This translates into
 * two Spike outputs at 0v, one at 12v and one at 0v, one at 0v and the other at 12v, or two
 * Spike outputs at 12V. This allows off, full forward, or full reverse control of motors without
 * variable speed.  It also allows the two channels (forward and reverse) to be used independently
 * for something that does not care about voltage polarity (like a solenoid).
 */
public class Relay extends SensorBase implements IDeviceController, LiveWindowSendable {

    /**
     * This class represents errors in trying to set relay values contradictory
     * to the direction to which the relay is set.
     */
    public class InvalidValueException extends RuntimeException {

        /**
         * Create a new exception with the given message
         * @param message the message to pass with the exception
         */
        public InvalidValueException(String message) {
            super(message);
        }
    }

    /**
     * The state to drive a Relay to.
     */
    public static class Value {

        /**
         * The integer value representing this enumeration
         */
        public final int value;
        static final int kOff_val = 0;
        static final int kOn_val = 1;
        static final int kForward_val = 2;
        static final int kReverse_val = 3;
        /**
         * value: off
         */
        public static final Value kOff = new Value(kOff_val);
        /**
         * value: on for relays with defined direction
         */
        public static final Value kOn = new Value(kOn_val);
        /**
         * value: forward
         */
        public static final Value kForward = new Value(kForward_val);
        /**
         * value: reverse
         */
        public static final Value kReverse = new Value(kReverse_val);

        private Value(int value) {
            this.value = value;
        }
    }

    /**
     * The Direction(s) that a relay is configured to operate in.
     */
    public static class Direction {

        /**
         * The integer value representing this enumeration
         */
        public final int value;
        static final int kBoth_val = 0;
        static final int kForward_val = 1;
        static final int kReverse_val = 2;
        /**
         * direction: both directions are valid
         */
        public static final Direction kBoth = new Direction(kBoth_val);
        /**
         * direction: Only forward is valid
         */
        public static final Direction kForward = new Direction(kForward_val);
        /**
         * direction: only reverse is valid
         */
        public static final Direction kReverse = new Direction(kReverse_val);

        private Direction(int value) {
            this.value = value;
        }
    }
    private int m_channel;
    private Direction m_direction;
    private DigitalModule m_module;
    private static Resource relayChannels = new Resource(tDIO.kNumSystems * kRelayChannels * 2);

    /**
     * Common relay initialization method.
     * This code is common to all Relay constructors and initializes the relay and reserves
     * all resources that need to be locked. Initially the relay is set to both lines at 0v.
     * @param moduleNumber The number of the digital module to use.
     */
    private void initRelay(final int moduleNumber) {
        SensorBase.checkRelayModule(moduleNumber);
        SensorBase.checkRelayChannel(m_channel);
        try {
            if (m_direction == Direction.kBoth || m_direction == Direction.kForward) {
                relayChannels.allocate(((moduleNumber - 1) * kRelayChannels + m_channel - 1) * 2);
                UsageReporting.report(UsageReporting.kResourceType_Relay, m_channel, moduleNumber-1);
            }
            if (m_direction == Direction.kBoth || m_direction == Direction.kReverse) {
                relayChannels.allocate(((moduleNumber - 1) * kRelayChannels + m_channel - 1) * 2 + 1);
                UsageReporting.report(UsageReporting.kResourceType_Relay, m_channel+128, moduleNumber-1);
            }
        } catch (CheckedAllocationException e) {
            throw new AllocationException("Relay channel " + m_channel + " on module " + moduleNumber + " is already allocated");
        }
        m_module = DigitalModule.getInstance(moduleNumber);
        m_module.setRelayForward(m_channel, false);
        m_module.setRelayReverse(m_channel, false);
        LiveWindow.addActuator("Relay", moduleNumber, m_channel, this);
    }


    /**
     * Relay constructor given the module and the channel.
     * @param moduleNumber The number of the digital module to use.
     * @param channel The channel number within the module for this relay.
     * @param direction The direction that the Relay object will control.
     */
    public Relay(final int moduleNumber, final int channel, Direction direction) {
        if (direction == null)
            throw new NullPointerException("Null Direction was given");
        m_channel = channel;
        m_direction = direction;
        initRelay(moduleNumber);
    }

    /**
     * Relay constructor given a channel only where the default digital module is used.
     * @param channel The channel number within the default module for this relay.
     * @param direction The direction that the Relay object will control.
     */
    public Relay(final int channel, Direction direction) {
        if (direction == null)
            throw new NullPointerException("Null Direction was given");
        m_channel = channel;
        m_direction = direction;
        initRelay(getDefaultDigitalModule());
    }

    /**
     * Relay constructor given the module and the channel, allowing both directions.
     * @param moduleNumber The number of the digital module to use.
     * @param channel The channel number within the module for this relay.
     */
    public Relay(final int moduleNumber, final int channel) {
        m_channel = channel;
        m_direction = Direction.kBoth;
        initRelay(moduleNumber);
    }

    /**
     * Relay constructor given a channel only where the default digital module is used,
     * allowing both directions.
     * @param channel The channel number within the default module for this relay.
     */
    public Relay(final int channel) {
        m_channel = channel;
        m_direction = Direction.kBoth;
        initRelay(getDefaultDigitalModule());
    }

    public void free() {
        m_module.setRelayForward(m_channel, false);
        m_module.setRelayReverse(m_channel, false);

        if (m_direction == Direction.kBoth || m_direction == Direction.kForward) {
            relayChannels.free(((m_module.getModuleNumber() - 1) * kRelayChannels + m_channel - 1) * 2);
        }
        if (m_direction == Direction.kBoth || m_direction == Direction.kReverse) {
            relayChannels.free(((m_module.getModuleNumber() - 1) * kRelayChannels + m_channel - 1) * 2 + 1);
        }
    }

    /**
     * Set the relay state.
     *
     * Valid values depend on which directions of the relay are controlled by the object.
     *
     * When set to kBothDirections, the relay can be set to any of the four states:
	 *		0v-0v, 12v-0v, 0v-12v, 12v-12v
     *
     * When set to kForwardOnly or kReverseOnly, you can specify the constant for the
     *    direction or you can simply specify kOff_val and kOn_val.  Using only kOff_val and kOn_val is
     *    recommended.
     *
     * @param value The state to set the relay.
     */
    public void set(Value value) {
        switch (value.value) {
            case Value.kOff_val:
                if (m_direction == Direction.kBoth || m_direction == Direction.kForward) {
                    m_module.setRelayForward(m_channel, false);
                }
                if (m_direction == Direction.kBoth || m_direction == Direction.kReverse) {
                    m_module.setRelayReverse(m_channel, false);
                }
                break;
            case Value.kOn_val:
                if (m_direction == Direction.kBoth || m_direction == Direction.kForward) {
                    m_module.setRelayForward(m_channel, true);
                } 
				if (m_direction == Direction.kBoth || m_direction == Direction.kReverse) {
                    m_module.setRelayReverse(m_channel, true);
                }
                break;
            case Value.kForward_val:
                if (m_direction == Direction.kReverse)
                    throw new InvalidValueException("A relay configured for reverse cannot be set to forward");
                if (m_direction == Direction.kBoth || m_direction == Direction.kForward) {
                    m_module.setRelayForward(m_channel, true);
                }
                if (m_direction == Direction.kBoth) {
                    m_module.setRelayReverse(m_channel, false);
                }
                break;
            case Value.kReverse_val:
                if (m_direction == Direction.kForward)
                    throw new InvalidValueException("A relay configured for forward cannot be set to reverse");
                if (m_direction == Direction.kBoth) {
                    m_module.setRelayForward(m_channel, false);
                }
                if (m_direction == Direction.kBoth || m_direction == Direction.kReverse) {
                    m_module.setRelayReverse(m_channel, true);
                }
                break;
            default:
            //Cannot hit this, limited by Value enum
        }
    }
    
    /**
     * Get the Relay State
     * 
     * Gets the current state of the relay.
     * 
     * When set to kForwardOnly or kReverseOnly, value is returned as kOn/kOff not
     * kForward/kReverse (per the recommendation in Set)
     * 
     * @return The current state of the relay as a Relay::Value
     */
    public Value get() {
       if(m_module.getRelayForward(m_channel)) {
           if(m_module.getRelayReverse(m_channel)) {
               return Value.kOn;
           } else {
               if(m_direction == Direction.kForward) {
                   return Value.kOn;
               } else {
               return Value.kForward;
               }
           }
       } else {
           if(m_module.getRelayReverse(m_channel)) {
               if(m_direction == Direction.kForward) {
                   return Value.kOn;
               } else {
                   return Value.kReverse;
               }
           } else {
               return Value.kOff;
           }
       }        
    }

    /**
     * Set the Relay Direction
     *
     * Changes which values the relay can be set to depending on which direction is
     * used
     *
     * Valid inputs are kBothDirections, kForwardOnly, and kReverseOnly
     *
     *@param direction The direction for the relay to operate in
     */
    public void setDirection(Direction direction) {
        if (direction == null)
            throw new NullPointerException("Null Direction was given");
        if (m_direction == direction) {
            return;
        }

        free();

        m_direction = direction;

        initRelay(m_module.getModuleNumber());
    }
    
    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Relay";
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
        if(m_table != null){
            if (get() == Value.kOn) {
                m_table.putString("Value", "On");
            } else if (get() == Value.kForward) {
                m_table.putString("Value", "Forward");
            } else if (get() == Value.kReverse) {
                m_table.putString("Value", "Reverse");
            } else {
                m_table.putString("Value", "Off");
            }
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {
        m_table_listener = new ITableListener() {
            public void valueChanged(ITable itable, String key, Object value, boolean bln) {
                String val = ((String) value);
                if (val.equals("Off")) {
                    set(Value.kOff);
                }
                else if (val.equals("Forward")) {
                    set(Value.kForward);
                }
                else if (val.equals("Reverse")) {
                    set(Value.kReverse);
                }
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

/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.fpga.tAnalogTrigger;
import edu.wpi.first.wpilibj.parsing.IInputOutput;
import edu.wpi.first.wpilibj.util.AllocationException;
import edu.wpi.first.wpilibj.util.BoundaryException;
import edu.wpi.first.wpilibj.util.CheckedAllocationException;

/**
 * Class for creating and configuring Analog Triggers
 * @author dtjones
 */
public class AnalogTrigger implements IInputOutput{

    /**
     * Exceptions dealing with improper operation of the Analog trigger
     */
    public class AnalogTriggerException extends RuntimeException {

        /**
         * Create a new exception with the given message
         * @param message the message to pass with the exception
         */
        public AnalogTriggerException(String message) {
            super(message);
        }
        
    }

    private static Resource triggers = new Resource(tAnalogTrigger.kNumSystems);
    /**
     * Where the analog trigger is attached
     */
    protected int m_index;
    private tAnalogTrigger m_trigger;
    private AnalogModule m_analogModule;
    private int m_channel;

    /**
     * Initialize an analog trigger from a module number and channel.
     * This is the common code for the two constructors that use a module number and channel.
     * @param moduleNumber The number of the analog module to create this trigger on.
     * @param channel the port to use for the analog trigger
     */
    protected void initTrigger(final int moduleNumber, final int channel) {
        m_channel = channel;
        m_analogModule = AnalogModule.getInstance(moduleNumber);
        try {
            m_index = triggers.allocate();
        } catch (CheckedAllocationException e) {
            throw new AllocationException("No analog triggers are available to allocate");
        }
        m_trigger = new tAnalogTrigger((byte) m_index);
        m_trigger.writeSourceSelect_Channel((byte) (m_channel - 1));
        m_trigger.writeSourceSelect_Module((byte) moduleNumber - 1);
        UsageReporting.report(UsageReporting.kResourceType_AnalogTrigger, channel, moduleNumber-1);
    }

    /**
     * Constructor for an analog trigger given a channel number.
     * The default module is used in this case.
     * @param channel the port to use for the analog trigger
     */
    public AnalogTrigger(final int channel) {
        initTrigger(AnalogModule.getDefaultAnalogModule(), channel);
    }

    /**
     * Constructor for an analog trigger given both the module number and channel.
     * @param moduleNumber The number of the analog module to create this trigger on.
     * @param channel the port to use for the analog trigger
     */
    public AnalogTrigger(final int moduleNumber, final int channel) {
        initTrigger(moduleNumber, channel);
    }

    /**
     * Construct an analog trigger given an analog channel.
     * This should be used in the case of sharing an analog channel between the trigger
     * and an analog input object.
     * @param channel the AnalogChannel to use for the analog trigger
     */
    public AnalogTrigger(AnalogChannel channel) {
        initTrigger(channel.getModuleNumber(), channel.getChannel());
    }

    /**
     * Release the resources used by this object
     */
    public void free() {
        triggers.free(m_index);
        m_trigger.Release();
        m_trigger = null;
    }

    /**
     * Set the upper and lower limits of the analog trigger.
     * The limits are given in ADC codes.  If oversampling is used, the units must be scaled
     * appropriately.
     * @param lower the lower raw limit
     * @param upper the upper raw limit
     */
    public void setLimitsRaw(final int lower, final int upper) {
        if (lower > upper) {
            throw new BoundaryException("Lower bound is greater than upper");
        }
        m_trigger.writeLowerLimit(lower);
        m_trigger.writeUpperLimit(upper);
    }

    /**
     * Set the upper and lower limits of the analog trigger.
     * The limits are given as floating point voltage values.
     * @param lower the lower voltage limit
     * @param upper the upper voltage limit
     */
    public void setLimitsVoltage(double lower, double upper) {
        if (lower > upper) {
            throw new BoundaryException("Lower bound is greater than upper bound");
        }
        // TODO: This depends on the averaged setting.  Only raw values will work as is.
        m_trigger.writeLowerLimit(m_analogModule.voltsToValue(m_channel, lower));
        m_trigger.writeUpperLimit(m_analogModule.voltsToValue(m_channel, upper));
    }

    /**
     * Configure the analog trigger to use the averaged vs. raw values.
     * If the value is true, then the averaged value is selected for the analog trigger, otherwise
     * the immediate value is used.
     * @param useAveragedValue true to use an averaged value, false otherwise
     */
    public void setAveraged(boolean useAveragedValue) {
        if (m_trigger.readSourceSelect_Filter() && useAveragedValue) {
            throw new AnalogTriggerException("Cannot set to Averaged if the analog trigger is filtered");
        }
        m_trigger.writeSourceSelect_Averaged(useAveragedValue);
    }

    /**
     * Configure the analog trigger to use a filtered value.
     * The analog trigger will operate with a 3 point average rejection filter. This is designed to
     * help with 360 degree pot applications for the period where the pot crosses through zero.
     * @param useFilteredValue true to use a filterd value, false otherwise
     */
    public void setFiltered(boolean useFilteredValue) {
        if (m_trigger.readSourceSelect_Averaged() && useFilteredValue) {
            throw new AnalogTriggerException("Cannot set to Filtered if the analog trigger is averaged");
        }
        m_trigger.writeSourceSelect_Filter(useFilteredValue);
    }

    /**
     * Return the index of the analog trigger.
     * This is the FPGA index of this analog trigger instance.
     * @return The index of the analog trigger.
     */
    public int getIndex() {
        return m_index;
    }

    /**
     * Return the InWindow output of the analog trigger.
     * True if the analog input is between the upper and lower limits.
     * @return The InWindow output of the analog trigger.
     */
    public boolean getInWindow() {
        return tAnalogTrigger.readOutput_InHysteresis((byte) m_index);
    }

    /**
     * Return the TriggerState output of the analog trigger.
     * True if above upper limit.
     * False if below lower limit.
     * If in Hysteresis, maintain previous state.
     * @return The TriggerState output of the analog trigger.
     */
    public boolean getTriggerState() {
        return tAnalogTrigger.readOutput_OverLimit((byte) m_index);
    }

    /**
     * Creates an AnalogTriggerOutput object.
     * Gets an output object that can be used for routing.
     * Caller is responsible for deleting the AnalogTriggerOutput object.
     * @param type An enum of the type of output object to create.
     * @return A pointer to a new AnalogTriggerOutput object.
     */
    AnalogTriggerOutput createOutput(AnalogTriggerOutput.Type type) {
        return new AnalogTriggerOutput(this, type);
    }
}

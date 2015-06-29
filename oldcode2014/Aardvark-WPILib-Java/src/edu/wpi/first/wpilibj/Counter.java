/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.fpga.tCounter;
import edu.wpi.first.wpilibj.livewindow.LiveWindowSendable;
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.util.AllocationException;
import edu.wpi.first.wpilibj.util.BoundaryException;
import edu.wpi.first.wpilibj.util.CheckedAllocationException;

/**
 * Class for counting the number of ticks on a digital input channel.
 * This is a general purpose class for counting repetitive events. It can return the number
 * of counts, the period of the most recent cycle, and detect when the signal being counted
 * has stopped by supplying a maximum cycle time.
 */
public class Counter extends SensorBase implements CounterBase, LiveWindowSendable, PIDSource {

    /**
     * Mode determines how and what the counter counts
     */
    public static class Mode {

        /**
         * The integer value representing this enumeration
         */
        public final int value;
        static final int kTwoPulse_val = 0;
        static final int kSemiperiod_val = 1;
        static final int kPulseLength_val = 2;
        static final int kExternalDirection_val = 3;
        /**
         * mode: two pulse
         */
        public static final Mode kTwoPulse = new Mode(kTwoPulse_val);
        /**
         * mode: semi period
         */
        public static final Mode kSemiperiod = new Mode(kSemiperiod_val);
        /**
         * mode: pulse length
         */
        public static final Mode kPulseLength = new Mode(kPulseLength_val);
        /**
         * mode: external direction
         */
        public static final Mode kExternalDirection = new Mode(kExternalDirection_val);

        private Mode(int value) {
            this.value = value;
        }
    }
    private DigitalSource m_upSource;		///< What makes the counter count up.
    private DigitalSource m_downSource;	///< What makes the counter count down.
    private boolean m_allocatedUpSource;
    private boolean m_allocatedDownSource;
    private tCounter m_counter;				///< The FPGA counter object.
    private int m_index;					///< The index of this counter.
    private static Resource counters = new Resource(tCounter.kNumSystems);
    private PIDSourceParameter m_pidSource;
    private double m_distancePerPulse;		// distance of travel for each tick

    private void initCounter(final Mode mode) {
        m_allocatedUpSource = false;
        m_allocatedDownSource = false;

        try {
            m_index = counters.allocate();
        } catch (CheckedAllocationException e) {
            throw new AllocationException("No counters left to be allocated");
        }

        m_counter = new tCounter(m_index);
        m_counter.writeConfig_Mode(mode.value);
        m_upSource = null;
        m_downSource = null;
        m_counter.writeTimerConfig_AverageSize(1);

       UsageReporting.report(UsageReporting.kResourceType_Counter, m_index, mode.value);
    }

    /**
     * Create an instance of a counter where no sources are selected.
     * Then they all must be selected by calling functions to specify the upsource and the downsource
     * independently.
     */
    public Counter() {
        initCounter(Mode.kTwoPulse);
    }

    /**
     * Create an instance of a counter from a Digital Input.
     * This is used if an existing digital input is to be shared by multiple other objects such
     * as encoders.
     * @param source the digital source to count
     */
    public Counter(DigitalSource source) {
        if (source == null)
            throw new NullPointerException("Source given was null");
        initCounter(Mode.kTwoPulse);
        setUpSource(source);
    }

    /**
     * Create an instance of a Counter object.
     * Create an up-Counter instance given a channel. The default digital module is assumed.
     * @param channel the digital input channel to count
     */
    public Counter(int channel) {
        initCounter(Mode.kTwoPulse);
        setUpSource(channel);
    }

    /**
     * Create an instance of a Counter object.
     * Create an instance of an up-Counter given a digital module and a channel.
     * @param slot The cRIO chassis slot for the digital module used
     * @param channel The channel in the digital module
     */
    public Counter(int slot, int channel) {
        initCounter(Mode.kTwoPulse);
        setUpSource(slot, channel);
    }

    /**
     * Create an instance of a Counter object.
     * Create an instance of a simple up-Counter given an analog trigger.
     * Use the trigger state output from the analog trigger.
     * @param encodingType which edges to count
     * @param upSource first source to count
     * @param downSource second source for direction
     * @param inverted true to invert the count
     */
    public Counter(EncodingType encodingType, DigitalSource upSource, DigitalSource downSource, boolean inverted) {
        initCounter(Mode.kExternalDirection);
        if (encodingType != EncodingType.k1X && encodingType != EncodingType.k2X) {
            throw new RuntimeException("Counters only support 1X and 2X quadreature decoding!");
        }
        if (upSource == null)
            throw new NullPointerException("Up Source given was null");
        setUpSource(upSource);
        if (downSource == null)
            throw new NullPointerException("Down Source given was null");
        setDownSource(downSource);

        if (encodingType == null)
            throw new NullPointerException("Encoding type given was null");

        if (encodingType == EncodingType.k1X) {
            setUpSourceEdge(true, false);
        } else {
            setUpSourceEdge(true, true);
        }

        setDownSourceEdge(inverted, true);
    }

    /**
     * Create an instance of a Counter object.
     * Create an instance of a simple up-Counter given an analog trigger.
     * Use the trigger state output from the analog trigger.
     * @param trigger the analog trigger to count
     */
    public Counter(AnalogTrigger trigger) {
        initCounter(Mode.kTwoPulse);
        setUpSource(trigger.createOutput(AnalogTriggerOutput.Type.kTypeState));
    }

    public void free() {
        setUpdateWhenEmpty(true);

        clearUpSource();
        clearDownSource();
        m_counter.Release();

        m_upSource = null;
        m_downSource = null;
        m_counter = null;
        counters.free(m_index);
    }

    /**
     * Set the up source for the counter as digital input channel and slot.
     * @param slot the location of the digital module to use
     * @param channel the digital port to count
     */
    public void setUpSource(int slot, int channel) {
        setUpSource(new DigitalInput(slot, channel));
        m_allocatedUpSource = true;
    }

    /**
     * Set the upsource for the counter as a digital input channel.
     * The slot will be the default digital module slot.
     * @param channel the digital port to count
     */
    public void setUpSource(int channel) {
        setUpSource(new DigitalInput(channel));
        m_allocatedUpSource = true;
    }

    /**
     * Set the source object that causes the counter to count up.
     * Set the up counting DigitalSource.
     * @param source the digital source to count
     */
    public void setUpSource(DigitalSource source) {
        if (m_upSource != null && m_allocatedUpSource) {
            m_upSource.free();
            m_allocatedUpSource = false;
        }
        m_upSource = source;
        m_counter.writeConfig_UpSource_Module(source.getModuleForRouting());
        m_counter.writeConfig_UpSource_Channel(source.getChannelForRouting());
        m_counter.writeConfig_UpSource_AnalogTrigger(source.getAnalogTriggerForRouting());

        if (m_counter.readConfig_Mode() == Mode.kTwoPulse.value ||
                m_counter.readConfig_Mode() == Mode.kExternalDirection.value) {
            setUpSourceEdge(true, false);
        }
        m_counter.strobeReset();
    }

    /**
     * Set the up counting source to be an analog trigger.
     * @param analogTrigger The analog trigger object that is used for the Up Source
     * @param triggerType The analog trigger output that will trigger the counter.
     */
    public void setUpSource(AnalogTrigger analogTrigger, AnalogTriggerOutput.Type triggerType) {
        setUpSource(analogTrigger.createOutput(triggerType));
        m_allocatedUpSource = true;
    }

    /**
     * Set the edge sensitivity on an up counting source.
     * Set the up source to either detect rising edges or falling edges.
     * @param risingEdge true to count rising edge
     * @param fallingEdge true to count falling edge
     */
    public void setUpSourceEdge(boolean risingEdge, boolean fallingEdge) {
        if (m_upSource == null) throw new RuntimeException(
                "Up Source must be set before setting the edge!");
        m_counter.writeConfig_UpRisingEdge(risingEdge);
        m_counter.writeConfig_UpFallingEdge(fallingEdge);
    }

    /**
     * Disable the up counting source to the counter.
     */
    public void clearUpSource() {
        if (m_upSource != null && m_allocatedUpSource) {
            m_upSource.free();
            m_allocatedUpSource = false;
        }
        m_upSource = null;

        boolean state = m_counter.readConfig_Enable();
        m_counter.writeConfig_Enable(false);
        m_counter.writeConfig_UpFallingEdge(false);
        m_counter.writeConfig_UpRisingEdge(false);
        // Index 0 of digital is always 0.
        m_counter.writeConfig_UpSource_Channel(0);
        m_counter.writeConfig_UpSource_AnalogTrigger(false);
        m_counter.writeConfig_Enable(state);
    }

    /**
     * Set the down counting source to be a digital input channel.
     * The slot will be set to the default digital module slot.
     * @param channel the digital port to count
     */
    public void setDownSource(int channel) {
        setDownSource(new DigitalInput(channel));
        m_allocatedDownSource = true;
    }

    /**
     * Set the down counting source to be a digital input slot and channel.
     * @param slot the location of the digital module to use
     * @param channel the digital port to count
     */
    public void setDownSource(int slot, int channel) {
        setDownSource(new DigitalInput(slot, channel));
        m_allocatedDownSource = true;
    }

    /**
     * Set the source object that causes the counter to count down.
     * Set the down counting DigitalSource.
     * @param source the digital source to count
     */
    public void setDownSource(DigitalSource source) {
        if (m_downSource != null && m_allocatedDownSource) {
            m_downSource.free();
            m_allocatedDownSource = false;
        }
        int mode = m_counter.readConfig_Mode();
        if(mode != Mode.kTwoPulse_val && mode != Mode.kExternalDirection_val) {
            throw new RuntimeException(
                    "Down Source only supported in TwoPulse and ExternalDirection modes!");
        }
        m_downSource = source;
        m_counter.writeConfig_DownSource_Module(source.getModuleForRouting());
        m_counter.writeConfig_DownSource_Channel(source.getChannelForRouting());
        m_counter.writeConfig_DownSource_AnalogTrigger(source.getAnalogTriggerForRouting());

        setDownSourceEdge(true, false);
        m_counter.strobeReset();
    }

    /**
     * Set the down counting source to be an analog trigger.
     * @param analogTrigger The analog trigger object that is used for the Down Source
     * @param triggerType The analog trigger output that will trigger the counter.
     */
    public void setDownSource(AnalogTrigger analogTrigger, AnalogTriggerOutput.Type triggerType) {
        setDownSource(analogTrigger.createOutput(triggerType));
        m_allocatedDownSource = true;
    }

    /**
     * Set the edge sensitivity on a down counting source.
     * Set the down source to either detect rising edges or falling edges.
     * @param risingEdge true to count the rising edge
     * @param fallingEdge true to count the falling edge
     */
    public void setDownSourceEdge(boolean risingEdge, boolean fallingEdge) {
        if (m_downSource == null) throw new RuntimeException(
                " Down Source must be set before setting the edge!");
        m_counter.writeConfig_DownRisingEdge(risingEdge);
        m_counter.writeConfig_DownFallingEdge(fallingEdge);
    }

    /**
     * Disable the down counting source to the counter.
     */
    public void clearDownSource() {
        if (m_downSource != null && m_allocatedDownSource) {
            m_downSource.free();
            m_allocatedDownSource = false;
        }
        m_downSource = null;

        boolean state = m_counter.readConfig_Enable();
        m_counter.writeConfig_Enable(false);
        m_counter.writeConfig_DownFallingEdge(false);
        m_counter.writeConfig_DownRisingEdge(false);
        // Index 0 of digital is always 0.
        m_counter.writeConfig_DownSource_Channel(0);
        m_counter.writeConfig_DownSource_AnalogTrigger(false);
        m_counter.writeConfig_Enable(state);
    }

    /**
     * Set standard up / down counting mode on this counter.
     * Up and down counts are sourced independently from two inputs.
     */
    public void setUpDownCounterMode() {
        m_counter.writeConfig_Mode(Mode.kTwoPulse.value);
    }

    /**
     * Set external direction mode on this counter.
     * Counts are sourced on the Up counter input.
     * The Down counter input represents the direction to count.
     */
    public void setExternalDirectionMode() {
        m_counter.writeConfig_Mode(Mode.kExternalDirection.value);
    }

    /**
     * Set Semi-period mode on this counter.
     * Counts up on both rising and falling edges.
     * @param highSemiPeriod true to count up on both rising and falling
     */
    public void setSemiPeriodMode(boolean highSemiPeriod) {
        m_counter.writeConfig_Mode(Mode.kSemiperiod.value);
        m_counter.writeConfig_UpRisingEdge(highSemiPeriod);
        setUpdateWhenEmpty(false);
    }

    /**
     * Configure the counter to count in up or down based on the length of the input pulse.
     * This mode is most useful for direction sensitive gear tooth sensors.
     * @param threshold The pulse length beyond which the counter counts the opposite direction.  Units are seconds.
     */
    public void setPulseLengthMode(double threshold) {
        m_counter.writeConfig_Mode(Mode.kPulseLength.value);
        m_counter.writeConfig_PulseLengthThreshold((short) ((threshold * 1.0e6) * kSystemClockTicksPerMicrosecond));
    }

    /**
     * Start the Counter counting.
     * This enables the counter and it starts accumulating counts from the associated
     * input channel. The counter value is not reset on starting, and still has the previous value.
     */
    public void start() {
        m_counter.writeConfig_Enable(true);
    }

    /**
     * Read the current counter value.
     * Read the value at this instant. It may still be running, so it reflects the current value. Next
     * time it is read, it might have a different value.
     */
    public int get() {
        return m_counter.readOutput_Value();
    }
    
    /**
     * Read the current scaled counter value.
     * Read the value at this instant, scaled by the distance per pulse (defaults to 1).
     * @return 
     */
    public double getDistance() {
        return m_counter.readOutput_Value() * m_distancePerPulse;
    }

    /**
     * Reset the Counter to zero.
     * Set the counter value to zero. This doesn't effect the running state of the counter, just sets
     * the current value to zero.
     */
    public void reset() {
        m_counter.strobeReset();
    }

    /**
     * Stop the Counter.
     * Stops the counting but doesn't effect the current value.
     */
    public void stop() {
        m_counter.writeConfig_Enable(false);
    }

    /**
     * Set the maximum period where the device is still considered "moving".
     * Sets the maximum period where the device is considered moving. This value is used to determine
     * the "stopped" state of the counter using the GetStopped method.
     * @param maxPeriod The maximum period where the counted device is considered moving in
     * seconds.
     */
    public void setMaxPeriod(double maxPeriod) {
        m_counter.writeTimerConfig_StallPeriod((int) (maxPeriod * 1.0e6));
    }

    /**
     * Select whether you want to continue updating the event timer output when there are no samples captured.
     * The output of the event timer has a buffer of periods that are averaged and posted to
     * a register on the FPGA.  When the timer detects that the event source has stopped
     * (based on the MaxPeriod) the buffer of samples to be averaged is emptied.  If you
     * enable the update when empty, you will be notified of the stopped source and the event
     * time will report 0 samples.  If you disable update when empty, the most recent average
     * will remain on the output until a new sample is acquired.  You will never see 0 samples
     * output (except when there have been no events since an FPGA reset) and you will likely not
     * see the stopped bit become true (since it is updated at the end of an average and there are
     * no samples to average).
     * @param enabled true to continue updating
     */
    public void setUpdateWhenEmpty(boolean enabled) {
        m_counter.writeTimerConfig_UpdateWhenEmpty(enabled);
    }

    /**
     * Determine if the clock is stopped.
     * Determine if the clocked input is stopped based on the MaxPeriod value set using the
     * SetMaxPeriod method. If the clock exceeds the MaxPeriod, then the device (and counter) are
     * assumed to be stopped and it returns true.
     * @return Returns true if the most recent counter period exceeds the MaxPeriod value set by
     * SetMaxPeriod.
     */
    public boolean getStopped() {
        return m_counter.readTimerOutput_Stalled();
    }

    /**
     * The last direction the counter value changed.
     * @return The last direction the counter value changed.
     */
    public boolean getDirection() {
        boolean value = m_counter.readOutput_Direction();
        return value;
    }

    /**
     * Set the Counter to return reversed sensing on the direction.
     * This allows counters to change the direction they are counting in the case of 1X and 2X
     * quadrature encoding only. Any other counter mode isn't supported.
     * @param reverseDirection true if the value counted should be negated.
     */
    public void setReverseDirection(boolean reverseDirection) {
        if (m_counter.readConfig_Mode() == Mode.kExternalDirection.value) {
            if (reverseDirection) {
                setDownSourceEdge(true, true);
            } else {
                setDownSourceEdge(false, true);
            }
        }
    }

    /**
     * Get the Period of the most recent count.
     * Returns the time interval of the most recent count. This can be used for velocity calculations
     * to determine shaft speed.
     * @returns The period of the last two pulses in units of seconds.
     */
    public double getPeriod() {
        double period;
        if (m_counter.readTimerOutput_Stalled()) {
            return Double.POSITIVE_INFINITY;
        } else {
            period = (double) m_counter.readTimerOutput_Period() / (double) m_counter.readTimerOutput_Count();
        }
        return period / 1.0e6;
    }
    
    /**
     * Get the current rate of the Counter.
     * Read the current rate of the counter accounting for the distance per pulse value. 
     * The default value for distance per pulse (1) yields units of pulses per second.
     * @return The rate in units/sec
     */
    public double getRate() {
        return m_distancePerPulse / getPeriod();
    }
    
    /**
     * Set the Samples to Average which specifies the number of samples of the timer to 
     * average when calculating the period. Perform averaging to account for 
     * mechanical imperfections or as oversampling to increase resolution.
     * @param samplesToAverage The number of samples to average from 1 to 127.
     */
    public void setSamplesToAverage (int samplesToAverage) {
       BoundaryException.assertWithinBounds(samplesToAverage, 1, 127);
       m_counter.writeTimerConfig_AverageSize(samplesToAverage);
    }
    
    /**
     * Get the Samples to Average which specifies the number of samples of the timer to 
     * average when calculating the period. Perform averaging to account for 
     * mechanical imperfections or as oversampling to increase resolution.
     * @return SamplesToAverage The number of samples being averaged (from 1 to 127)
     */
    public int getSamplesToAverage()
    {
        return m_counter.readTimerConfig_AverageSize();
    }
    
    /**
     * Set the distance per pulse for this counter.
     * This sets the multiplier used to determine the distance driven based on the count value
     * from the encoder. Set this value based on the Pulses per Revolution and factor in any 
     * gearing reductions. This distance can be in any units you like, linear or angular.
     *
     * @param distancePerPulse The scale factor that will be used to convert pulses to useful units.
     */
    public void setDistancePerPulse(double distancePerPulse) {
        m_distancePerPulse = distancePerPulse;
    }
    
    /**
     * Set which parameter of the encoder you are using as a process control variable.
     * The counter class supports the rate and distance parameters.
     * @param pidSource An enum to select the parameter.
     */
    public void setPIDSourceParameter(PIDSourceParameter pidSource) {
	BoundaryException.assertWithinBounds(pidSource.value, 0, 1);
        m_pidSource = pidSource;
    }
	
    public double pidGet() {
        switch (m_pidSource.value) {
        case PIDSourceParameter.kDistance_val:
            return getDistance();
        case PIDSourceParameter.kRate_val:
            return getRate();
        default:
            return 0.0;
        }
    }
	
    /**
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Counter";
    }
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
            m_table.putNumber("Value", m_counter.readOutput_Value());
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {}
    
    /**
     * {@inheritDoc}
     */
    public void stopLiveWindowMode() {}
}

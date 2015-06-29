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

/**
 * Class to represent a specific output from an analog trigger.
 * This class is used to get the current output value and also as a DigitalSource
 * to provide routing of an output to digital subsystems on the FPGA such as
 * Counter, Encoder, and Interrupt.
 *
 * The TriggerState output indicates the primary output value of the trigger.  If the analog
 * signal is less than the lower limit, the output is false.  If the analog value is greater
 * than the upper limit, then the output is true.  If the analog value is in between, then
 * the trigger output state maintains its most recent value.
 *
 * The InWindow output indicates whether or not the analog signal is inside the range defined
 * by the limits.
 *
 * The RisingPulse and FallingPulse outputs detect an instantaneous transition from above the
 * upper limit to below the lower limit, and vise versa.  These pulses represent a rollover
 * condition of a sensor and can be routed to an up / down couter or to interrupts.  Because
 * the outputs generate a pulse, they cannot be read directly.  To help ensure that a rollover
 * condition is not missed, there is an average rejection filter available that operates on the
 * upper 8 bits of a 12 bit number and selects the nearest outlyer of 3 samples.  This will reject
 * a sample that is (due to averaging or sampling) errantly between the two limits.  This filter
 * will fail if more than one sample in a row is errantly in between the two limits.  You may see
 * this problem if attempting to use this feature with a mechanical rollover sensor, such as a
 * 360 degree no-stop potentiometer without signal conditioning, because the rollover transition
 * is not sharp / clean enough.  Using the averaging engine may help with this, but rotational speeds of
 * the sensor will then be limited.
 */
public class AnalogTriggerOutput extends DigitalSource implements IInputOutput{

        /**
     * Exceptions dealing with improper operation of the Analog trigger output
     */
    public class AnalogTriggerOutputException extends RuntimeException {

        /**
         * Create a new exception with the given message
         * @param message the message to pass with the exception
         */
        public AnalogTriggerOutputException(String message) {
            super(message);
        }

    }

    /**
     * Type determines under what state the analog trigger evaluates to true or
     * false
     */
    public static class Type {

        /**
         * The integer value representing this enumeration
         */
        public final int value;
        static final int kInWindow_val = 0;
        static final int kTypeState_val = 1;
        static final int kRisingPulse_val = 2;
        static final int kFallingPulse_val = 3;
        /**
         * outputType: true if in the trigger window
         */
        public static final Type kInWindow = new Type(kInWindow_val);
        /**
         * outputType: true if above the upper limit, false if below the lower limit
         * maintain previous state otherwise
         */
        public static final Type kTypeState = new Type(kTypeState_val);
        /**
         * outputType: true on a rising edge
         */
        public static final Type kRisingPulse = new Type(kRisingPulse_val);
        /**
         * outputType: true on a falling edge
         */
        public static final Type kFallingPulse = new Type(kFallingPulse_val);

        private Type(int value) {
            this.value = value;
        }
    }
    private AnalogTrigger m_trigger;
    private Type m_outputType;

    /**
     * Create an object that represents one of the four outputs from an analog trigger.
     *
     * Because this class derives from DigitalSource, it can be passed into routing functions
     * for Counter, Encoder, etc.
     *
     * @param trigger The trigger for which this is an output.
     * @param outputType An enum that specifies the output on the trigger to represent.
     */
    public AnalogTriggerOutput(AnalogTrigger trigger, final Type outputType) {
        if (trigger ==  null)
            throw new NullPointerException("Analog Trigger given was null");
        m_trigger = trigger;
        m_outputType = outputType;

        UsageReporting.report(UsageReporting.kResourceType_AnalogTriggerOutput, trigger.getIndex(), outputType.value);
    }

    public void free() {
    }

    /**
     * Get the state of the analog trigger output.
     * @return The state of the analog trigger output.
     */
    public boolean get() {
        switch (m_outputType.value) {
            case Type.kInWindow_val:
                return tAnalogTrigger.readOutput_InHysteresis((byte) m_trigger.m_index);
            case Type.kTypeState_val:
                return tAnalogTrigger.readOutput_OverLimit((byte) m_trigger.m_index);
            case Type.kRisingPulse_val:
            case Type.kFallingPulse_val:
                throw new AnalogTriggerOutputException("Cannot get the state of the analog trigger output if it is configured for a rising or falling edge");
        }
        // Should never get here.
        return false;
    }

    public int getChannelForRouting() {
        return (m_trigger.m_index << 2) + m_outputType.value;
    }

    public int getModuleForRouting() {
        return m_trigger.m_index >> 2;
    }

    public boolean getAnalogTriggerForRouting() {
        return true;
    }

    /**
     * Request interrupts asynchronously on this digital input.
     * @param handler the interrupt service routine
     * @param param a parameter for the ISR
     */
//    public void requestInterrupts(/*tInterruptHandler*/Object handler, Object param) {
        //TODO: add interrupt support
        //TODO: throw exception
//    }

    /**
     * Request interrupts synchronously on this digital input.
     */
//    public void requestInterrupts() {
        //TODO: throw exception
//    }
}

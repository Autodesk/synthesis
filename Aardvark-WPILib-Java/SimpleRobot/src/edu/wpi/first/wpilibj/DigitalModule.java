/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.ModulePresence;
import edu.wpi.first.wpilibj.fpga.tDIO;
import edu.wpi.first.wpilibj.util.AllocationException;
import edu.wpi.first.wpilibj.util.CheckedAllocationException;

/**
 * Class representing a digital module
 * @author dtjones
 */
public class DigitalModule extends Module {

    /**
     * Expected loop timing
     */
    public static final int kExpectedLoopTiming = 260;
    private static final Resource DIOChannels = new Resource(tDIO.kNumSystems * SensorBase.kDigitalChannels);
    private static final Resource DO_PWMGenerators[] = new Resource[tDIO.kNumSystems];
    tDIO m_fpgaDIO;
    private final Object syncRoot = new Object();

    /**
     * Get an instance of an Digital Module.
     * Singleton digital module creation where a module is allocated on the first use
     * and the same module is returned on subsequent uses.
     *
     * @param moduleNumber The number of the digital module to access.
     * @return The digital module of the specified number.
     */
    public static synchronized DigitalModule getInstance(final int moduleNumber) {
        SensorBase.checkDigitalModule(moduleNumber);
        return (DigitalModule) getModule(ModulePresence.ModuleType.kDigital, moduleNumber);
    }

    /**
     * Convert a channel to its fpga reference
     * @param channel the channel to convert
     * @return the converted channel
     */
    public static int remapDigitalChannel(final int channel) {
        return 15 - channel;
    }

    /**
     * Convert a channel from it's fpga reference
     * @param channel the channel to convert
     * @return the converted channel
     */
    public static int unmapDigitalChannel(final int channel) {
        return 15 - channel;
    }

    /**
     * Create a new digital module
     * @param moduleNumber The number of the digital module to use (1 or 2)
     */
    protected DigitalModule(final int moduleNumber) {
        super(ModulePresence.ModuleType.kDigital, moduleNumber);
        
        DO_PWMGenerators[m_moduleNumber - 1] = new Resource(tDIO.kDO_PWMDutyCycle_NumElements);
        m_fpgaDIO = new tDIO(m_moduleNumber - 1);

        while (tDIO.readLoopTiming() == 0) {
            Timer.delay(.001);
        }

        if (tDIO.readLoopTiming() != kExpectedLoopTiming) {
            System.out.print("DIO LoopTiming: ");
            System.out.print(tDIO.readLoopTiming());
            System.out.print(", expecting: ");
            System.out.println(kExpectedLoopTiming);
        }
        
        //Calculate the length, in ms, of one DIO loop
        double loopTime = tDIO.readLoopTiming()/(kSystemClockTicksPerMicrosecond*1e3);
        
        tDIO.writePWMConfig_Period((short) (PWM.kDefaultPwmPeriod/loopTime + .5));

        //Calculate the minimum time for the PWM signal to be high by using the number of steps down from center
        tDIO.writePWMConfig_MinHigh((short) ((PWM.kDefaultPwmCenter-PWM.kDefaultPwmStepsDown*loopTime)/loopTime + .5));


        // Ensure that PWM output values are set to OFF
        for (int pwm_index = 1; pwm_index <= kPwmChannels; pwm_index++) {
            setPWM(pwm_index, PWM.kPwmDisabled);
            setPWMPeriodScale(pwm_index, PWM.PeriodMultiplier.k4X_val); // Set all to 4x by default.
        }

        // Turn off all relay outputs.
        m_fpgaDIO.writeSlowValue_RelayFwd(0);
        m_fpgaDIO.writeSlowValue_RelayRev(0);
    }

    /**
     * Set a PWM channel to the desired value. The values range from 0 to 255 and the period is controlled
     * by the PWM Period and MinHigh registers.
     *
     * @param channel The PWM channel to set.
     * @param value The PWM value to set.
     */
    public void setPWM(final int channel, final int value) {
        checkPWMChannel(channel);
        m_fpgaDIO.writePWMValue(channel - 1, value);
    }

    /**
     * Get a value from a PWM channel. The values range from 0 to 255.
     *
     * @param channel The PWM channel to read from.
     * @return The raw PWM value.
     */
    public int getPWM(final int channel) {
        checkPWMChannel(channel);
        return m_fpgaDIO.readPWMValue(channel - 1);
    }

    /**
     * Set how how often the PWM signal is squelched, thus scaling the period.
     *
     * @param channel The PWM channel to configure.
     * @param squelchMask The 2-bit mask of outputs to squelch.
     */
    public void setPWMPeriodScale(final int channel, final int squelchMask) {
        checkPWMChannel(channel);
        m_fpgaDIO.writePWMPeriodScale((byte) (channel - 1), squelchMask);
    }

    /**
     * Set the state of a relay.
     * Set the state of a relay output to be forward. Relays have two outputs and each is
     * independently set to 0v or 12v.
     *
     * @param channel The Relay channel.
     * @param on Indicates whether to set the relay to the On state.
     */
    public void setRelayForward(final int channel, final boolean on) {
        checkRelayChannel(channel);

        synchronized (syncRoot) {
            int forwardRelays = m_fpgaDIO.readSlowValue_RelayFwd();
            if (on) {
                forwardRelays |= 1 << (channel - 1);
            } else {
                forwardRelays &= ~(1 << (channel - 1));
            }
            m_fpgaDIO.writeSlowValue_RelayFwd(forwardRelays);
        }
    }

    /**
     * Set the state of a relay.
     * Set the state of a relay output to be reverse. Relays have two outputs and each is
     * independently set to 0v or 12v.
     *
     * @param channel The Relay channel.
     * @param on Indicates whether to set the relay to the On state.
     */
    public void setRelayReverse(final int channel, final boolean on) {
        SensorBase.checkRelayChannel(channel);

        synchronized (syncRoot) {
            int reverseRelays = m_fpgaDIO.readSlowValue_RelayRev();
            if (on) {
                reverseRelays |= 1 << (channel - 1);
            } else {
                reverseRelays &= ~(1 << (channel - 1));
            }
            m_fpgaDIO.writeSlowValue_RelayRev(reverseRelays);
        }
    }

    /**
     * Get the current state of the forward relay channel
     * @param channel the channel of the relay to get
     * @return The current state of the relay.
     */
    public boolean getRelayForward(int channel) {
        int forwardRelays = m_fpgaDIO.readSlowValue_RelayFwd();
        return (forwardRelays & (1 << (channel - 1))) != 0;
    }

    /**
     * Get the current state of all of the forward relay channels on this module.
     * @return The state of all forward relay channels as a byte.
     */
    public byte getRelayForward() {
        return (byte) m_fpgaDIO.readSlowValue_RelayFwd();
    }

    /**
     * Get the current state of the reverse relay channel
     * @param channel the channel of the relay to get
     * @return The current statte of the relay
     */
    public boolean getRelayReverse(int channel) {
        int reverseRelays = m_fpgaDIO.readSlowValue_RelayRev();
        return (reverseRelays & (1 << (channel - 1))) != 0;
    }

    /**
     * Get the current state of all of the reverse relay channels on this module.
     * @return The state of all forward relay channels as a byte.
     */
    public byte getRelayReverse() {
        return (byte) m_fpgaDIO.readSlowValue_RelayRev();
    }

    /**
     * Allocate Digital I/O channels.
     * Allocate channels so that they are not accidently reused. Also the direction is set at the
     * time of the allocation.
     *
     * @param channel The channel to allocate.
     * @param input Indicates whether the I/O pin is an input (true) or an output (false).
     * @return True if the I/O pin was allocated, false otherwise.
     */
    public boolean allocateDIO(final int channel, final boolean input) {
        try {
            DIOChannels.allocate((kDigitalChannels * (m_moduleNumber - 1) + channel - 1));
        } catch (CheckedAllocationException e) {
            throw new AllocationException(
                    "Digital channel " + channel + " on module " + m_moduleNumber + " is already allocated");
        }
        final int outputEnable = m_fpgaDIO.readOutputEnable();
        final int bitToSet = 1 << (DigitalModule.remapDigitalChannel((channel - 1)));
        short outputEnableValue;

        if (input) {
            outputEnableValue = (short) (outputEnable & (~bitToSet));
        } else {
            outputEnableValue = (short) (outputEnable | bitToSet);
        }

        m_fpgaDIO.writeOutputEnable(outputEnableValue);
        return true;
    }

    /**
     * Free the resource associated with a digital I/O channel.
     *
     * @param channel The channel whose resources should be freed.
     */
    public void freeDIO(final int channel) {
        DIOChannels.free((kDigitalChannels * (m_moduleNumber - 1) + channel - 1));
    }

    /**
     * Write a digital I/O bit to the FPGA.
     * Set a single value on a digital I/O channel.
     *
     * @param channel The channel to set.
     * @param value The value to set.
     */
    public void setDIO(final int channel, final boolean value) {
        int currentDIO = m_fpgaDIO.readDO();
        if (!value) {
            currentDIO = (currentDIO & ~(1 << DigitalModule.remapDigitalChannel(channel - 1)));
        } else {
            currentDIO = (currentDIO | (1 << DigitalModule.remapDigitalChannel(channel - 1)));
        }
        m_fpgaDIO.writeDO(currentDIO);
    }

    /**
     * Read a digital I/O bit from the FPGA.
     * Get a single value from a digital I/O channel.
     *
     * @param channel The channel to read
     * @return The value of the selected channel
     */
    public boolean getDIO(final int channel) {
        final int currentDIO = m_fpgaDIO.readDI();

        // Shift 00000001 over channel-1 places.
        // AND it against the currentDIO
        // if it == 0, then return 0
        // else return 1
        return ((currentDIO >> remapDigitalChannel(channel - 1)) & 1) == 1;
    }

    /**
     * Read the state of all the Digital I/O lines from the FPGA
     * These are not remapped to logical order.  They are still in hardware order.
     * @return The state of all the Digital IO lines in hardware order
     */
    public short getAllDIO() {
        return (short) m_fpgaDIO.readDI();
    }

    /**
     * Read the direction of a digital I/O line
     * @param channel The channel of the DIO to get the direction of.
     * @return True if the digital channel is configured as an output, false if it is an input
     */
    public boolean getDIODirection(int channel) {
        int currentOutputEnable = m_fpgaDIO.readOutputEnable();

        //Shift 00000001 over channel-1 places.
        //AND it against the currentOutputEnable
        //if it == 0, then return false
        //else return true
        return ((currentOutputEnable >> remapDigitalChannel(channel - 1)) & 1) != 0;
    }

    /**
     * Read the direction of all the Digital I/O lines from the FPGA
     * A 1 bit means output and a 0 bit means input.
     * These are not remapped to logical order.  They are still in hardware order.
     * @return The direction of all the Digital IO lines in hardware order
     */
    public short getDIODirection() {
        return (short) m_fpgaDIO.readOutputEnable();
    }

    /**
     * Generate a single pulse.
     * Write a pulse to the specified digital output channel. There can only be a single pulse going at any time.
     *
     * @param channel The channel to pulse.
     * @param pulseLength The length of the pulse.
     */
    public void pulse(final int channel, final int pulseLength) {
        final short mask = (short) (1 << remapDigitalChannel(channel - 1));
        m_fpgaDIO.writePulseLength(pulseLength);
        m_fpgaDIO.writePulse(mask);
    }

    /**
     * Check a DIO line to see if it is currently generating a pulse.
     *
     * @param channel The channel to check.
     * @return True if the channel is pulsing, false otherwise.
     */
    public boolean isPulsing(final int channel) {
        final int mask = 1 << remapDigitalChannel(channel - 1);
        final int pulseRegister = m_fpgaDIO.readPulse();
        return (pulseRegister & mask) != 0;
    }

    /**
     * Check if any DIO line is currently generating a pulse.
     *
     * @return True if any channel is pulsing, false otherwise.
     */
    public boolean isPulsing() {
        final int pulseRegister = m_fpgaDIO.readPulse();
        return pulseRegister != 0;
    }

    /**
     * Allocate a DO PWM Generator.
     * Allocate PWM generators so that they are not accidently reused.
     */
    public int allocateDO_PWM() {
        try {
            return DO_PWMGenerators[m_moduleNumber - 1].allocate();
        } catch (CheckedAllocationException e) {
            throw new AllocationException(
                "No Digital Output PWM Generators on module " + m_moduleNumber + " remaining");
        }
    }

    /**
     * Free the resource associated with a DO PWM generator.
     */
    public void freeDO_PWM(int pwmGenerator) {
	if (pwmGenerator == ~0) return;
        DO_PWMGenerators[m_moduleNumber - 1].free(pwmGenerator);
    }

    /**
     * Change the frequency of the DO PWM generator.
     *
     * The valid range is from 0.6 Hz to 19 kHz.  The frequency resolution is logarithmic.
     *
     * @param rate The frequency to output all digital output PWM signals on this module.
     */
    public void setDO_PWMRate(double rate) {
        // Currently rounding in the log rate domain... heavy weight toward picking a higher freq.
	// TODO: Round in the linear rate domain.
	byte pwmPeriodPower = (byte)(Math.log(1.0 / (m_fpgaDIO.readLoopTiming() * 0.25E-6 * rate)) / Math.log(2.0) + 0.5);
        m_fpgaDIO.writeDO_PWMConfig_PeriodPower(pwmPeriodPower);
    }

    /**
     * Configure which DO channel the PWM siganl is output on
     * @param pwmGenerator The generator index reserved by allocateDO_PWM()
     * @param channel The Digital Output channel to output on
     */
    public void setDO_PWMOutputChannel(int pwmGenerator, int channel) {
	if (pwmGenerator == ~0) return;
        switch (pwmGenerator) {
            case 0:
                m_fpgaDIO.writeDO_PWMConfig_OutputSelect_0(remapDigitalChannel(channel - 1));
                break;
            case 1:
                m_fpgaDIO.writeDO_PWMConfig_OutputSelect_1(remapDigitalChannel(channel - 1));
                break;
            case 2:
                m_fpgaDIO.writeDO_PWMConfig_OutputSelect_2(remapDigitalChannel(channel - 1));
                break;
            case 3:
                m_fpgaDIO.writeDO_PWMConfig_OutputSelect_3(remapDigitalChannel(channel - 1));
                break;
        }
    }

    /**
     * Configure the duty-cycle of the PWM generator
     * @param pwmGenerator The generator index reserved by allocateDO_PWM()
     * @param dutyCycle The percent duty cycle to output [0..1].
     */
    public void setDO_PWMDutyCycle(int pwmGenerator, double dutyCycle) {
	if (pwmGenerator == ~0) return;
        if (dutyCycle > 1.0) {
            dutyCycle = 1.0;
        }
        if (dutyCycle < 0.0) {
            dutyCycle = 0.0;
        }
        double rawDutyCycle = 256.0 * dutyCycle;
        if (rawDutyCycle > 255.5) {
            rawDutyCycle = 255.5;
        }
        byte pwmPeriodPower = m_fpgaDIO.readDO_PWMConfig_PeriodPower();
        if (pwmPeriodPower < 4) {
            // The resolution of the duty cycle drops close to the highest frequencies.
            rawDutyCycle = rawDutyCycle / Math.pow(2.0, 4 - pwmPeriodPower);
        }
        m_fpgaDIO.writeDO_PWMDutyCycle(pwmGenerator, (byte)rawDutyCycle);
    }

    /**
     * Return an I2C object for this digital module
     *
     * @param address The device address.
     * @return The associated I2C object.
     */
    public I2C getI2C(final int address) {
        return new I2C(this, address);
    }
    
    /**
     * Get the loop timing of the Digital Module
     *
     * @return The number of clock ticks per DIO loop
     */
    public int getLoopTiming() {
        return tDIO.readLoopTiming();
    }
}

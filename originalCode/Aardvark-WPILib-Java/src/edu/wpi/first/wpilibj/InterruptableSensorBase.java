/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.fpga.tInterrupt;
import edu.wpi.first.wpilibj.fpga.tInterruptManager;

/**
 * Base for sensors to be used with interrupts
 */
public abstract class InterruptableSensorBase extends SensorBase {

    /**
     * The interrupt resource
     */
    protected tInterrupt m_interrupt;
    /**
     * The interrupt manager resource
     */
    protected tInterruptManager m_manager;
    /**
     * The index of the interrupt
     */
    protected int m_interruptIndex;
    /**
     * Resource manager
     */
    protected static Resource interrupts = new Resource(8);

    /**
     * Create a new InterrupatableSensorBase
     */
    public InterruptableSensorBase() {
        m_manager = null;
        m_interrupt = null;
    }

    /**
     * Allocate the interrupt
     * @param watcher
     */
    public void allocateInterrupts(boolean watcher) {
        if (!watcher) {
            throw new IllegalArgumentException("Interrupt callbacks not yet supported");
        }
        // Expects the calling leaf class to allocate an interrupt index.
        m_interrupt = new tInterrupt((byte) m_interruptIndex);
        m_interrupt.writeConfig_WaitForAck(false);
        m_manager = new tInterruptManager(1 << m_interruptIndex, watcher);
    }

    /**
     * Cancel interrupts on this device.
     * This deallocates all the chipobject structures and disables any interrupts.
     */
    public void cancelInterrupts() {
        if (m_interrupt == null || m_manager == null) {
            throw new IllegalStateException();
        }
        m_interrupt.Release();
        m_interrupt = null;
        m_manager.Release();
        m_manager = null;
    }

    /**
     * In synchronous mode, wait for the defined interrupt to occur.
     * @param timeout Timeout in seconds
     */
    public void waitForInterrupt(double timeout) {
        m_manager.watch((int) (timeout * 1e3));
    }

    /**
     * Enable interrupts to occur on this input.
     * Interrupts are disabled when the RequestInterrupt call is made. This gives time to do the
     * setup of the other options before starting to field interrupts.
     */
    public void enableInterrupts() {
        throw new IllegalArgumentException("Interrupt callbacks not yet supported");
    }

    /**
     * Disable Interrupts without without deallocating structures.
     */
    public void disableInterrupts() {
        throw new IllegalArgumentException("Interrupt callbacks not yet supported");
    }

    /**
     * Return the timestamp for the interrupt that occurred most recently.
     * This is in the same time domain as getClock().
     * @return Timestamp in seconds since boot.
     */
    public double readInterruptTimestamp() {
        return m_interrupt.readTimeStamp() * 1e-6;
    }
}

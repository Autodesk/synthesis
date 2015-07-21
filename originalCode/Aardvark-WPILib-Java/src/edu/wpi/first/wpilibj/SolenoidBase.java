/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.fpga.tSolenoid;
import edu.wpi.first.wpilibj.parsing.IDeviceController;

/**
 * SolenoidBase class is the common base class for the Solenoid and
 * DoubleSolenoid classes.
 */
public abstract class SolenoidBase extends SensorBase implements IDeviceController {

    protected int m_moduleNumber; ///< The number of the solenoid module being used.
    protected static Resource m_allocated = new Resource(tSolenoid.kDO7_0_NumElements * kSolenoidChannels);

    private static tSolenoid m_fpgaSolenoidModule; ///< FPGA Solenoid Module object.
    private static int m_refCount; ///< Reference count for the chip object.

    /**
     * Constructor.
     *
     * @param moduleNumber The number of the solenoid module to use.
     */
    public SolenoidBase(final int moduleNumber) {
        m_moduleNumber = moduleNumber;
        checkSolenoidModule(m_moduleNumber);

        m_refCount++;
        if (m_refCount == 1) {
            m_fpgaSolenoidModule = new tSolenoid();
        }
    }

    /**
     * Destructor.
     */
    public synchronized void free() {
        if (m_refCount == 1) {
            m_fpgaSolenoidModule.Release();
            m_fpgaSolenoidModule = null;
        }
        m_refCount--;
    }

    /**
     * Set the value of a solenoid.
     *
     * @param value The value you want to set on the module.
     * @param mask The channels you want to be affected.
     */
    protected synchronized void set(int value, int mask) {
        byte currentValue = (byte)tSolenoid.readDO7_0(m_moduleNumber - 1);
        // Zero out the values to change
        currentValue = (byte)(currentValue & ~mask);
        currentValue = (byte)(currentValue | (value & mask));
        tSolenoid.writeDO7_0(m_moduleNumber - 1, currentValue);
    }

    /**
     * Read all 8 solenoids from the module used by this solenoid as a single byte
     *
     * @return The current value of all 8 solenoids on this module.
     */
    public byte getAll() {
        return (byte)tSolenoid.readDO7_0(m_moduleNumber - 1);
    }

    /**
     * Read all 8 solenoids in the default solenoid module as a single byte
     *
     * @return The current value of all 8 solenoids on the default module.
     */
    public static byte getAllFromDefaultModule() {
        return getAllFromModule(getDefaultSolenoidModule());
    }

    /**
     * Read all 8 solenoids in the specified solenoid module as a single byte
     *
     * @return The current value of all 8 solenoids on the specified module.
     */
    public static byte getAllFromModule(int moduleNumber) {
        checkSolenoidModule(moduleNumber);
        return (byte)tSolenoid.readDO7_0(moduleNumber - 1);
    }
}

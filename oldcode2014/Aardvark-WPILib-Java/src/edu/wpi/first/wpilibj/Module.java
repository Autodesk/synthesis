/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.ModulePresence;

/**
 * Base class for AnalogModule and DigitalModule.
 * 
 * @author dtjones
 */
public class Module extends SensorBase {

    /**
     * An array holding the object representing each module
     */
    protected static Module[] m_modules = new Module[ModulePresence.ModuleType.kSolenoid.getValue() * ModulePresence.kMaxModuleNumber + (ModulePresence.kMaxModuleNumber - 1)];
    /**
     * The module number of the module
     */
    protected int m_moduleNumber;
    protected ModulePresence.ModuleType m_moduleType;

    /**
     * Constructor.
     *
     * @param moduleNumber The number of this module (1 or 2).
     */
    protected Module(ModulePresence.ModuleType moduleType, final int moduleNumber) {
        m_modules[toIndex(moduleType, moduleNumber)] = this;
        m_moduleNumber = moduleNumber;
    }

    /**
     * Gets the module number associated with a module.
     *
     * @return The module's number.
     */
    public int getModuleNumber() {
        return m_moduleNumber;
    }

    /**
     * Gets the module type associated with a module.
     *
     * @return The module's type.
     */
    public ModulePresence.ModuleType getModuleType() {
        return m_moduleType;
    }

    /**
     * Static module singleton factory.
     * 
     * @param moduleType The type of the module represented.
     * @param moduleNumber The module index within the module type.
     * @return the module
     */
    public static Module getModule(ModulePresence.ModuleType moduleType, int moduleNumber) {
        if(m_modules[toIndex(moduleType, moduleNumber)] == null) {
            if(moduleType.equals(ModulePresence.ModuleType.kAnalog)) {
                new AnalogModule(moduleNumber);
            } else if (moduleType.equals(ModulePresence.ModuleType.kDigital)) {
                new DigitalModule(moduleNumber);
  /*        } else if (moduleType.equals(ModulePresence.ModuleType.kSolenoid)) {
                new Sol
   */      } else {
                throw new RuntimeException("A module of type "+moduleType+" with module index "+moduleNumber);
            }
        }
        return m_modules[toIndex(moduleType, moduleNumber)];
    }

    /**
     * Create an index into m_modules based on type and number
     *
     * @param moduleType The type of the module represented.
     * @param moduleNumber The module index within the module type.
     * @return The index into m_modules.
     */
    private static int toIndex(ModulePresence.ModuleType moduleType, int moduleNumber) {
        if(moduleNumber == 0 || moduleNumber > ModulePresence.kMaxModuleNumber)
            return 0;
        return moduleType.getValue() * ModulePresence.kMaxModuleNumber + (moduleNumber - 1);
    }
}

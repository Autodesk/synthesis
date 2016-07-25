/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

/**
 * DigitalSource Interface.
 * The DigitalSource represents all the possible inputs for a counter or a quadrature encoder. The source may be
 * either a digital input or an analog input. If the caller just provides a channel, then a digital input will be
 * constructed and freed when finished for the source. The source can either be a digital input or analog trigger
 * but not both.
 */
public abstract class DigitalSource extends InterruptableSensorBase {

    /**
     * Get the channel routing number
     * @return channel routing number
     */
    public abstract int getChannelForRouting();

    /**
     * Get the module routing number
     * @return module routing number
     */
    public abstract int getModuleForRouting();

    /**
     * Is this an analog trigger
     * @return true if this is an analog trigger
     */
    public abstract boolean getAnalogTriggerForRouting();
}

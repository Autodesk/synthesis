/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;
import edu.wpi.first.wpilibj.parsing.IDeviceController;

/**
 * CTRE Talon Speed Controller
 */
public class Talon extends SafePWM implements SpeedController, IDeviceController {

    /**
     * Common initialization code called by all constructors.
     *
     * Note that the Talon uses the following bounds for PWM values. These values should work reasonably well for
     * most controllers, but if users experience issues such as asymmetric behavior around
     * the deadband or inability to saturate the controller in either direction, calibration is recommended.
     * The calibration procedure can be found in the Talon User Manual available from CTRE.
     * 
     *   - 2.037ms = full "forward"
     *   - 1.539ms = the "high end" of the deadband range
     *   - 1.513ms = center of the deadband range (off)
     *   - 1.487ms = the "low end" of the deadband range
     *   - .989ms = full "reverse"
     */
    private void initTalon() {
        setBounds(2.037, 1.539, 1.513, 1.487, .989);
        setPeriodMultiplier(PeriodMultiplier.k2X);
        setRaw(m_centerPwm);

        LiveWindow.addActuator("Talon", getModuleNumber(), getChannel(), this);
        UsageReporting.report(UsageReporting.kResourceType_Talon, getChannel(), getModuleNumber()-1);
    }

    /**
     * Constructor that assumes the default digital module.
     *
     * @param channel The PWM channel on the digital module that the Victor is attached to.
     */
    public Talon(final int channel) {
        super(channel);
        initTalon();
    }

    /**
     * Constructor that specifies the digital module.
     *
     * @param slot The slot in the chassis that the digital module is plugged into.
     * @param channel The PWM channel on the digital module that the Victor is attached to.
     */
    public Talon(final int slot, final int channel) {
        super(slot, channel);
        initTalon();
    }

    /**
     * Set the PWM value.
     *
     * @deprecated For compatibility with CANJaguar
     *
     * The PWM value is set using a range of -1.0 to 1.0, appropriately
     * scaling the value for the FPGA.
     *
     * @param speed The speed to set.  Value should be between -1.0 and 1.0.
     * @param syncGroup The update group to add this Set() to, pending UpdateSyncGroup().  If 0, update immediately.
     */
    public void set(double speed, byte syncGroup) {
        setSpeed(speed);
		Feed();
    }

    /**
     * Set the PWM value.
     *
     * The PWM value is set using a range of -1.0 to 1.0, appropriately
     * scaling the value for the FPGA.
     *
     * @param speed The speed value between -1.0 and 1.0 to set.
     */
    public void set(double speed) {
        setSpeed(speed);
		Feed();
    }

    /**
     * Get the recently set value of the PWM.
     *
     * @return The most recently set value for the PWM between -1.0 and 1.0.
     */
    public double get() {
        return getSpeed();
    }

    /**
     * Write out the PID value as seen in the PIDOutput base object.
     *
     * @param output Write out the PWM value as was found in the PIDController
     */
    public void pidWrite(double output) {
        set(output);
    }
}

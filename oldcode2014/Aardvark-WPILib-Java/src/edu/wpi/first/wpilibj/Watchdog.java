/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.fpga.tWatchdog;
import edu.wpi.first.wpilibj.parsing.IUtility;

/**
 * Watchdog timer class.
 * The watchdog timer is designed to keep the robots safe. The idea is that the robot program must
 * constantly "feed" the watchdog otherwise it will shut down all the motor outputs. That way if a
 * program breaks, rather than having the robot continue to operate at the last known speed, the
 * motors will be shut down.
 *
 * This is serious business.  Don't just disable the watchdog.  You can't afford it!
 *
 * http://thedailywtf.com/Articles/_0x2f__0x2f_TODO_0x3a__Uncomment_Later.aspx
 */
public class Watchdog extends SensorBase implements IUtility{

    private static Watchdog m_instance;
    private tWatchdog m_fpgaWatchdog;
    /**
     * Default expiration for the watchdog in seconds
     */
    public static final double kDefaultWatchdogExpiration = .5;

    /**
     * The Watchdog is born.
     */
    protected Watchdog() {
        m_fpgaWatchdog = new tWatchdog();
        setExpiration(Watchdog.kDefaultWatchdogExpiration);
        setEnabled(true);
    }

    /**
     *  Get an instance of the watchdog
     * @return an instance of the watchdog
     */
    public static synchronized Watchdog getInstance() {
        if (m_instance == null) {
            m_instance = new Watchdog();
        }
        return m_instance;
    }

    /**
     * Throw the dog a bone.
     *
     * When everything is going well, you feed your dog when you get home.
     * Let's hope you don't drive your car off a bridge on the way home...
     * Your dog won't get fed and he will starve to death.
     *
     * By the way, it's not cool to ask the neighbor (some random task) to
     * feed your dog for you.  He's your responsibility!
     */
    public void feed() {
        tWatchdog.strobeFeed();
    }

    /**
     * Put the watchdog out of its misery.
     *
     * Don't wait for your dying robot to starve when there is a problem.
     * Kill it quickly, cleanly, and humanely.
     */
    public void kill() {
        tWatchdog.strobeKill();
    }

    /**
     * Read how long it has been since the watchdog was last fed.
     *
     * @return The number of seconds since last meal.
     */
    public double getTimer() {
        long timer = tWatchdog.readTimer();
        return timer / (kSystemClockTicksPerMicrosecond * 1e6);
    }

    /**
     * Read what the current expiration is.
     *
     * @return The number of seconds before starvation following a meal (watchdog starves if it doesn't eat this often).
     */
    public double getExpiration() {
        long expiration = tWatchdog.readExpiration();
        return (double)expiration / (kSystemClockTicksPerMicrosecond * 1e6);
    }

    /**
     * Configure how many seconds your watchdog can be neglected before it starves to death.
     *
     * @param expiration The number of seconds before starvation following a meal (watchdog starves if it doesn't eat this often).
     */
    public void setExpiration(double expiration) {
        tWatchdog.writeExpiration((int) (expiration * (kSystemClockTicksPerMicrosecond * 1e6)));
    }

    /**
     * Find out if the watchdog is currently enabled or disabled (mortal or immortal).
     *
     * @return Enabled or disabled.
     */
    public boolean getEnabled() {
        return !tWatchdog.readImmortal();
    }

    /**
     * Enable or disable the watchdog timer.
     *
     * When enabled, you must keep feeding the watchdog timer to
     * keep the watchdog active, and hence the dangerous parts
     * (motor outputs, etc.) can keep functioning.
     * When disabled, the watchdog is immortal and will remain active
     * even without being fed.  It will also ignore any kill commands
     * while disabled.
     *
     * @param enabled Enable or disable the watchdog.
     */
    public void setEnabled(final boolean enabled) {
        tWatchdog.writeImmortal(!enabled);
    }

    /**
     * Check in on the watchdog and make sure he's still kicking.
     *
     * This indicates that your watchdog is allowing the system to operate.
     * It is still possible that the network communications is not allowing the
     * system to run, but you can check this to make sure it's not your fault.
     * Check isSystemActive() for overall system status.
     *
     * If the watchdog is disabled, then your watchdog is immortal.
     *
     * @return Is the watchdog still alive?
     */
    public boolean isAlive() {
        return tWatchdog.readStatus_Alive();
    }

    /**
     * Check on the overall status of the system.
     *
     * @return Is the system active (i.e. PWM motor outputs, etc. enabled)?
     */
    public boolean isSystemActive() {
        return tWatchdog.readStatus_SystemActive();
    }
}

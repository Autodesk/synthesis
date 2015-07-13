/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;
import edu.wpi.first.wpilibj.parsing.IDevice;
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.tables.ITableListener;

/**
 * Standard hobby style servo.
 *
 * The range parameters default to the appropriate values for the Hitec HS-322HD servo provided
 * in the FIRST Kit of Parts in 2008.
 */
public class Servo extends PWM implements IDevice {

    private static final double kMaxServoAngle = 170.0;
    private static final double kMinServoAngle = 0.0;

    /**
     * Common initialization code called by all constructors.
     *
     * InitServo() assigns defaults for the period multiplier for the servo PWM control signal, as
     * well as the minimum and maximum PWM values supported by the servo.
     */
    private void initServo() {
        setBounds(2.27, 0, 0, 0, .743);
        setPeriodMultiplier(PeriodMultiplier.k4X);

        LiveWindow.addActuator("Servo", getModuleNumber(), getChannel(), this);
        UsageReporting.report(UsageReporting.kResourceType_Servo, getChannel(), getModuleNumber()-1);
    }

    /**
     * Constructor that assumes the default digital module.
     *
     * @param channel The PWM channel on the digital module to which the servo is attached.
     */
    public Servo(final int channel) {
        super(channel);
        initServo();
    }

    /**
     * Constructor that specifies the digital module.
     *
     * @param slot The slot in the chassis that the digital module is plugged into.
     * @param channel The PWM channel on the digital module to which the servo is attached.
     */
    public Servo(final int slot, final int channel) {
        super(slot, channel);
        initServo();
    }

    /**
     * Set the servo position.
     *
     * Servo values range from 0.0 to 1.0 corresponding to the range of full left to full right.
     *
     * @param value Position from 0.0 to 1.0.
     */
    public void set(double value) {
        setPosition(value);
    }

    /**
     * Get the servo position.
     *
     * Servo values range from 0.0 to 1.0 corresponding to the range of full left to full right.
     *
     * @return Position from 0.0 to 1.0.
     */
    public double get() {
        return getPosition();
    }

    /**
     * Set the servo angle.
     *
     * Assume that the servo angle is linear with respect to the PWM value (big assumption, need to test).
     *
     * Servo angles that are out of the supported range of the servo simply "saturate" in that direction
     * In other words, if the servo has a range of (X degrees to Y degrees) than angles of less than X
     * result in an angle of X being set and angles of more than Y degrees result in an angle of Y being set.
     *
     * @param degrees The angle in degrees to set the servo.
     */
    public void setAngle(double degrees) {
        if (degrees < kMinServoAngle) {
            degrees = kMinServoAngle;
        } else if (degrees > kMaxServoAngle) {
            degrees = kMaxServoAngle;
        }

        setPosition(((degrees - kMinServoAngle)) / getServoAngleRange());
    }

    /**
     * Get the servo angle.
     *
     * Assume that the servo angle is linear with respect to the PWM value (big assumption, need to test).
     * @return The angle in degrees to which the servo is set.
     */
    public double getAngle() {
        return getPosition() * getServoAngleRange() + kMinServoAngle;
    }

    private double getServoAngleRange() {
        return kMaxServoAngle - kMinServoAngle;
    }
    
    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Servo";
    }
    private ITable m_table;
    private ITableListener m_table_listener;
    
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
    public void updateTable() {
        if (m_table != null) {
            m_table.putNumber("Value", get());
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void startLiveWindowMode() {
        m_table_listener = new ITableListener() {
            public void valueChanged(ITable itable, String key, Object value, boolean bln) {
                set(((Double) value).doubleValue());
            }
        };
        m_table.addTableListener("Value", m_table_listener, true);
    }
    
    /**
     * {@inheritDoc}
     */
    public void stopLiveWindowMode() {
        // TODO: Broken, should only remove the listener from "Value" only.
        m_table.removeTableListener(m_table_listener);
    }
}

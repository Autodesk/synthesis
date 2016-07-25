/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;
import edu.wpi.first.wpilibj.livewindow.LiveWindowSendable;
import edu.wpi.first.wpilibj.parsing.ISensor;
import edu.wpi.first.wpilibj.tables.ITable;

/**
 * HiTechnic NXT Compass.
 *
 * This class alows access to a HiTechnic NXT Compass on an I2C bus.
 * These sensors to not allow changing addresses so you cannot have more
 *   than one on a single bus.
 *
 * Details on the sensor can be found here:
 *   http://www.hitechnic.com/index.html?lang=en-us&target=d17.html
 *
 */
public class HiTechnicCompass extends SensorBase implements ISensor, LiveWindowSendable, PIDSource {

    /**
     * An exception dealing with connecting to and communicating with the
     * HiTechnicCompass
     */
    public class CompassException extends RuntimeException {

        /**
         * Create a new exception with the given message
         * @param message the message to pass with the exception
         */
        public CompassException(String message) {
            super(message);
        }
       
    }

    private static final byte kAddress = 0x02;
    private static final byte kManufacturerBaseRegister = 0x08;
    private static final byte kManufacturerSize = 0x08;
    private static final byte kSensorTypeBaseRegister = 0x10;
    private static final byte kSensorTypeSize = 0x08;
    private static final byte kHeadingRegister = 0x44;
    private I2C m_i2c;

    /**
     * Constructor.
     *
     * @param slot The slot of the digital module that the sensor is plugged into.
     */
    public HiTechnicCompass(int slot) {
        DigitalModule module = DigitalModule.getInstance(slot);
        m_i2c = module.getI2C(kAddress);

        // Verify Sensor
        final byte[] kExpectedManufacturer = "HiTechnc".getBytes();
        final byte[] kExpectedSensorType = "Compass ".getBytes();
        if (!m_i2c.verifySensor(kManufacturerBaseRegister, kManufacturerSize, kExpectedManufacturer)) {
            throw new CompassException("Invalid Compass Manufacturer");
        }
        if (!m_i2c.verifySensor(kSensorTypeBaseRegister, kSensorTypeSize, kExpectedSensorType)) {
            throw new CompassException("Invalid Sensor type");
        }
        
        UsageReporting.report(UsageReporting.kResourceType_HiTechnicCompass, module.getModuleNumber()-1);
        LiveWindow.addSensor("HiTechnicCompass", slot, 0, this);
    }

    /**
     * Destructor.
     */
    public void free() {
        if (m_i2c != null) {
            m_i2c.free();
        }
        m_i2c = null;
    }

    /**
     * Get the compass angle in degrees.
     *
     * The resolution of this reading is 1 degree.
     *
     * @return Angle of the compass in degrees.
     */
    public double getAngle() {
        byte[] heading = new byte[2];
        m_i2c.read(kHeadingRegister, (byte) heading.length, heading);

        return ((int) heading[0] + (int) heading[1] * (int) (1 << 8));
    }
    
    public double pidGet() {
        return getAngle();
    }

    /*
     * Live Window code, only does anything if live window is activated.
     */
    public String getSmartDashboardType(){
        return "Compass";
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
            m_table.putNumber("Value", getAngle());
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

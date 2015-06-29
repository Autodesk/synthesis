/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.util.BoundaryException;

/**
 * I2C bus interface class.
 *
 * This class is intended to be used by sensor (and other I2C device) drivers.
 * It probably should not be used directly.
 *
 * It is constructed by calling DigitalModule::GetI2C() on a DigitalModule object.
 */
public class I2C extends SensorBase {

    private DigitalModule m_module;
    private int m_deviceAddress;
    private boolean m_compatibilityMode;

    /**
     * Constructor.
     *
     * @param module The Digital Module to which the device is connected.
     * @param deviceAddress The address of the device on the I2C bus.
     */
    public I2C(DigitalModule module, int deviceAddress) {
        if (module == null) {
            throw new NullPointerException("Digital Module given was null");
        }
        m_module = module;
        m_deviceAddress = deviceAddress;
        m_compatibilityMode = true;

        UsageReporting.report(UsageReporting.kResourceType_I2C, deviceAddress, module.getModuleNumber()-1);
    }

    /**
     * Destructor.
     */
    public void free() {
    }

    /**
     * Generic transaction.
     *
     * This is a lower-level interface to the I2C hardware giving you more control over each transaction.
     *
     * @param dataToSend Buffer of data to send as part of the transaction.
     * @param sendSize Number of bytes to send as part of the transaction. [0..6]
     * @param dataReceived Buffer to read data into.
     * @param receiveSize Number of bytes to read from the device. [0..7]
     * @return Transfer Aborted... false for success, true for aborted.
     */
    public synchronized boolean transaction(byte[] dataToSend, int sendSize, byte[] dataReceived, int receiveSize) {
        BoundaryException.assertWithinBounds(sendSize, 0, 6);
        BoundaryException.assertWithinBounds(receiveSize, 0, 7);

        long data = 0;
        long dataHigh = 0;
        int i;
        for (i = 0; i < sendSize && i < 4; i++) {
            data |= ((long) dataToSend[i] & 0x000000FF) << (8 * i);
        }
        for (; i < sendSize; i++) {
            dataHigh |= ((long) dataToSend[i] & 0x000000FF) << (8 * (i - 4));
        }

        boolean aborted = true;

        m_module.m_fpgaDIO.writeI2CConfig_Address(m_deviceAddress);
        m_module.m_fpgaDIO.writeI2CConfig_BytesToWrite(sendSize);
        m_module.m_fpgaDIO.writeI2CConfig_BytesToRead(receiveSize);
        if (sendSize > 0) {
            m_module.m_fpgaDIO.writeI2CDataToSend(data);
        }
        if (sendSize > 4) {
            m_module.m_fpgaDIO.writeI2CConfig_DataToSendHigh((int) dataHigh);
        }
	m_module.m_fpgaDIO.writeI2CConfig_BitwiseHandshake(m_compatibilityMode);
        byte transaction = m_module.m_fpgaDIO.readI2CStatus_Transaction();
        m_module.m_fpgaDIO.strobeI2CStart();
        while (transaction == m_module.m_fpgaDIO.readI2CStatus_Transaction()) {
            Timer.delay(.001);
        }
        while (!m_module.m_fpgaDIO.readI2CStatus_Done()) {
            Timer.delay(.001);
        }
        aborted = m_module.m_fpgaDIO.readI2CStatus_Aborted();
        if (receiveSize > 0) {
            data = m_module.m_fpgaDIO.readI2CDataReceived();
        }
        if (receiveSize > 4) {
            dataHigh = m_module.m_fpgaDIO.readI2CStatus_DataReceivedHigh();
        }


        for (i = 0; i < receiveSize && i < 4; i++) {
            dataReceived[i] = (byte) ((data >> (8 * i)) & 0xFF);
        }
        for (; i < receiveSize; i++) {
            dataReceived[i] = (byte) ((dataHigh >> (8 * (i - 4))) & 0xFF);
        }
        return aborted;
    }

    /**
     * Attempt to address a device on the I2C bus.
     *
     * This allows you to figure out if there is a device on the I2C bus that
     * responds to the address specified in the constructor.
     *
     * @return Transfer Aborted... false for success, true for aborted.
     */
    public boolean addressOnly() {
        return transaction(null, (byte) 0, null, (byte) 0);
    }

    /**
     * Execute a write transaction with the device.
     *
     * Write a single byte to a register on a device and wait until the
     *   transaction is complete.
     *
     * @param registerAddress The address of the register on the device to be written.
     * @param data The byte to write to the register on the device.
     */
    public synchronized boolean write(int registerAddress, int data) {
        byte[] buffer = new byte[2];
        buffer[0] = (byte) registerAddress;
        buffer[1] = (byte) data;
        return transaction(buffer, buffer.length, null, 0);
    }

    /**
     * Execute a read transaction with the device.
     *
     * Read 1 to 7 bytes from a device.
     * Most I2C devices will auto-increment the register pointer internally
     *   allowing you to read up to 7 consecutive registers on a device in a
     *   single transaction.
     *
     * @param registerAddress The register to read first in the transaction.
     * @param count The number of bytes to read in the transaction. [1..7]
     * @param buffer A pointer to the array of bytes to store the data read from the device.
     * @return Transfer Aborted... false for success, true for aborted.
     */
    public boolean read(int registerAddress, int count, byte[] buffer) {
        BoundaryException.assertWithinBounds(count, 1, 7);
        if (buffer == null) {
            throw new NullPointerException("Null return buffer was given");
        }
        byte[] registerAddressArray = new byte[1];
        registerAddressArray[0] = (byte) registerAddress;

        return transaction(registerAddressArray, registerAddressArray.length, buffer, count);
    }

    /**
     * Send a broadcast write to all devices on the I2C bus.
     *
     * This is not currently implemented!
     *
     * @param registerAddress The register to write on all devices on the bus.
     * @param data The value to write to the devices.
     */
    public void broadcast(int registerAddress, int data) {
    }

    /**
     * SetCompatabilityMode
     *
     * Enables bitwise clock skewing detection.  This will reduce the I2C interface speed,
     * but will allow you to communicate with devices that skew the clock at abnormal times.
     * Compatability mode is enabled by default.
     *
     * @param enable Enable compatability mode for this sensor or not.
     */
    public void setCompatabilityMode(boolean enable) {
            m_compatibilityMode = enable;
            UsageReporting.report(UsageReporting.kResourceType_I2C, m_deviceAddress, m_module.getModuleNumber()-1, "C");
    }

    /**
     * Verify that a device's registers contain expected values.
     *
     * Most devices will have a set of registers that contain a known value that
     *   can be used to identify them.  This allows an I2C device driver to easily
     *   verify that the device contains the expected value.
     *
     * @pre The device must support and be configured to use register auto-increment.
     *
     * @param registerAddress The base register to start reading from the device.
     * @param count The size of the field to be verified.
     * @param expected A buffer containing the values expected from the device.
     * @return true if the sensor was verified to be connected
     */
    public boolean verifySensor(int registerAddress, int count, byte[] expected) {
        // TODO: Make use of all 7 read bytes
        byte[] deviceData = new byte[4];
        for (int i = 0, curRegisterAddress = registerAddress; i < count; i += 4, curRegisterAddress += 4) {
            int toRead = count - i < 4 ? count - i : 4;
            // Read the chunk of data.  Return false if the sensor does not respond.
            if (read(curRegisterAddress, toRead, deviceData)) {
                return false;
            }

            for (byte j = 0; j < toRead; j++) {
                if (deviceData[j] != expected[i + j]) {
                    return false;
                }
            }
        }
        return true;
    }
}

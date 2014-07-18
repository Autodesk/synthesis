/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "I2C.h"
#include "DigitalModule.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Synchronized.h"
#include "WPIErrors.h"
#include <taskLib.h>

SEM_ID I2C::m_semaphore = NULL;
uint32_t I2C::m_objCount = 0;

/**
 * Constructor.
 * 
 * @param module The Digital Module to which the device is conneted.
 * @param deviceAddress The address of the device on the I2C bus.
 */
I2C::I2C(DigitalModule *module, uint8_t deviceAddress)
	: m_module (module)
	, m_deviceAddress (deviceAddress)
	, m_compatibilityMode (true)
{
	if (m_semaphore == NULL)
	{
		m_semaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);
	}
	m_objCount++;

	nUsageReporting::report(nUsageReporting::kResourceType_I2C, deviceAddress, module->GetNumber() - 1);
}

/**
 * Destructor.
 */
I2C::~I2C()
{
	m_objCount--;
	if (m_objCount <= 0)
	{
		semDelete(m_semaphore);
		m_semaphore = NULL;
	}
}

/**
 * Generic transaction.
 * 
 * This is a lower-level interface to the I2C hardware giving you more control over each transaction.
 * 
 * @param dataToSend Buffer of data to send as part of the transaction.
 * @param sendSize Number of bytes to send as part of the transaction. [0..6]
 * @param dataReceived Buffer to read data into.
 * @param receiveSize Number of byted to read from the device. [0..7]
 * @return Transfer Aborted... false for success, true for aborted.
 */
bool I2C::Transaction(uint8_t *dataToSend, uint8_t sendSize, uint8_t *dataReceived, uint8_t receiveSize)
{
	if (sendSize > 6)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "sendSize");
		return true;
	}
	if (receiveSize > 7)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "receiveSize");
		return true;
	}

	uint32_t data=0;
	uint32_t dataHigh=0;
	uint32_t i;
	for(i=0; i<sendSize && i<sizeof(data); i++)
	{
		data |= (uint32_t)dataToSend[i] << (8*i);
	}
	for(; i<sendSize; i++)
	{
		dataHigh |= (uint32_t)dataToSend[i] << (8*(i-sizeof(data)));
	}

	bool aborted = true;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	{
		Synchronized sync(m_semaphore);
		m_module->m_fpgaDIO->writeI2CConfig_Address(m_deviceAddress, &localStatus);
		m_module->m_fpgaDIO->writeI2CConfig_BytesToWrite(sendSize, &localStatus);
		m_module->m_fpgaDIO->writeI2CConfig_BytesToRead(receiveSize, &localStatus);
		if (sendSize > 0) m_module->m_fpgaDIO->writeI2CDataToSend(data, &localStatus);
		if (sendSize > sizeof(data)) m_module->m_fpgaDIO->writeI2CConfig_DataToSendHigh(dataHigh, &localStatus);
		m_module->m_fpgaDIO->writeI2CConfig_BitwiseHandshake(m_compatibilityMode, &localStatus);
		uint8_t transaction = m_module->m_fpgaDIO->readI2CStatus_Transaction(&localStatus);
		m_module->m_fpgaDIO->strobeI2CStart(&localStatus);
		while(transaction == m_module->m_fpgaDIO->readI2CStatus_Transaction(&localStatus)) taskDelay(1);
		while(!m_module->m_fpgaDIO->readI2CStatus_Done(&localStatus)) taskDelay(1);
		aborted = m_module->m_fpgaDIO->readI2CStatus_Aborted(&localStatus);
		if (receiveSize > 0) data = m_module->m_fpgaDIO->readI2CDataReceived(&localStatus);
		if (receiveSize > sizeof(data)) dataHigh = m_module->m_fpgaDIO->readI2CStatus_DataReceivedHigh(&localStatus);
	}
	wpi_setError(localStatus);

	for(i=0; i<receiveSize && i<sizeof(data); i++)
	{
		dataReceived[i] = (data >> (8*i)) & 0xFF;
	}
	for(; i<receiveSize; i++)
	{
		dataReceived[i] = (dataHigh >> (8*(i-sizeof(data)))) & 0xFF;
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
bool I2C::AddressOnly()
{
	return Transaction(NULL, 0, NULL, 0);
}

/**
 * Execute a write transaction with the device.
 * 
 * Write a single byte to a register on a device and wait until the
 *   transaction is complete.
 * 
 * @param registerAddress The address of the register on the device to be written.
 * @param data The byte to write to the register on the device.
 * @return Transfer Aborted... false for success, true for aborted.
 */
bool I2C::Write(uint8_t registerAddress, uint8_t data)
{
	uint8_t buffer[2];
	buffer[0] = registerAddress;
	buffer[1] = data;
	return Transaction(buffer, sizeof(buffer), NULL, 0);
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
bool I2C::Read(uint8_t registerAddress, uint8_t count, uint8_t *buffer)
{
	if (count < 1 || count > 7)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "count");
		return true;
	}
	if (buffer == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "buffer");
		return true;
	}

	return Transaction(&registerAddress, sizeof(registerAddress), buffer, count);
}

/**
 * Send a broadcast write to all devices on the I2C bus.
 * 
 * This is not currently implemented!
 * 
 * @param registerAddress The register to write on all devices on the bus.
 * @param data The value to write to the devices.
 */
void I2C::Broadcast(uint8_t registerAddress, uint8_t data)
{
}

/**
 * SetCompatibilityMode
 * 
 * Enables bitwise clock skewing detection.  This will reduce the I2C interface speed,
 * but will allow you to communicate with devices that skew the clock at abnormal times.
 * Compatability mode is enabled by default. 
 * @param enable Enable compatibility mode for this sensor or not.
 */
void I2C::SetCompatibilityMode(bool enable)
{
	m_compatibilityMode = enable;

	const char *cm = NULL;
	if (m_compatibilityMode) cm = "C";
	nUsageReporting::report(nUsageReporting::kResourceType_I2C, m_deviceAddress, m_module->GetNumber() - 1, cm);
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
 */
bool I2C::VerifySensor(uint8_t registerAddress, uint8_t count, const uint8_t *expected)
{
	// TODO: Make use of all 7 read bytes
	uint8_t deviceData[4];
	for (uint8_t i=0, curRegisterAddress = registerAddress; i < count; i+=4, curRegisterAddress+=4)
	{
		uint8_t toRead = count - i < 4 ? count - i : 4;
		// Read the chunk of data.  Return false if the sensor does not respond.
		if (Read(curRegisterAddress, toRead, deviceData)) return false;

		for (uint8_t j=0; j<toRead; j++)
		{
			if(deviceData[j] != expected[i + j]) return false;
		}
	}
	return true;
}


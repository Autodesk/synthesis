/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/I2C.h"

#include "DigitalInternal.h"
#include "HAL/DIO.h"
#include "HAL/HAL.h"
#include "i2clib/i2c-lib.h"

using namespace hal;

static priority_recursive_mutex digitalI2COnBoardMutex;
static priority_recursive_mutex digitalI2CMXPMutex;

static uint8_t i2COnboardObjCount = 0;
static uint8_t i2CMXPObjCount = 0;
static uint8_t i2COnBoardHandle = 0;
static uint8_t i2CMXPHandle = 0;

static HAL_DigitalHandle i2CMXPDigitalHandle1 = HAL_kInvalidHandle;
static HAL_DigitalHandle i2CMXPDigitalHandle2 = HAL_kInvalidHandle;

extern "C" {
/*
 * Initialize the I2C port. Opens the port if necessary and saves the handle.
 * If opening the MXP port, also sets up the channel functions appropriately
 * @param port The port to open, 0 for the on-board, 1 for the MXP.
 */
void HAL_InitializeI2C(HAL_I2CPort port, int32_t* status) {}

/**
 * Generic transaction.
 *
 * This is a lower-level interface to the I2C hardware giving you more control
 * over each transaction.
 *
 * @param dataToSend Buffer of data to send as part of the transaction.
 * @param sendSize Number of bytes to send as part of the transaction.
 * @param dataReceived Buffer to read data into.
 * @param receiveSize Number of bytes to read from the device.
 * @return >= 0 on success or -1 on transfer abort.
 */
int32_t HAL_TransactionI2C(HAL_I2CPort port, int32_t deviceAddress,
                           uint8_t* dataToSend, int32_t sendSize,
                           uint8_t* dataReceived, int32_t receiveSize) {
  return 0;
}

/**
 * Execute a write transaction with the device.
 *
 * Write a single byte to a register on a device and wait until the
 *   transaction is complete.
 *
 * @param registerAddress The address of the register on the device to be
 * written.
 * @param data The byte to write to the register on the device.
 * @return >= 0 on success or -1 on transfer abort.
 */
int32_t HAL_WriteI2C(HAL_I2CPort port, int32_t deviceAddress,
                     uint8_t* dataToSend, int32_t sendSize) {
	  return 0;
}

/**
 * Execute a read transaction with the device.
 *
 * Read bytes from a device.
 * Most I2C devices will auto-increment the register pointer internally allowing
 *   you to read consecutive registers on a device in a single transaction.
 *
 * @param registerAddress The register to read first in the transaction.
 * @param count The number of bytes to read in the transaction.
 * @param buffer A pointer to the array of bytes to store the data read from the
 * device.
 * @return >= 0 on success or -1 on transfer abort.
 */
int32_t HAL_ReadI2C(HAL_I2CPort port, int32_t deviceAddress, uint8_t* buffer,
                    int32_t count) {
	  return 0;
}

void HAL_CloseI2C(HAL_I2CPort port) {}
}

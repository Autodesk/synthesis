/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

#include "HAL/Types.h"

/* General Handle Data Layout
 * Bits 0-15:  Handle Index
 * Bits 16-23: Unused
 * Bits 24-30: Handle Type
 * Bit 31:     1 if handle error, 0 if no error
 *
 * Other specialized handles will use different formats, however Bits 24-31 are
 * always reserved for type and error handling.
 */

namespace hal {

constexpr int16_t InvalidHandleIndex = -1;

enum class HAL_HandleEnum {
  Undefined = 0,
  DIO = 1,
  Port = 2,
  Notifier = 3,
  Interrupt = 4,
  AnalogOutput = 5,
  AnalogInput = 6,
  AnalogTrigger = 7,
  Relay = 8,
  PWM = 9,
  DigitalPWM = 10,
  Counter = 11,
  FPGAEncoder = 12,
  Encoder = 13,
  Compressor = 14,
  Solenoid = 15,
  AnalogGyro = 16,
  Vendor = 17
};

static inline int16_t getHandleIndex(HAL_Handle handle) {
  // mask and return last 16 bits
  return static_cast<int16_t>(handle & 0xffff);
}
static inline HAL_HandleEnum getHandleType(HAL_Handle handle) {
  // mask first 8 bits and cast to enum
  return static_cast<HAL_HandleEnum>((handle >> 24) & 0xff);
}
static inline bool isHandleType(HAL_Handle handle, HAL_HandleEnum handleType) {
  return handleType == getHandleType(handle);
}
static inline int16_t getHandleTypedIndex(HAL_Handle handle,
                                          HAL_HandleEnum enumType) {
  if (!isHandleType(handle, enumType)) return InvalidHandleIndex;
  return getHandleIndex(handle);
}

/* specialized functions for Port handle
 * Port Handle Data Layout
 * Bits 0-7:   Channel Number
 * Bits 8-15:  Module Number
 * Bits 16-23: Unused
 * Bits 24-30: Handle Type
 * Bit 31:     1 if handle error, 0 if no error
 */

// using a 16 bit value so we can store 0-255 and still report error
static inline int16_t getPortHandleChannel(HAL_PortHandle handle) {
  if (!isHandleType(handle, HAL_HandleEnum::Port)) return InvalidHandleIndex;
  return static_cast<uint8_t>(handle & 0xff);
}

// using a 16 bit value so we can store 0-255 and still report error
static inline int16_t getPortHandleModule(HAL_PortHandle handle) {
  if (!isHandleType(handle, HAL_HandleEnum::Port)) return InvalidHandleIndex;
  return static_cast<uint8_t>((handle >> 8) & 0xff);
}

// using a 16 bit value so we can store 0-255 and still report error
static inline int16_t getPortHandleSPIEnable(HAL_PortHandle handle) {
  if (!isHandleType(handle, HAL_HandleEnum::Port)) return InvalidHandleIndex;
  return static_cast<uint8_t>((handle >> 16) & 0xff);
}

HAL_PortHandle createPortHandle(uint8_t channel, uint8_t module);

HAL_PortHandle createPortHandleForSPI(uint8_t channel);

HAL_Handle createHandle(int16_t index, HAL_HandleEnum handleType);
}  // namespace hal

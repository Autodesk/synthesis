/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/Power.h"

#include <memory>

#include "HAL/ChipObject.h"

using namespace hal;

extern "C" {

/**
 * Get the roboRIO input voltage
 */
double HAL_GetVinVoltage(int32_t* status) {
  return 12.0;
}

/**
 * Get the roboRIO input current
 */
double HAL_GetVinCurrent(int32_t* status) {
  return 0.0;
}

/**
 * Get the 6V rail voltage
 */
double HAL_GetUserVoltage6V(int32_t* status) {
  return 6.0;
}

/**
 * Get the 6V rail current
 */
double HAL_GetUserCurrent6V(int32_t* status) {
  return 0.0;
}

/**
 * Get the active state of the 6V rail
 */
HAL_Bool HAL_GetUserActive6V(int32_t* status) {
  return true;
}

/**
 * Get the fault count for the 6V rail
 */
int32_t HAL_GetUserCurrentFaults6V(int32_t* status) {
  return 0;
}

/**
 * Get the 5V rail voltage
 */
double HAL_GetUserVoltage5V(int32_t* status) {
  return 5.0;
}

/**
 * Get the 5V rail current
 */
double HAL_GetUserCurrent5V(int32_t* status) {
  return 0.0;
}

/**
 * Get the active state of the 5V rail
 */
HAL_Bool HAL_GetUserActive5V(int32_t* status) {
  return true;
}

/**
 * Get the fault count for the 5V rail
 */
int32_t HAL_GetUserCurrentFaults5V(int32_t* status) {
  return 0;
}

/**
 * Get the 3.3V rail voltage
 */
double HAL_GetUserVoltage3V3(int32_t* status) {
  return 3.0;
}

/**
 * Get the 3.3V rail current
 */
double HAL_GetUserCurrent3V3(int32_t* status) {
  return 0.0;
}

/**
 * Get the active state of the 3.3V rail
 */
HAL_Bool HAL_GetUserActive3V3(int32_t* status) {
  return true;
}

/**
 * Get the fault count for the 3.3V rail
 */
int32_t HAL_GetUserCurrentFaults3V3(int32_t* status) {
  return 0;
}

}  // extern "C"

/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/PDP.h"

#include <memory>

#include "HAL/Errors.h"
#include "HAL/Ports.h"
#include "HAL/cpp/make_unique.h"
#include "PortsInternal.h"
//#include "ctre/PDP.h"

using namespace hal;

//static std::unique_ptr<PDP> pdp[kNumPDPModules];

static inline bool checkPDPInit(int32_t module, int32_t* status) {
}

extern "C" {

void HAL_InitializePDP(int32_t module, int32_t* status) {}

HAL_Bool HAL_CheckPDPModule(int32_t module) {
  return module < kNumPDPModules && module >= 0;
}

HAL_Bool HAL_CheckPDPChannel(int32_t channel) {
  return channel < kNumPDPChannels && channel >= 0;
}

double HAL_GetPDPTemperature(int32_t module, int32_t* status) {
  return 0.0;
}

double HAL_GetPDPVoltage(int32_t module, int32_t* status) {
  return 0.0;
}

double HAL_GetPDPChannelCurrent(int32_t module, int32_t channel,
                                int32_t* status) {
  return 0.0;
}

double HAL_GetPDPTotalCurrent(int32_t module, int32_t* status) {
  return 0.0;
}

double HAL_GetPDPTotalPower(int32_t module, int32_t* status) {
  return 0.0;
}

double HAL_GetPDPTotalEnergy(int32_t module, int32_t* status) {
  return 0.0;
}

void HAL_ResetPDPTotalEnergy(int32_t module, int32_t* status) {}

void HAL_ClearPDPStickyFaults(int32_t module, int32_t* status) {}

}  // extern "C"

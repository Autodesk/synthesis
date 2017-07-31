/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

#include <memory>

#include "HAL/Errors.h"
#include "HAL/Ports.h"
#include "HAL/Solenoid.h"
#include "PortsInternal.h"
#include "ctre/PCM.h"

namespace hal {
extern std::unique_ptr<PCM> PCM_modules[kNumPCMModules];

static inline bool checkPCMInit(int32_t module, int32_t* status) {
  if (!HAL_CheckSolenoidModule(module)) {
    *status = RESOURCE_OUT_OF_RANGE;
    return false;
  }
  if (!PCM_modules[module]) {
    *status = INCOMPATIBLE_STATE;
    return false;
  }
  return true;
}

void initializePCM(int32_t module, int32_t* status);
}  // namespace hal

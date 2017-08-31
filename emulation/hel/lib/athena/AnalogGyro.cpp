/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/AnalogGyro.h"

#include <chrono>
#include <thread>

#include "AnalogInternal.h"
#include "HAL/AnalogAccumulator.h"
#include "HAL/AnalogInput.h"
#include "HAL/handles/IndexedHandleResource.h"

namespace {
struct AnalogGyro {
  HAL_AnalogInputHandle handle;
  double voltsPerDegreePerSecond;
  double offset;
  int32_t center;
};
}

static constexpr uint32_t kOversampleBits = 10;
static constexpr uint32_t kAverageBits = 0;
static constexpr double kSamplesPerSecond = 50.0;
static constexpr double kCalibrationSampleTime = 5.0;
static constexpr double kDefaultVoltsPerDegreePerSecond = 0.007;

using namespace hal;

static IndexedHandleResource<HAL_GyroHandle, AnalogGyro, kNumAccumulators,
                             HAL_HandleEnum::AnalogGyro>
    analogGyroHandles;

static void Wait(double seconds) {
  if (seconds < 0.0) return;
  std::this_thread::sleep_for(std::chrono::duration<double>(seconds));
}

extern "C" {
HAL_GyroHandle HAL_InitializeAnalogGyro(HAL_AnalogInputHandle analogHandle,
                                        int32_t* status) {
	return 0;
  }
}

void HAL_SetupAnalogGyro(HAL_GyroHandle handle, int32_t* status) {}

void HAL_FreeAnalogGyro(HAL_GyroHandle handle) {}

void HAL_SetAnalogGyroParameters(HAL_GyroHandle handle,
                                 double voltsPerDegreePerSecond, double offset,
                                 int32_t center, int32_t* status) {}

void HAL_SetAnalogGyroVoltsPerDegreePerSecond(HAL_GyroHandle handle,
                                              double voltsPerDegreePerSecond,
                                              int32_t* status) {}

void HAL_ResetAnalogGyro(HAL_GyroHandle handle, int32_t* status) {}

void HAL_CalibrateAnalogGyro(HAL_GyroHandle handle, int32_t* status) {}

void HAL_SetAnalogGyroDeadband(HAL_GyroHandle handle, double volts,
                               int32_t* status) {}

double HAL_GetAnalogGyroAngle(HAL_GyroHandle handle, int32_t* status) {
  return 0;
}

double HAL_GetAnalogGyroRate(HAL_GyroHandle handle, int32_t* status) {
  return 0;
}

double HAL_GetAnalogGyroOffset(HAL_GyroHandle handle, int32_t* status) {
  return 0;
}

int32_t HAL_GetAnalogGyroCenter(HAL_GyroHandle handle, int32_t* status) {
  return 0;
}


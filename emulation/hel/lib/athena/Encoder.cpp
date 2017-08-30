/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/Encoder.h"

#include "EncoderInternal.h"
#include "FPGAEncoder.h"
#include "HAL/ChipObject.h"
#include "HAL/Counter.h"
#include "HAL/Errors.h"
#include "HAL/handles/LimitedClassedHandleResource.h"
#include "PortsInternal.h"

using namespace hal;

Encoder::Encoder(HAL_Handle digitalSourceHandleA,
                 HAL_AnalogTriggerType analogTriggerTypeA,
                 HAL_Handle digitalSourceHandleB,
                 HAL_AnalogTriggerType analogTriggerTypeB,
                 bool reverseDirection, HAL_EncoderEncodingType encodingType,
                 int32_t* status) {
  m_encodingType = encodingType;
}

void Encoder::SetupCounter(HAL_Handle digitalSourceHandleA,
                           HAL_AnalogTriggerType analogTriggerTypeA,
                           HAL_Handle digitalSourceHandleB,
                           HAL_AnalogTriggerType analogTriggerTypeB,
                           bool reverseDirection,
                           HAL_EncoderEncodingType encodingType,
                           int32_t* status) {}

Encoder::~Encoder() {}

// CounterBase interface
int32_t Encoder::Get(int32_t* status) const {
  return static_cast<int32_t>(GetRaw(status) * DecodingScaleFactor());
}

int32_t Encoder::GetRaw(int32_t* status) const {
  return 0;
}

int32_t Encoder::GetEncodingScale(int32_t* status) const {
  return m_encodingScale;
}

void Encoder::Reset(int32_t* status) {}

double Encoder::GetPeriod(int32_t* status) const {
  return 0;
}

void Encoder::SetMaxPeriod(double maxPeriod, int32_t* status) {}

bool Encoder::GetStopped(int32_t* status) const {
  return true;
}

bool Encoder::GetDirection(int32_t* status) const {
  return true;
}

double Encoder::GetDistance(int32_t* status) const {
  return 0.0;
}

double Encoder::GetRate(int32_t* status) const {
  return 0.0;
}

void Encoder::SetMinRate(double minRate, int32_t* status) {}

void Encoder::SetDistancePerPulse(double distancePerPulse, int32_t* status) {}

void Encoder::SetReverseDirection(bool reverseDirection, int32_t* status) {}

void Encoder::SetSamplesToAverage(int32_t samplesToAverage, int32_t* status) {}

int32_t Encoder::GetSamplesToAverage(int32_t* status) const {
  return 0;
}

void Encoder::SetIndexSource(HAL_Handle digitalSourceHandle,
                             HAL_AnalogTriggerType analogTriggerType,
                             HAL_EncoderIndexingType type, int32_t* status) {}

double Encoder::DecodingScaleFactor() const {
  return 0.0;
}

static LimitedClassedHandleResource<HAL_EncoderHandle, Encoder,
                                    kNumEncoders + kNumCounters,
                                    HAL_HandleEnum::Encoder>
    encoderHandles;

extern "C" {
HAL_EncoderHandle HAL_InitializeEncoder(
    HAL_Handle digitalSourceHandleA, HAL_AnalogTriggerType analogTriggerTypeA,
    HAL_Handle digitalSourceHandleB, HAL_AnalogTriggerType analogTriggerTypeB,
    HAL_Bool reverseDirection, HAL_EncoderEncodingType encodingType,
    int32_t* status) {
  auto encoder = std::make_shared<Encoder>(
      digitalSourceHandleA, analogTriggerTypeA, digitalSourceHandleB,
      analogTriggerTypeB, reverseDirection, encodingType, status);
  if (*status != 0) return HAL_kInvalidHandle;  // return in creation error
  auto handle = encoderHandles.Allocate(encoder);
  if (handle == HAL_kInvalidHandle) {
    *status = NO_AVAILABLE_RESOURCES;
    return HAL_kInvalidHandle;
  }
  return handle;
}

void HAL_FreeEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
  encoderHandles.Free(encoderHandle);
}

int32_t HAL_GetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->Get(status);
}

int32_t HAL_GetEncoderRaw(HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetRaw(status);
}

int32_t HAL_GetEncoderEncodingScale(HAL_EncoderHandle encoderHandle,
                                    int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetEncodingScale(status);
}

void HAL_ResetEncoder(HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->Reset(status);
}

double HAL_GetEncoderPeriod(HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetPeriod(status);
}

void HAL_SetEncoderMaxPeriod(HAL_EncoderHandle encoderHandle, double maxPeriod,
                             int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetMaxPeriod(maxPeriod, status);
}

HAL_Bool HAL_GetEncoderStopped(HAL_EncoderHandle encoderHandle,
                               int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetStopped(status);
}

HAL_Bool HAL_GetEncoderDirection(HAL_EncoderHandle encoderHandle,
                                 int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetDirection(status);
}

double HAL_GetEncoderDistance(HAL_EncoderHandle encoderHandle,
                              int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetDistance(status);
}

double HAL_GetEncoderRate(HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetRate(status);
}

void HAL_SetEncoderMinRate(HAL_EncoderHandle encoderHandle, double minRate,
                           int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetMinRate(minRate, status);
}

void HAL_SetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle,
                                    double distancePerPulse, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetDistancePerPulse(distancePerPulse, status);
}

void HAL_SetEncoderReverseDirection(HAL_EncoderHandle encoderHandle,
                                    HAL_Bool reverseDirection,
                                    int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetReverseDirection(reverseDirection, status);
}

void HAL_SetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle,
                                    int32_t samplesToAverage, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetSamplesToAverage(samplesToAverage, status);
}

int32_t HAL_GetEncoderSamplesToAverage(HAL_EncoderHandle encoderHandle,
                                       int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetSamplesToAverage(status);
}

double HAL_GetEncoderDecodingScaleFactor(HAL_EncoderHandle encoderHandle,
                                         int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->DecodingScaleFactor();
}

double HAL_GetEncoderDistancePerPulse(HAL_EncoderHandle encoderHandle,
                                      int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetDistancePerPulse();
}

HAL_EncoderEncodingType HAL_GetEncoderEncodingType(
    HAL_EncoderHandle encoderHandle, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return HAL_Encoder_k4X;  // default to k4X
  }
  return encoder->GetEncodingType();
}

void HAL_SetEncoderIndexSource(HAL_EncoderHandle encoderHandle,
                               HAL_Handle digitalSourceHandle,
                               HAL_AnalogTriggerType analogTriggerType,
                               HAL_EncoderIndexingType type, int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  encoder->SetIndexSource(digitalSourceHandle, analogTriggerType, type, status);
}

int32_t HAL_GetEncoderFPGAIndex(HAL_EncoderHandle encoderHandle,
                                int32_t* status) {
  auto encoder = encoderHandles.Get(encoderHandle);
  if (encoder == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  return encoder->GetFPGAIndex();
}
}

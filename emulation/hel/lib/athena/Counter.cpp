/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "HAL/Counter.h"

#include "ConstantsInternal.h"
#include "DigitalInternal.h"
#include "HAL/HAL.h"
#include "HAL/handles/LimitedHandleResource.h"
#include "PortsInternal.h"

using namespace hal;

namespace {
struct Counter {
  //std::unique_ptr<tCounter> counter;
  //uint8_tindex;
  std::unique_ptr<int32_t> counter;
};
}

static LimitedHandleResource<HAL_CounterHandle, Counter, kNumCounters,
                             HAL_HandleEnum::Counter>
    counterHandles;

extern "C" {
HAL_CounterHandle HAL_InitializeCounter(HAL_Counter_Mode mode, int32_t* index,
                                        int32_t* status) {
  auto handle = counterHandles.Allocate();
  if (handle == HAL_kInvalidHandle) {  // out of resources
    *status = NO_AVAILABLE_RESOURCES;
    return HAL_kInvalidHandle;
  }
  auto counter = counterHandles.Get(handle);
  if (counter == nullptr) {  // would only occur on thread issues
    *status = HAL_HANDLE_ERROR;
    return HAL_kInvalidHandle;
  }

  counter->counter.reset(0);

  return handle;
}

void HAL_FreeCounter(HAL_CounterHandle counterHandle, int32_t* status) {
  counterHandles.Free(counterHandle);
}

void HAL_SetCounterAverageSize(HAL_CounterHandle counterHandle, int32_t size,
                               int32_t* status) {}

/**
 * Set the source object that causes the counter to count up.
 * Set the up counting DigitalSource.
 */
void HAL_SetCounterUpSource(HAL_CounterHandle counterHandle,
                            HAL_Handle digitalSourceHandle,
                            HAL_AnalogTriggerType analogTriggerType,
                            int32_t* status) {}

/**
 * Set the edge sensitivity on an up counting source.
 * Set the up source to either detect rising edges or falling edges.
 */
void HAL_SetCounterUpSourceEdge(HAL_CounterHandle counterHandle,
                                HAL_Bool risingEdge, HAL_Bool fallingEdge,
                                int32_t* status) {}

/**
 * Disable the up counting source to the counter.
 */
void HAL_ClearCounterUpSource(HAL_CounterHandle counterHandle,
                              int32_t* status) {}

/**
 * Set the source object that causes the counter to count down.
 * Set the down counting DigitalSource.
 */
void HAL_SetCounterDownSource(HAL_CounterHandle counterHandle,
                              HAL_Handle digitalSourceHandle,
                              HAL_AnalogTriggerType analogTriggerType,
                              int32_t* status) {}

/**
 * Set the edge sensitivity on a down counting source.
 * Set the down source to either detect rising edges or falling edges.
 */
void HAL_SetCounterDownSourceEdge(HAL_CounterHandle counterHandle,
                                  HAL_Bool risingEdge, HAL_Bool fallingEdge,
                                  int32_t* status) {}

/**
 * Disable the down counting source to the counter.
 */
void HAL_ClearCounterDownSource(HAL_CounterHandle counterHandle,
                                int32_t* status) {}

/**
 * Set standard up / down counting mode on this counter.
 * Up and down counts are sourced independently from two inputs.
 */
void HAL_SetCounterUpDownMode(HAL_CounterHandle counterHandle,
                              int32_t* status) {}

/**
 * Set external direction mode on this counter.
 * Counts are sourced on the Up counter input.
 * The Down counter input represents the direction to count.
 */
void HAL_SetCounterExternalDirectionMode(HAL_CounterHandle counterHandle,
                                         int32_t* status) {}

/**
 * Set Semi-period mode on this counter.
 * Counts up on both rising and falling edges.
 */
void HAL_SetCounterSemiPeriodMode(HAL_CounterHandle counterHandle,
                                  HAL_Bool highSemiPeriod, int32_t* status) {}

/**
 * Configure the counter to count in up or down based on the length of the input
 * pulse.
 * This mode is most useful for direction sensitive gear tooth sensors.
 * @param threshold The pulse length beyond which the counter counts the
 * opposite direction.  Units are seconds.
 */
void HAL_SetCounterPulseLengthMode(HAL_CounterHandle counterHandle,
                                   double threshold, int32_t* status) {}

/**
 * Get the Samples to Average which specifies the number of samples of the timer
 * to
 * average when calculating the period. Perform averaging to account for
 * mechanical imperfections or as oversampling to increase resolution.
 * @return SamplesToAverage The number of samples being averaged (from 1 to 127)
 */
int32_t HAL_GetCounterSamplesToAverage(HAL_CounterHandle counterHandle,
                                       int32_t* status) {
  return 0;
}

/**
 * Set the Samples to Average which specifies the number of samples of the timer
 * to average when calculating the period. Perform averaging to account for
 * mechanical imperfections or as oversampling to increase resolution.
 * @param samplesToAverage The number of samples to average from 1 to 127.
 */
void HAL_SetCounterSamplesToAverage(HAL_CounterHandle counterHandle,
                                    int32_t samplesToAverage, int32_t* status) {}

/**
 * Reset the Counter to zero.
 * Set the counter value to zero. This doesn't effect the running state of the
 * counter, just sets the current value to zero.
 */
void HAL_ResetCounter(HAL_CounterHandle counterHandle, int32_t* status) {
  auto counter = counterHandles.Get(counterHandle);
  if (counter == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return;
  }
  *counter->counter = 0;
}

/**
 * Read the current counter value.
 * Read the value at this instant. It may still be running, so it reflects the
 * current value. Next time it is read, it might have a different value.
 */
int32_t HAL_GetCounter(HAL_CounterHandle counterHandle, int32_t* status) {
  auto counter = counterHandles.Get(counterHandle);
  if (counter == nullptr) {
    *status = HAL_HANDLE_ERROR;
    return 0;
  }
  int32_t value = *counter->counter;
  return value;
}

/*
 * Get the Period of the most recent count.
 * Returns the time interval of the most recent count. This can be used for
 * velocity calculations to determine shaft speed.
 * @returns The period of the last two pulses in units of seconds.
 */
double HAL_GetCounterPeriod(HAL_CounterHandle counterHandle, int32_t* status) {
  return 0.0;
}

/**
 * Set the maximum period where the device is still considered "moving".
 * Sets the maximum period where the device is considered moving. This value is
 * used to determine the "stopped" state of the counter using the GetStopped
 * method.
 * @param maxPeriod The maximum period where the counted device is considered
 * moving in seconds.
 */
void HAL_SetCounterMaxPeriod(HAL_CounterHandle counterHandle, double maxPeriod,
                             int32_t* status) {}

/**
 * Select whether you want to continue updating the event timer output when
 * there are no samples captured. The output of the event timer has a buffer of
 * periods that are averaged and posted to a register on the FPGA.  When the
 * timer detects that the event source has stopped (based on the MaxPeriod) the
 * buffer of samples to be averaged is emptied.  If you enable the update when
 * empty, you will be notified of the stopped source and the event time will
 * report 0 samples.  If you disable update when empty, the most recent average
 * will remain on the output until a new sample is acquired.  You will never see
 * 0 samples output (except when there have been no events since an FPGA reset)
 * and you will likely not see the stopped bit become true (since it is updated
 * at the end of an average and there are no samples to average).
 */
void HAL_SetCounterUpdateWhenEmpty(HAL_CounterHandle counterHandle,
                                   HAL_Bool enabled, int32_t* status) {}

/**
 * Determine if the clock is stopped.
 * Determine if the clocked input is stopped based on the MaxPeriod value set
 * using the SetMaxPeriod method. If the clock exceeds the MaxPeriod, then the
 * device (and counter) are assumed to be stopped and it returns true.
 * @return Returns true if the most recent counter period exceeds the MaxPeriod
 * value set by SetMaxPeriod.
 */
HAL_Bool HAL_GetCounterStopped(HAL_CounterHandle counterHandle,
                               int32_t* status) {
  return true;
}

/**
 * The last direction the counter value changed.
 * @return The last direction the counter value changed.
 */
HAL_Bool HAL_GetCounterDirection(HAL_CounterHandle counterHandle,
                                 int32_t* status) {
  return true;
}

/**
 * Set the Counter to return reversed sensing on the direction.
 * This allows counters to change the direction they are counting in the case of
 * 1X and 2X quadrature encoding only. Any other counter mode isn't supported.
 * @param reverseDirection true if the value counted should be negated.
 */
void HAL_SetCounterReverseDirection(HAL_CounterHandle counterHandle,
                                    HAL_Bool reverseDirection,
                                    int32_t* status) {}
}

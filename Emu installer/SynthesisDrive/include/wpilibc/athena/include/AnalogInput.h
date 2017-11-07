/*----------------------------------------------------------------------------*/
/* Copyright (c) 2008-2017 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <stdint.h>

#include <memory>
#include <string>

#include <HAL/Types.h>

#include "LiveWindow/LiveWindowSendable.h"
#include "PIDSource.h"
#include "SensorBase.h"

namespace frc {

/**
 * Analog input class.
 *
 * Connected to each analog channel is an averaging and oversampling engine.
 * This engine accumulates the specified ( by SetAverageBits() and
 * SetOversampleBits() ) number of samples before returning a new value.  This
 * is not a sliding window average.  The only difference between the oversampled
 * samples and the averaged samples is that the oversampled samples are simply
 * accumulated effectively increasing the resolution, while the averaged samples
 * are divided by the number of samples to retain the resolution, but get more
 * stable values.
 */
class AnalogInput : public SensorBase,
                    public PIDSource,
                    public LiveWindowSendable {
  friend class AnalogTrigger;
  friend class AnalogGyro;

 public:
  static const int kAccumulatorModuleNumber = 1;
  static const int kAccumulatorNumChannels = 2;
  static const int kAccumulatorChannels[kAccumulatorNumChannels];

  explicit AnalogInput(int channel);
  virtual ~AnalogInput();

  int GetValue() const;
  int GetAverageValue() const;

  double GetVoltage() const;
  double GetAverageVoltage() const;

  int GetChannel() const;

  void SetAverageBits(int bits);
  int GetAverageBits() const;
  void SetOversampleBits(int bits);
  int GetOversampleBits() const;

  int GetLSBWeight() const;
  int GetOffset() const;

  bool IsAccumulatorChannel() const;
  void InitAccumulator();
  void SetAccumulatorInitialValue(int64_t value);
  void ResetAccumulator();
  void SetAccumulatorCenter(int center);
  void SetAccumulatorDeadband(int deadband);
  int64_t GetAccumulatorValue() const;
  int64_t GetAccumulatorCount() const;
  void GetAccumulatorOutput(int64_t& value, int64_t& count) const;

  static void SetSampleRate(double samplesPerSecond);
  static double GetSampleRate();

  double PIDGet() override;

  void UpdateTable() override;
  void StartLiveWindowMode() override;
  void StopLiveWindowMode() override;
  std::string GetSmartDashboardType() const override;
  void InitTable(std::shared_ptr<ITable> subTable) override;
  std::shared_ptr<ITable> GetTable() const override;

 private:
  int m_channel;
  // TODO: Adjust HAL to avoid use of raw pointers.
  HAL_AnalogInputHandle m_port;
  int64_t m_accumulatorOffset;

  std::shared_ptr<ITable> m_table;
};

}  // namespace frc

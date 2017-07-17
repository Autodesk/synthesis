/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <memory>
#include <string>

#include "CounterBase.h"
#include "LiveWindow/LiveWindowSendable.h"
#include "SensorBase.h"

namespace frc {

/**
 * Class for counting the number of ticks on a digital input channel.
 *
 * This is a general purpose class for counting repetitive events. It can return
 * the number of counts, the period of the most recent cycle, and detect when
 * the signal being counted has stopped by supplying a maximum cycle time.
 *
 * All counters will immediately start counting - Reset() them if you need them
 * to be zeroed before use.
 */
class Counter : public SensorBase,
                public CounterBase,
                public LiveWindowSendable {
 public:
  explicit Counter(Mode mode = kTwoPulse);
  explicit Counter(int channel);
  // TODO: [Not Supported] explicit Counter(DigitalSource *source);
  // TODO: [Not Supported] explicit Counter(DigitalSource &source);
  // TODO: [Not Supported] explicit Counter(AnalogTrigger *source);
  // TODO: [Not Supported] explicit Counter(AnalogTrigger &source);
  // TODO: [Not Supported] Counter(EncodingType encodingType, DigitalSource
  // *upSource, DigitalSource *downSource, bool inverted);
  virtual ~Counter();

  void SetUpSource(int channel);
  // TODO: [Not Supported] void SetUpSource(AnalogTrigger *analogTrigger,
  // AnalogTriggerType triggerType);
  // TODO: [Not Supported] void SetUpSource(AnalogTrigger &analogTrigger,
  // AnalogTriggerType triggerType);
  // TODO: [Not Supported] void SetUpSource(DigitalSource *source);
  // TODO: [Not Supported] void SetUpSource(DigitalSource &source);
  void SetUpSourceEdge(bool risingEdge, bool fallingEdge);
  void ClearUpSource();

  void SetDownSource(int channel);
  // TODO: [Not Supported] void SetDownSource(AnalogTrigger *analogTrigger,
  // AnalogTriggerType triggerType);
  // TODO: [Not Supported] void SetDownSource(AnalogTrigger &analogTrigger,
  // AnalogTriggerType triggerType);
  // TODO: [Not Supported] void SetDownSource(DigitalSource *source);
  // TODO: [Not Supported] void SetDownSource(DigitalSource &source);
  void SetDownSourceEdge(bool risingEdge, bool fallingEdge);
  void ClearDownSource();

  void SetUpDownCounterMode();
  void SetExternalDirectionMode();
  void SetSemiPeriodMode(bool highSemiPeriod);
  void SetPulseLengthMode(double threshold);

  void SetReverseDirection(bool reverseDirection);

  // CounterBase interface
  int Get() const override;
  void Reset() override;
  double GetPeriod() const override;
  void SetMaxPeriod(double maxPeriod) override;
  void SetUpdateWhenEmpty(bool enabled);
  bool GetStopped() const override;
  bool GetDirection() const override;

  void SetSamplesToAverage(int samplesToAverage);
  int GetSamplesToAverage() const;
  int GetFPGAIndex() const { return m_index; }

  void UpdateTable() override;
  void StartLiveWindowMode() override;
  void StopLiveWindowMode() override;
  std::string GetSmartDashboardType() const override;
  void InitTable(std::shared_ptr<ITable> subTable) override;
  std::shared_ptr<ITable> GetTable() const override;

 protected:
  // What makes the counter count up.
  // TODO: [Not Supported] DigitalSource *m_upSource;
  // What makes the counter count down.
  // TODO: [Not Supported] DigitalSource *m_downSource;
  void* m_counter;  ///< The FPGA counter object.
 private:
  bool m_allocatedUpSource;    ///< Was the upSource allocated locally?
  bool m_allocatedDownSource;  ///< Was the downSource allocated locally?
  int m_index;                 ///< The index of this counter.

  std::shared_ptr<ITable> m_table;
};

}  // namespace frc

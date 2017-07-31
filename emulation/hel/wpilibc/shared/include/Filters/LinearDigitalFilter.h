/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2015-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <initializer_list>
#include <memory>
#include <vector>

#include "CircularBuffer.h"
#include "Filter.h"

namespace frc {

/**
 * This class implements a linear, digital filter. All types of FIR and IIR
 * filters are supported. Static factory methods are provided to create commonly
 * used types of filters.
 *
 * Filters are of the form:<br>
 *  y[n] = (b0 * x[n] + b1 * x[n-1] + … + bP * x[n-P]) -
 *         (a0 * y[n-1] + a2 * y[n-2] + … + aQ * y[n-Q])
 *
 * Where:<br>
 *  y[n] is the output at time "n"<br>
 *  x[n] is the input at time "n"<br>
 *  y[n-1] is the output from the LAST time step ("n-1")<br>
 *  x[n-1] is the input from the LAST time step ("n-1")<br>
 *  b0 … bP are the "feedforward" (FIR) gains<br>
 *  a0 … aQ are the "feedback" (IIR) gains<br>
 * IMPORTANT! Note the "-" sign in front of the feedback term! This is a common
 *            convention in signal processing.
 *
 * What can linear filters do? Basically, they can filter, or diminish, the
 * effects of undesirable input frequencies. High frequencies, or rapid changes,
 * can be indicative of sensor noise or be otherwise undesirable. A "low pass"
 * filter smooths out the signal, reducing the impact of these high frequency
 * components.  Likewise, a "high pass" filter gets rid of slow-moving signal
 * components, letting you detect large changes more easily.
 *
 * Example FRC applications of filters:
 *  - Getting rid of noise from an analog sensor input (note: the roboRIO's FPGA
 *    can do this faster in hardware)
 *  - Smoothing out joystick input to prevent the wheels from slipping or the
 *    robot from tipping
 *  - Smoothing motor commands so that unnecessary strain isn't put on
 *    electrical or mechanical components
 *  - If you use clever gains, you can make a PID controller out of this class!
 *
 * For more on filters, I highly recommend the following articles:<br>
 *  http://en.wikipedia.org/wiki/Linear_filter<br>
 *  http://en.wikipedia.org/wiki/Iir_filter<br>
 *  http://en.wikipedia.org/wiki/Fir_filter<br>
 *
 * Note 1: PIDGet() should be called by the user on a known, regular period.
 * You can set up a Notifier to do this (look at the WPILib PIDController
 * class), or do it "inline" with code in a periodic function.
 *
 * Note 2: For ALL filters, gains are necessarily a function of frequency. If
 * you make a filter that works well for you at, say, 100Hz, you will most
 * definitely need to adjust the gains if you then want to run it at 200Hz!
 * Combining this with Note 1 - the impetus is on YOU as a developer to make
 * sure PIDGet() gets called at the desired, constant frequency!
 */
class LinearDigitalFilter : public Filter {
 public:
  LinearDigitalFilter(std::shared_ptr<PIDSource> source,
                      std::initializer_list<double> ffGains,
                      std::initializer_list<double> fbGains);
  LinearDigitalFilter(std::shared_ptr<PIDSource> source,
                      std::initializer_list<double> ffGains,
                      const std::vector<double>& fbGains);
  LinearDigitalFilter(std::shared_ptr<PIDSource> source,
                      const std::vector<double>& ffGains,
                      std::initializer_list<double> fbGains);
  LinearDigitalFilter(std::shared_ptr<PIDSource> source,
                      const std::vector<double>& ffGains,
                      const std::vector<double>& fbGains);

  // Static methods to create commonly used filters
  static LinearDigitalFilter SinglePoleIIR(std::shared_ptr<PIDSource> source,
                                           double timeConstant, double period);
  static LinearDigitalFilter HighPass(std::shared_ptr<PIDSource> source,
                                      double timeConstant, double period);
  static LinearDigitalFilter MovingAverage(std::shared_ptr<PIDSource> source,
                                           int taps);

  // Filter interface
  double Get() const override;
  void Reset() override;

  // PIDSource interface
  double PIDGet() override;

 private:
  CircularBuffer<double> m_inputs;
  CircularBuffer<double> m_outputs;
  std::vector<double> m_inputGains;
  std::vector<double> m_outputGains;
};

}  // namespace frc

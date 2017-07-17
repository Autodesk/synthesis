/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "TalonSRX.h"

#include "HAL/HAL.h"
#include "LiveWindow/LiveWindow.h"

using namespace frc;

/**
 * Construct a TalonSRX connected via PWM.
 *
 * @param channel The PWM channel that the TalonSRX is attached to. 0-9 are
 *                on-board, 10-19 are on the MXP port
 */
TalonSRX::TalonSRX(int channel) : PWMSpeedController(channel) {
  /* Note that the TalonSRX uses the following bounds for PWM values. These
   * values should work reasonably well for most controllers, but if users
   * experience issues such as asymmetric behavior around the deadband or
   * inability to saturate the controller in either direction, calibration is
   * recommended. The calibration procedure can be found in the TalonSRX User
   * Manual available from Cross The Road Electronics.
   *   2.004ms = full "forward"
   *   1.52ms = the "high end" of the deadband range
   *   1.50ms = center of the deadband range (off)
   *   1.48ms = the "low end" of the deadband range
   *   0.997ms = full "reverse"
   */
  SetBounds(2.004, 1.52, 1.50, 1.48, .997);
  SetPeriodMultiplier(kPeriodMultiplier_1X);
  SetSpeed(0.0);
  SetZeroLatch();

  HAL_Report(HALUsageReporting::kResourceType_TalonSRX, GetChannel());
  LiveWindow::GetInstance()->AddActuator("TalonSRX", GetChannel(), this);
}

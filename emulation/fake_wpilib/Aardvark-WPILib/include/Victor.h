/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include "PWMSpeedController.h"

namespace frc {

/**
 * Vex Robotics Victor 888 Speed Controller
 *
 * The Vex Robotics Victor 884 Speed Controller can also be used with this
 * class but may need to be calibrated per the Victor 884 user manual.
 */
class Victor : public PWMSpeedController {
 public:
  explicit Victor(int channel);
  virtual ~Victor() = default;
};

}  // namespace frc

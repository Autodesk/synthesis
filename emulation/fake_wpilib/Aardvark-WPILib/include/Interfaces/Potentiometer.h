/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include "PIDSource.h"

namespace frc {

/**
 * Interface for potentiometers.
 */
class Potentiometer : public PIDSource {
 public:
  virtual ~Potentiometer() = default;

  /**
   * Common interface for getting the current value of a potentiometer.
   *
   * @return The current set speed.  Value is between -1.0 and 1.0.
   */
  virtual double Get() const = 0;

  void SetPIDSourceType(PIDSourceType pidSource) override;
};

}  // namespace frc

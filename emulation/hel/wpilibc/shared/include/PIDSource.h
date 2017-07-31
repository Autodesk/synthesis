/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

namespace frc {

enum class PIDSourceType { kDisplacement, kRate };

/**
 * PIDSource interface is a generic sensor source for the PID class.
 * All sensors that can be used with the PID class will implement the PIDSource
 * that returns a standard value that will be used in the PID code.
 */
class PIDSource {
 public:
  virtual void SetPIDSourceType(PIDSourceType pidSource);
  PIDSourceType GetPIDSourceType() const;
  virtual double PIDGet() = 0;

 protected:
  PIDSourceType m_pidSource = PIDSourceType::kDisplacement;
};

}  // namespace frc

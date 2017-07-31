/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <string>

#include "Commands/Command.h"

namespace frc {

class WaitUntilCommand : public Command {
 public:
  explicit WaitUntilCommand(double time);
  WaitUntilCommand(const std::string& name, double time);
  virtual ~WaitUntilCommand() = default;

 protected:
  virtual bool IsFinished();

 private:
  double m_time;
};

}  // namespace frc

/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include <memory>
#include <string>

#include "Commands/Command.h"
#include "PIDController.h"
#include "PIDOutput.h"
#include "PIDSource.h"

namespace frc {

class PIDCommand : public Command, public PIDOutput, public PIDSource {
 public:
  PIDCommand(const std::string& name, double p, double i, double d);
  PIDCommand(const std::string& name, double p, double i, double d,
             double period);
  PIDCommand(const std::string& name, double p, double i, double d, double f,
             double period);
  PIDCommand(double p, double i, double d);
  PIDCommand(double p, double i, double d, double period);
  PIDCommand(double p, double i, double d, double f, double period);
  virtual ~PIDCommand() = default;

  void SetSetpointRelative(double deltaSetpoint);

  // PIDOutput interface
  virtual void PIDWrite(double output);

  // PIDSource interface
  virtual double PIDGet();

 protected:
  std::shared_ptr<PIDController> GetPIDController() const;
  virtual void _Initialize();
  virtual void _Interrupted();
  virtual void _End();
  void SetSetpoint(double setpoint);
  double GetSetpoint() const;
  double GetPosition();

  virtual double ReturnPIDInput() = 0;
  virtual void UsePIDOutput(double output) = 0;

 private:
  /** The internal {@link PIDController} */
  std::shared_ptr<PIDController> m_controller;

 public:
  void InitTable(std::shared_ptr<ITable> subtable) override;
  std::string GetSmartDashboardType() const override;
};

}  // namespace frc

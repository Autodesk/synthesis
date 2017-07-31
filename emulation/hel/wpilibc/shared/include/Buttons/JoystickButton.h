/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#pragma once

#include "Buttons/Button.h"
#include "GenericHID.h"

namespace frc {

class JoystickButton : public Button {
 public:
  JoystickButton(GenericHID* joystick, int buttonNumber);
  virtual ~JoystickButton() = default;

  virtual bool Get();

 private:
  GenericHID* m_joystick;
  int m_buttonNumber;
};

}  // namespace frc

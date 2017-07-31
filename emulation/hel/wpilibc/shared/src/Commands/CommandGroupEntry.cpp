/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "Commands/CommandGroupEntry.h"

#include "Commands/Command.h"

using namespace frc;

CommandGroupEntry::CommandGroupEntry(Command* command, Sequence state,
                                     double timeout)
    : m_timeout(timeout), m_command(command), m_state(state) {}

bool CommandGroupEntry::IsTimedOut() const {
  if (m_timeout < 0.0) return false;
  double time = m_command->TimeSinceInitialized();
  if (time == 0.0) return false;
  return time >= m_timeout;
}

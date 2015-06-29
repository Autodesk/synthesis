/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Commands/CommandGroup.h"
#include "WPIErrors.h"

/**
 * Creates a new {@link CommandGroup CommandGroup}.
 */
CommandGroup::CommandGroup()
{
	m_currentCommandIndex = -1;
}

/**
 * Creates a new {@link CommandGroup CommandGroup} with the given name.
 * @param name the name for this command group
 */
CommandGroup::CommandGroup(const char *name) :
	Command(name)
{
	m_currentCommandIndex = -1;
}

CommandGroup::~CommandGroup()
{
}

/**
 * Adds a new {@link Command Command} to the group.  The {@link Command Command} will be started after
 * all the previously added {@link Command Commands}.
 *
 * <p>Note that any requirements the given {@link Command Command} has will be added to the
 * group.  For this reason, a {@link Command Command's} requirements can not be changed after
 * being added to a group.</p>
 *
 * <p>It is recommended that this method be called in the constructor.</p>
 * 
 * @param command The {@link Command Command} to be added
 */
void CommandGroup::AddSequential(Command *command)
{
	if (command == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "command");
		return;
	}
	if (!AssertUnlocked("Cannot add new command to command group"))
		return;

	command->SetParent(this);

	m_commands.push_back(CommandGroupEntry(command, CommandGroupEntry::kSequence_InSequence));
	// Iterate through command->GetRequirements() and call Requires() on each required subsystem
	Command::SubsystemSet requirements = command->GetRequirements();
	Command::SubsystemSet::iterator iter = requirements.begin();
	for (; iter != requirements.end(); iter++)
		Requires(*iter);
}

/**
 * Adds a new {@link Command Command} to the group with a given timeout.
 * The {@link Command Command} will be started after all the previously added commands.
 *
 * <p>Once the {@link Command Command} is started, it will be run until it finishes or the time
 * expires, whichever is sooner.  Note that the given {@link Command Command} will have no
 * knowledge that it is on a timer.</p>
 *
 * <p>Note that any requirements the given {@link Command Command} has will be added to the
 * group.  For this reason, a {@link Command Command's} requirements can not be changed after
 * being added to a group.</p>
 *
 * <p>It is recommended that this method be called in the constructor.</p>
 *
 * @param command The {@link Command Command} to be added
 * @param timeout The timeout (in seconds)
 */
void CommandGroup::AddSequential(Command *command, double timeout)
{
	if (command == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "command");
		return;
	}
	if (!AssertUnlocked("Cannot add new command to command group"))
		return;
	if (timeout < 0.0)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "timeout < 0.0");
		return;
	}

	command->SetParent(this);

	m_commands.push_back(CommandGroupEntry(command, CommandGroupEntry::kSequence_InSequence, timeout));
	// Iterate through command->GetRequirements() and call Requires() on each required subsystem
	Command::SubsystemSet requirements = command->GetRequirements();
	Command::SubsystemSet::iterator iter = requirements.begin();
	for (; iter != requirements.end(); iter++)
		Requires(*iter);
}

/**
 * Adds a new child {@link Command} to the group.  The {@link Command} will be started after
 * all the previously added {@link Command Commands}.
 *
 * <p>Instead of waiting for the child to finish, a {@link CommandGroup} will have it
 * run at the same time as the subsequent {@link Command Commands}.  The child will run until either
 * it finishes, a new child with conflicting requirements is started, or
 * the main sequence runs a {@link Command} with conflicting requirements.  In the latter
 * two cases, the child will be canceled even if it says it can't be
 * interrupted.</p>
 *
 * <p>Note that any requirements the given {@link Command Command} has will be added to the
 * group.  For this reason, a {@link Command Command's} requirements can not be changed after
 * being added to a group.</p>
 *
 * <p>It is recommended that this method be called in the constructor.</p>
 *
 * @param command The command to be added
 */
void CommandGroup::AddParallel(Command *command)
{
	if (command == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "command");
		return;
	}
	if (!AssertUnlocked("Cannot add new command to command group"))
		return;

	command->SetParent(this);

	m_commands.push_back(CommandGroupEntry(command, CommandGroupEntry::kSequence_BranchChild));
	// Iterate through command->GetRequirements() and call Requires() on each required subsystem
	Command::SubsystemSet requirements = command->GetRequirements();
	Command::SubsystemSet::iterator iter = requirements.begin();
	for (; iter != requirements.end(); iter++)
		Requires(*iter);
}

/**
 * Adds a new child {@link Command} to the group with the given timeout.  The {@link Command} will be started after
 * all the previously added {@link Command Commands}.
 *
 * <p>Once the {@link Command Command} is started, it will run until it finishes, is interrupted,
 * or the time expires, whichever is sooner.  Note that the given {@link Command Command} will have no
 * knowledge that it is on a timer.</p>
 *
 * <p>Instead of waiting for the child to finish, a {@link CommandGroup} will have it
 * run at the same time as the subsequent {@link Command Commands}.  The child will run until either
 * it finishes, the timeout expires, a new child with conflicting requirements is started, or
 * the main sequence runs a {@link Command} with conflicting requirements.  In the latter
 * two cases, the child will be canceled even if it says it can't be
 * interrupted.</p>
 *
 * <p>Note that any requirements the given {@link Command Command} has will be added to the
 * group.  For this reason, a {@link Command Command's} requirements can not be changed after
 * being added to a group.</p>
 *
 * <p>It is recommended that this method be called in the constructor.</p>
 *
 * @param command The command to be added
 * @param timeout The timeout (in seconds)
 */
void CommandGroup::AddParallel(Command *command, double timeout)
{
	if (command == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "command");
		return;
	}
	if (!AssertUnlocked("Cannot add new command to command group"))
		return;
	if (timeout < 0.0)
	{
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "timeout < 0.0");
		return;
	}

	command->SetParent(this);

	m_commands.push_back(CommandGroupEntry(command, CommandGroupEntry::kSequence_BranchChild, timeout));
	// Iterate through command->GetRequirements() and call Requires() on each required subsystem
	Command::SubsystemSet requirements = command->GetRequirements();
	Command::SubsystemSet::iterator iter = requirements.begin();
	for (; iter != requirements.end(); iter++)
		Requires(*iter);
}

void CommandGroup::_Initialize()
{
	m_currentCommandIndex = -1;
}

void CommandGroup::_Execute()
{
	CommandGroupEntry entry;
	Command *cmd = NULL;
	bool firstRun = false;

	if (m_currentCommandIndex == -1)
	{
		firstRun = true;
		m_currentCommandIndex = 0;
	}

	while ((unsigned)m_currentCommandIndex < m_commands.size())
	{
		if (cmd != NULL)
		{
			if (entry.IsTimedOut())
				cmd->_Cancel();

			if (cmd->Run())
			{
				break;
			}
			else
			{
				cmd->Removed();
				m_currentCommandIndex++;
				firstRun = true;
				cmd = NULL;
				continue;
			}
		}

		entry = m_commands[m_currentCommandIndex];
		cmd = NULL;

		switch (entry.m_state)
		{
			case CommandGroupEntry::kSequence_InSequence:
				cmd = entry.m_command;
				if (firstRun)
				{
					cmd->StartRunning();
					CancelConflicts(cmd);
					firstRun = false;
				}
				break;

			case CommandGroupEntry::kSequence_BranchPeer:
				m_currentCommandIndex++;
				entry.m_command->Start();
				break;

			case CommandGroupEntry::kSequence_BranchChild:
				m_currentCommandIndex++;
				CancelConflicts(entry.m_command);
				entry.m_command->StartRunning();
				m_children.push_back(entry);
				break;
		}
	}

	// Run Children
	CommandList::iterator iter = m_children.begin();
	for (; iter != m_children.end();)
	{
		entry = *iter;
		Command *child = entry.m_command;
		if (entry.IsTimedOut())
			child->_Cancel();

		if (!child->Run())
		{
			child->Removed();
			iter = m_children.erase(iter);
		}
		else
		{
			iter++;
		}
	}
}

void CommandGroup::_End()
{
	// Theoretically, we don't have to check this, but we do if teams override the IsFinished method
	if (m_currentCommandIndex != -1 && (unsigned)m_currentCommandIndex < m_commands.size())
	{
		Command *cmd = m_commands[m_currentCommandIndex].m_command;
		cmd->_Cancel();
		cmd->Removed();
	}

	CommandList::iterator iter = m_children.begin();
	for (; iter != m_children.end(); iter++)
	{
		Command *cmd = iter->m_command;
		cmd->_Cancel();
		cmd->Removed();
	}
	m_children.clear();
}

void CommandGroup::_Interrupted()
{
	_End();
}

// Can be overwritten by teams
void CommandGroup::Initialize()
{
}

// Can be overwritten by teams
void CommandGroup::Execute()
{
}

// Can be overwritten by teams
void CommandGroup::End()
{
}

// Can be overwritten by teams
void CommandGroup::Interrupted()
{
}

bool CommandGroup::IsFinished()
{
	return (unsigned)m_currentCommandIndex >= m_commands.size() && m_children.empty();
}

bool CommandGroup::IsInterruptible()
{
	if (!Command::IsInterruptible())
		return false;

	if (m_currentCommandIndex != -1 && (unsigned)m_currentCommandIndex < m_commands.size())
	{
		Command *cmd = m_commands[m_currentCommandIndex].m_command;
		if (!cmd->IsInterruptible())
			return false;
	}

	CommandList::iterator iter = m_children.begin();
	for (; iter != m_children.end(); iter++)
	{
		if (!iter->m_command->IsInterruptible())
			return false;
	}

	return true;
}

void CommandGroup::CancelConflicts(Command *command)
{
	CommandList::iterator childIter = m_children.begin();
	for (; childIter != m_children.end();)
	{
		Command *child = childIter->m_command;
		bool erased = false;

		Command::SubsystemSet requirements = command->GetRequirements();
		Command::SubsystemSet::iterator requirementIter = requirements.begin();
		for (; requirementIter != requirements.end(); requirementIter++)
		{
			if (child->DoesRequire(*requirementIter))
			{
				child->_Cancel();
				child->Removed();
				childIter = m_children.erase(childIter);
				erased = true;
				break;
			}
		}
		if (!erased)
			childIter++;
	}
}

int CommandGroup::GetSize()
{
	return m_children.size();
}

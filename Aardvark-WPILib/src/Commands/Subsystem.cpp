/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Commands/Subsystem.h"

#include "Commands/Command.h"
#include "Commands/Scheduler.h"
#include "WPIErrors.h"

/**
 * Creates a subsystem with the given name
 * @param name the name of the subsystem
 */
Subsystem::Subsystem(const char *name) :
	m_currentCommand(NULL),
	m_defaultCommand(NULL),
	m_initializedDefaultCommand(false)
{
	m_name = name;
	Scheduler::GetInstance()->RegisterSubsystem(this);
	m_table = NULL;
	m_currentCommandChanged = true;
}
/**
 * Initialize the default command for this subsystem
 * This is meant to be the place to call SetDefaultCommand in a subsystem and will be called
 * on all the subsystems by the CommandBase method before the program starts running by using
 * the list of all registered Subsystems inside the Scheduler.
 * 
 * This should be overridden by a Subsystem that has a default Command
 */
void Subsystem::InitDefaultCommand() {

}

/**
 * Sets the default command.  If this is not called or is called with null,
 * then there will be no default command for the subsystem.
 *
 * <p><b>WARNING:</b> This should <b>NOT</b> be called in a constructor if the subsystem is a
 * singleton.</p>
 *
 * @param command the default command (or null if there should be none)
 */
void Subsystem::SetDefaultCommand(Command *command)
{
	if (command == NULL)
	{
		m_defaultCommand = NULL;
	}
	else
	{
		bool found = false;
		Command::SubsystemSet requirements = command->GetRequirements();
		Command::SubsystemSet::iterator iter = requirements.begin();
		for (; iter != requirements.end(); iter++)
		{
			if (*iter == this)
			{
				found = true;
				break;
			}
		}

		if (!found)
		{
			wpi_setWPIErrorWithContext(CommandIllegalUse, "A default command must require the subsystem");
			return;
		}
		
		m_defaultCommand = command;
	}
	if (m_table != NULL)
	{
		if (m_defaultCommand != NULL)
		{
			m_table->PutBoolean("hasDefault", true);
			m_table->PutString("default", m_defaultCommand->GetName());
		}
		else
		{
			m_table->PutBoolean("hasDefault", false);
		}
	}
}

/**
 * Returns the default command (or null if there is none).
 * @return the default command
 */
Command *Subsystem::GetDefaultCommand()
{
	if (!m_initializedDefaultCommand) {
		m_initializedDefaultCommand = true;
		InitDefaultCommand();
	}
	return m_defaultCommand;
}

/**
 * Sets the current command
 * @param command the new current command
 */
void Subsystem::SetCurrentCommand(Command *command)
{
	m_currentCommand = command;
	m_currentCommandChanged = true;
}

/**
 * Returns the command which currently claims this subsystem.
 * @return the command which currently claims this subsystem
 */
Command *Subsystem::GetCurrentCommand()
{
	return m_currentCommand;
}

/**
 * Call this to alert Subsystem that the current command is actually the command.
 * Sometimes, the {@link Subsystem} is told that it has no command while the {@link Scheduler}
 * is going through the loop, only to be soon after given a new one.  This will avoid that situation.
 */
void Subsystem::ConfirmCommand()
{
	if (m_currentCommandChanged) {
		if (m_table != NULL)
		{
			if (m_currentCommand != NULL)
			{
				m_table->PutBoolean("hasCommand", true);
				m_table->PutString("command", m_currentCommand->GetName());
			}
			else
			{
				m_table->PutBoolean("hasCommand", false);
			}
		}
		m_currentCommandChanged = false;
	}
}



std::string Subsystem::GetName()
{
	return m_name;
}

std::string Subsystem::GetSmartDashboardType()
{
	return "Subsystem";
}

void Subsystem::InitTable(ITable* table)
{
    m_table = table;
    if(m_table!=NULL){
        if (m_defaultCommand != NULL) {
        	m_table->PutBoolean("hasDefault", true);
        	m_table->PutString("default", m_defaultCommand->GetName());
        } else {
        	m_table->PutBoolean("hasDefault", false);
        }
        if (m_currentCommand != NULL) {
        	m_table->PutBoolean("hasCommand", true);
            m_table->PutString("command", m_currentCommand->GetName());
        } else {
        	m_table->PutBoolean("hasCommand", false);
        }
    }
}

ITable* Subsystem::GetTable(){
	return m_table;
}

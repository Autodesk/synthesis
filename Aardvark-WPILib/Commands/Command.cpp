/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Commands/Command.h"
#include "Commands/CommandGroup.h"
#include "Commands/Scheduler.h"
#include "DriverStation.h"
#include "Timer.h"
#include "WPIErrors.h"

static const char *kName = "name";
static const char *kRunning = "running";
static const char *kIsParented = "isParented";

int Command::m_commandCounter = 0;

void Command::InitCommand(const char *name, double timeout)
{
	m_commandID = m_commandCounter++;
	m_timeout = timeout;
	m_locked = false;
	m_startTime = -1;
	m_initialized = false;
	m_running = false;
	m_interruptible = true;
	m_canceled = false;
	m_runWhenDisabled = false;
	m_parent = NULL;
	if (name == NULL)
	{
		// Don't have a way to find the subclass name like java, so use the address
		char buf[32];
		snprintf(buf, 32, "Command_%p", this);
		m_name = buf;
	}
	else
	{
		m_name = name;
	}
	m_table = NULL;	
}

/**
 * Creates a new command.
 * The name of this command will be default.
 */
Command::Command()
{
	InitCommand(NULL, -1.0);
}

/**
 * Creates a new command with the given name and no timeout.
 * @param name the name for this command
 */
Command::Command(const char *name)
{
	if (name == NULL)
		wpi_setWPIErrorWithContext(NullParameter, "name");
	InitCommand(name, -1.0);
}

/**
 * Creates a new command with the given timeout and a default name.
 * @param timeout the time (in seconds) before this command "times out"
 * @see Command#isTimedOut() isTimedOut()
 */
Command::Command(double timeout)
{
	if (timeout < 0.0)
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "timeout < 0.0");
	InitCommand(NULL, timeout);
}

/**
 * Creates a new command with the given name and timeout.
 * @param name the name of the command
 * @param timeout the time (in seconds) before this command "times out"
 * @see Command#isTimedOut() isTimedOut()
 */
Command::Command(const char *name, double timeout)
{
	if (name == NULL)
		wpi_setWPIErrorWithContext(NullParameter, "name");
	if (timeout < 0.0)
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "timeout < 0.0");
	InitCommand(name, timeout);
}

Command::~Command()
{//TODO deal with cleaning up all listeners
	/*if (m_table != NULL){
		m_table->RemoveChangeListener(kRunning, this);
	}*/
}

/**
 * Get the ID (sequence number) for this command
 * The ID is a unique sequence number that is incremented for each command.
 * @return the ID of this command
 */
int Command::GetID() {
	return m_commandID;
}

/**
 * Sets the timeout of this command.
 * @param timeout the timeout (in seconds)
 * @see Command#isTimedOut() isTimedOut()
 */
void Command::SetTimeout(double timeout)
{
	if (timeout < 0.0)
		wpi_setWPIErrorWithContext(ParameterOutOfRange, "timeout < 0.0");
	else
		m_timeout = timeout;
}

/**
 * Returns the time since this command was initialized (in seconds).
 * This function will work even if there is no specified timeout.
 * @return the time since this command was initialized (in seconds).
 */
double Command::TimeSinceInitialized()
{
	if (m_startTime < 0.0)
		return 0.0;
	else
		return Timer::GetFPGATimestamp() - m_startTime;
}

/**
 * This method specifies that the given {@link Subsystem} is used by this command.
 * This method is crucial to the functioning of the Command System in general.
 *
 * <p>Note that the recommended way to call this method is in the constructor.</p>
 *
 * @param subsystem the {@link Subsystem} required
 * @see Subsystem
 */
void Command::Requires(Subsystem *subsystem)
{
	if (!AssertUnlocked("Can not add new requirement to command"))
		return;

	if (subsystem != NULL)
		m_requirements.insert(subsystem);
	else
		wpi_setWPIErrorWithContext(NullParameter, "subsystem");
}

/**
 * Called when the command has been removed.
 * This will call {@link Command#interrupted() interrupted()} or {@link Command#end() end()}.
 */
void Command::Removed()
{
	if (m_initialized)
	{
		if (IsCanceled())
		{
			Interrupted();
			_Interrupted();
		}
		else
		{
			End();
			_End();
		}
	}
	m_initialized = false;
	m_canceled = false;
	m_running = false;
	if (m_table != NULL)
		m_table->PutBoolean(kRunning, false);
}

/**
 * Starts up the command.  Gets the command ready to start.
 * <p>Note that the command will eventually start, however it will not necessarily
 * do so immediately, and may in fact be canceled before initialize is even called.</p>
 */
void Command::Start()
{
	LockChanges();
	if (m_parent != NULL)
		wpi_setWPIErrorWithContext(CommandIllegalUse, "Can not start a command that is part of a command group");

	Scheduler::GetInstance()->AddCommand(this);
}

/**
 * The run method is used internally to actually run the commands.
 * @return whether or not the command should stay within the {@link Scheduler}.
 */
bool Command::Run()
{
	if (!m_runWhenDisabled && m_parent == NULL && DriverStation::GetInstance()->IsDisabled())
		Cancel();

	if (IsCanceled())
		return false;

	if (!m_initialized)
	{
		m_initialized = true;
		StartTiming();
		_Initialize();
		Initialize();
	}
	_Execute();
	Execute();
	return !IsFinished();
}

void Command::_Initialize()
{
}

void Command::_Interrupted()
{
}

void Command::_Execute()
{
}

void Command::_End()
{
}

/**
 * Called to indicate that the timer should start.
 * This is called right before {@link Command#initialize() initialize()} is, inside the
 * {@link Command#run() run()} method.
 */
void Command::StartTiming()
{
	m_startTime = Timer::GetFPGATimestamp();
}

/**
 * Returns whether or not the {@link Command#timeSinceInitialized() timeSinceInitialized()}
 * method returns a number which is greater than or equal to the timeout for the command.
 * If there is no timeout, this will always return false.
 * @return whether the time has expired
 */
bool Command::IsTimedOut()
{
	return m_timeout != -1 && TimeSinceInitialized() >= m_timeout;
}

/**
 * Returns the requirements (as an std::set of {@link Subsystem Subsystems} pointers) of this command
 * @return the requirements (as an std::set of {@link Subsystem Subsystems} pointers) of this command
 */
Command::SubsystemSet Command::GetRequirements()
{
	return m_requirements;
}

/**
 * Prevents further changes from being made
 */
void Command::LockChanges()
{
	m_locked = true;
}

/**
 * If changes are locked, then this will generate a CommandIllegalUse error.
 * @param message the message to report on error (it is appended by a default message)
 * @return true if assert passed, false if assert failed
 */
bool Command::AssertUnlocked(const char *message)
{
	if (m_locked)
	{
		char buf[128];
		snprintf(buf, 128, "%s after being started or being added to a command group", message);
		wpi_setWPIErrorWithContext(CommandIllegalUse, buf);
		return false;
	}
	return true;
}

/**
 * Sets the parent of this command.  No actual change is made to the group.
 * @param parent the parent
 */
void Command::SetParent(CommandGroup *parent)
{
	if (parent == NULL)
	{
		wpi_setWPIErrorWithContext(NullParameter, "parent");
	}
	else if (m_parent != NULL)
	{
		wpi_setWPIErrorWithContext(CommandIllegalUse, "Can not give command to a command group after already being put in a command group");
	}
	else
	{
		LockChanges();
		m_parent = parent;
		if (m_table != NULL)
		{
			m_table->PutBoolean(kIsParented, true);
		}
	}
}

/**
 * This is used internally to mark that the command has been started.
 * The lifecycle of a command is:
 *
 * startRunning() is called.
 * run() is called (multiple times potentially)
 * removed() is called
 *
 * It is very important that startRunning and removed be called in order or some assumptions
 * of the code will be broken.
 */
void Command::StartRunning()
{
	m_running = true;
	m_startTime = -1;
	if (m_table != NULL)
		m_table->PutBoolean(kRunning, true);
}

/**
 * Returns whether or not the command is running.
 * This may return true even if the command has just been canceled, as it may
 * not have yet called {@link Command#interrupted()}.
 * @return whether or not the command is running
 */
bool Command::IsRunning()
{
	return m_running;
}

/**
 * This will cancel the current command.
 * <p>This will cancel the current command eventually.  It can be called multiple times.
 * And it can be called when the command is not running.  If the command is running though,
 * then the command will be marked as canceled and eventually removed.</p>
 * <p>A command can not be canceled
 * if it is a part of a command group, you must cancel the command group instead.</p>
 */
void Command::Cancel()
{
	if (m_parent != NULL)
		wpi_setWPIErrorWithContext(CommandIllegalUse, "Can not cancel a command that is part of a command group");

	_Cancel();
}

/**
 * This works like cancel(), except that it doesn't throw an exception if it is a part
 * of a command group.  Should only be called by the parent command group.
 */
void Command::_Cancel()
{
	if (IsRunning())
		m_canceled = true;
}

/**
 * Returns whether or not this has been canceled.
 * @return whether or not this has been canceled
 */
bool Command::IsCanceled()
{
	return m_canceled;
}

/**
 * Returns whether or not this command can be interrupted.
 * @return whether or not this command can be interrupted
 */
bool Command::IsInterruptible()
{
	return m_interruptible;
}

/**
 * Sets whether or not this command can be interrupted.
 * @param interruptible whether or not this command can be interrupted
 */
void Command::SetInterruptible(bool interruptible)
{
	m_interruptible = interruptible;
}

/**
 * Checks if the command requires the given {@link Subsystem}.
 * @param system the system
 * @return whether or not the subsystem is required (false if given NULL)
 */
bool Command::DoesRequire(Subsystem *system)
{
	return m_requirements.count(system) > 0;
}

/**
 * Returns the {@link CommandGroup} that this command is a part of.
 * Will return null if this {@link Command} is not in a group.
 * @return the {@link CommandGroup} that this command is a part of (or null if not in group)
 */
CommandGroup *Command::GetGroup()
{
	return m_parent;
}

/**
 * Sets whether or not this {@link Command} should run when the robot is disabled.
 *
 * <p>By default a command will not run when the robot is disabled, and will in fact be canceled.</p>
 * @param run whether or not this command should run when the robot is disabled
 */
void Command::SetRunWhenDisabled(bool run)
{
	m_runWhenDisabled = run;
}

/**
 * Returns whether or not this {@link Command} will run when the robot is disabled, or if it will cancel itself.
 * @return whether or not this {@link Command} will run when the robot is disabled, or if it will cancel itself
 */
bool Command::WillRunWhenDisabled()
{
	return m_runWhenDisabled;
}

std::string Command::GetName()
{
	return m_name;
}

std::string Command::GetSmartDashboardType()
{
	return "Command";
}

void Command::InitTable(ITable* table)
{
    if(m_table!=NULL)
    	m_table->RemoveTableListener(this);
    m_table = table;
    if(m_table!=NULL){
    	m_table->PutString(kName, GetName());
    	m_table->PutBoolean(kRunning, IsRunning());
    	m_table->PutBoolean(kIsParented, m_parent != NULL);
    	m_table->AddTableListener(kRunning, this, false);
    }
}

ITable* Command::GetTable(){
	return m_table;
}

void Command::ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew)
{
	if (value.b){
		if(!IsRunning())
			Start();
	}
	else{
		if(IsRunning())
			Cancel();
	}
}

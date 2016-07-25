/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __COMMAND_H__
#define __COMMAND_H__

#include "ErrorBase.h"
#include "SmartDashboard/NamedSendable.h"
#include <set>
#include <string>

class CommandGroup;
class Subsystem;

/**
 * The Command class is at the very core of the entire command framework.
 * Every command can be started with a call to {@link Command#Start() Start()}.
 * Once a command is started it will call {@link Command#Initialize() Initialize()}, and then
 * will repeatedly call {@link Command#Execute() Execute()} until the {@link Command#IsFinished() IsFinished()}
 * returns true.  Once it does, {@link Command#End() End()} will be called.
 *
 * <p>However, if at any point while it is running {@link Command#Cancel() Cancel()} is called, then
 * the command will be stopped and {@link Command#Interrupted() Interrupted()} will be called.</p>
 *
 * <p>If a command uses a {@link Subsystem}, then it should specify that it does so by
 * calling the {@link Command#Requires(Subsystem) Requires(...)} method
 * in its constructor. Note that a Command may have multiple requirements, and
 * {@link Command#Requires(Subsystem) Requires(...)} should be
 * called for each one.</p>
 *
 * <p>If a command is running and a new command with shared requirements is started,
 * then one of two things will happen.  If the active command is interruptible,
 * then {@link Command#Cancel() Cancel()} will be called and the command will be removed
 * to make way for the new one.  If the active command is not interruptible, the
 * other one will not even be started, and the active one will continue functioning.</p>
 *
 * @see CommandGroup
 * @see Subsystem
 */
class Command : public ErrorBase, public NamedSendable, public ITableListener
{
	friend class CommandGroup;
	friend class Scheduler;
public:
	Command();
	Command(const char *name);
	Command(double timeout);
	Command(const char *name, double timeout);
	virtual ~Command();
	double TimeSinceInitialized();
	void Requires(Subsystem *s);
	bool IsCanceled();
	void Start();
	bool Run();
	void Cancel();
	bool IsRunning();
	bool IsInterruptible();
	void SetInterruptible(bool interruptible);
	bool DoesRequire(Subsystem *subsystem);
	typedef std::set<Subsystem *> SubsystemSet;
	SubsystemSet GetRequirements();
	CommandGroup *GetGroup();
	void SetRunWhenDisabled(bool run);
	bool WillRunWhenDisabled();
	int GetID();


protected:
	void SetTimeout(double timeout);
	bool IsTimedOut();
	bool AssertUnlocked(const char *message);
	void SetParent(CommandGroup *parent);
	/**
	 * The initialize method is called the first time this Command is run after 
	 * being started.
	 */
	virtual void Initialize() = 0;
	/**
	 * The execute method is called repeatedly until this Command either finishes
	 * or is canceled.
	 */
	virtual void Execute() = 0;
	/**
	 * Returns whether this command is finished.
	 * If it is, then the command will be removed
	 * and {@link Command#end() end()} will be called.
	 *
	 * <p>It may be useful for a team to reference the {@link Command#isTimedOut() isTimedOut()} method
	 * for time-sensitive commands.</p>
	 * @return whether this command is finished.
	 * @see Command#isTimedOut() isTimedOut()
	 */
	virtual bool IsFinished() = 0;
	/**
	 * Called when the command ended peacefully.  This is where you may want
	 * to wrap up loose ends, like shutting off a motor that was being used
	 * in the command.
	 */
	virtual void End() = 0;
	/**
	 * Called when the command ends because somebody called {@link Command#cancel() cancel()}
	 * or another command shared the same requirements as this one, and booted
	 * it out.
	 *
	 * <p>This is where you may want
	 * to wrap up loose ends, like shutting off a motor that was being used
	 * in the command.</p>
	 *
	 * <p>Generally, it is useful to simply call the {@link Command#end() end()} method
	 * within this method</p>
	 */
	virtual void Interrupted() = 0;
	virtual void _Initialize();
	virtual void _Interrupted();
	virtual void _Execute();
	virtual void _End();
	virtual void _Cancel();

private:
	void InitCommand(const char *name, double timeout);
	void LockChanges();
	/*synchronized*/ void Removed();
	void StartRunning();
	void StartTiming();

	/** The name of this command */
	std::string m_name;
	/** The time since this command was initialized */
	double m_startTime;
	/** The time (in seconds) before this command "times out" (or -1 if no timeout) */
	double m_timeout;
	/** Whether or not this command has been initialized */
	bool m_initialized;
	/** The requirements (or null if no requirements) */
	SubsystemSet m_requirements;
	/** Whether or not it is running */
	bool m_running;
	/** Whether or not it is interruptible*/
	bool m_interruptible;
	/** Whether or not it has been canceled */
	bool m_canceled;
	/** Whether or not it has been locked */
	bool m_locked;
	/** Whether this command should run when the robot is disabled */
	bool m_runWhenDisabled;
	/** The {@link CommandGroup} this is in */
	CommandGroup *m_parent;
	int m_commandID;
	static int m_commandCounter;
	
public:
	virtual std::string GetName();
	virtual void InitTable(ITable* table);
	virtual ITable* GetTable();
	virtual std::string GetSmartDashboardType();
	virtual void ValueChanged(ITable* source, const std::string& key, EntryValue value, bool isNew);
protected:
	ITable* m_table;
};

#endif

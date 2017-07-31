/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.command;

import java.util.Enumeration;
import java.util.NoSuchElementException;

import edu.wpi.first.wpilibj.NamedSendable;
import edu.wpi.first.wpilibj.RobotState;
import edu.wpi.first.wpilibj.Timer;
import edu.wpi.first.wpilibj.tables.ITable;
import edu.wpi.first.wpilibj.tables.ITableListener;

/**
 * The Command class is at the very core of the entire command framework. Every command can be
 * started with a call to {@link Command#start() start()}. Once a command is started it will call
 * {@link Command#initialize() initialize()}, and then will repeatedly call {@link Command#execute()
 * execute()} until the {@link Command#isFinished() isFinished()} returns true. Once it does, {@link
 * Command#end() end()} will be called.
 *
 * <p>However, if at any point while it is running {@link Command#cancel() cancel()} is called,
 * then the command will be stopped and {@link Command#interrupted() interrupted()} will be called.
 *
 * <p>If a command uses a {@link Subsystem}, then it should specify that it does so by calling the
 * {@link Command#requires(Subsystem) requires(...)} method in its constructor. Note that a Command
 * may have multiple requirements, and {@link Command#requires(Subsystem) requires(...)} should be
 * called for each one.
 *
 * <p>If a command is running and a new command with shared requirements is started, then one of
 * two things will happen. If the active command is interruptible, then {@link Command#cancel()
 * cancel()} will be called and the command will be removed to make way for the new one. If the
 * active command is not interruptible, the other one will not even be started, and the active one
 * will continue functioning.
 *
 * @see Subsystem
 * @see CommandGroup
 * @see IllegalUseOfCommandException
 */
public abstract class Command implements NamedSendable {

  /**
   * The name of this command.
   */
  private String m_name;
  /**
   * The time since this command was initialized.
   */
  private double m_startTime = -1;
  /**
   * The time (in seconds) before this command "times out" (or -1 if no timeout).
   */
  private double m_timeout = -1;
  /**
   * Whether or not this command has been initialized.
   */
  private boolean m_initialized = false;
  /**
   * The requirements (or null if no requirements).
   */
  private Set m_requirements;
  /**
   * Whether or not it is running.
   */
  private boolean m_running = false;
  /**
   * Whether or not it is interruptible.
   */
  private boolean m_interruptible = true;
  /**
   * Whether or not it has been canceled.
   */
  private boolean m_canceled = false;
  /**
   * Whether or not it has been locked.
   */
  private boolean m_locked = false;
  /**
   * Whether this command should run when the robot is disabled.
   */
  private boolean m_runWhenDisabled = false;
  /**
   * The {@link CommandGroup} this is in.
   */
  private CommandGroup m_parent;

  /**
   * Creates a new command. The name of this command will be set to its class name.
   */
  public Command() {
    m_name = getClass().getName();
    m_name = m_name.substring(m_name.lastIndexOf('.') + 1);
  }

  /**
   * Creates a new command with the given name.
   *
   * @param name the name for this command
   * @throws IllegalArgumentException if name is null
   */
  public Command(String name) {
    if (name == null) {
      throw new IllegalArgumentException("Name must not be null.");
    }
    m_name = name;
  }

  /**
   * Creates a new command with the given timeout and a default name. The default name is the name
   * of the class.
   *
   * @param timeout the time (in seconds) before this command "times out"
   * @throws IllegalArgumentException if given a negative timeout
   * @see Command#isTimedOut() isTimedOut()
   */
  public Command(double timeout) {
    this();
    if (timeout < 0) {
      throw new IllegalArgumentException("Timeout must not be negative.  Given:" + timeout);
    }
    m_timeout = timeout;
  }

  /**
   * Creates a new command with the given name and timeout.
   *
   * @param name    the name of the command
   * @param timeout the time (in seconds) before this command "times out"
   * @throws IllegalArgumentException if given a negative timeout or name was null.
   * @see Command#isTimedOut() isTimedOut()
   */
  public Command(String name, double timeout) {
    this(name);
    if (timeout < 0) {
      throw new IllegalArgumentException("Timeout must not be negative.  Given:" + timeout);
    }
    m_timeout = timeout;
  }

  /**
   * Returns the name of this command. If no name was specified in the constructor, then the default
   * is the name of the class.
   *
   * @return the name of this command
   */
  public String getName() {
    return m_name;
  }

  /**
   * Sets the timeout of this command.
   *
   * @param seconds the timeout (in seconds)
   * @throws IllegalArgumentException if seconds is negative
   * @see Command#isTimedOut() isTimedOut()
   */
  protected final synchronized void setTimeout(double seconds) {
    if (seconds < 0) {
      throw new IllegalArgumentException("Seconds must be positive.  Given:" + seconds);
    }
    m_timeout = seconds;
  }

  /**
   * Returns the time since this command was initialized (in seconds). This function will work even
   * if there is no specified timeout.
   *
   * @return the time since this command was initialized (in seconds).
   */
  public final synchronized double timeSinceInitialized() {
    return m_startTime < 0 ? 0 : Timer.getFPGATimestamp() - m_startTime;
  }

  /**
   * This method specifies that the given {@link Subsystem} is used by this command. This method is
   * crucial to the functioning of the Command System in general.
   *
   * <p>Note that the recommended way to call this method is in the constructor.
   *
   * @param subsystem the {@link Subsystem} required
   * @throws IllegalArgumentException     if subsystem is null
   * @throws IllegalUseOfCommandException if this command has started before or if it has been given
   *                                      to a {@link CommandGroup}
   * @see Subsystem
   */
  protected synchronized void requires(Subsystem subsystem) {
    validate("Can not add new requirement to command");
    if (subsystem != null) {
      if (m_requirements == null) {
        m_requirements = new Set();
      }
      m_requirements.add(subsystem);
    } else {
      throw new IllegalArgumentException("Subsystem must not be null.");
    }
  }

  /**
   * Called when the command has been removed. This will call {@link Command#interrupted()
   * interrupted()} or {@link Command#end() end()}.
   */
  synchronized void removed() {
    if (m_initialized) {
      if (isCanceled()) {
        interrupted();
        _interrupted();
      } else {
        end();
        _end();
      }
    }
    m_initialized = false;
    m_canceled = false;
    m_running = false;
    if (m_table != null) {
      m_table.putBoolean("running", false);
    }
  }

  /**
   * The run method is used internally to actually run the commands.
   *
   * @return whether or not the command should stay within the {@link Scheduler}.
   */
  synchronized boolean run() {
    if (!m_runWhenDisabled && m_parent == null && RobotState.isDisabled()) {
      cancel();
    }
    if (isCanceled()) {
      return false;
    }
    if (!m_initialized) {
      m_initialized = true;
      startTiming();
      _initialize();
      initialize();
    }
    _execute();
    execute();
    return !isFinished();
  }

  /**
   * The initialize method is called the first time this Command is run after being started.
   */
  protected void initialize() {}

  /**
   * A shadow method called before {@link Command#initialize() initialize()}.
   */
  @SuppressWarnings("MethodName")
  void _initialize() {
  }

  /**
   * The execute method is called repeatedly until this Command either finishes or is canceled.
   */
  @SuppressWarnings("MethodName")
  protected void execute() {}

  /**
   * A shadow method called before {@link Command#execute() execute()}.
   */
  @SuppressWarnings("MethodName")
  void _execute() {
  }

  /**
   * Returns whether this command is finished. If it is, then the command will be removed and {@link
   * Command#end() end()} will be called.
   *
   * <p>It may be useful for a team to reference the {@link Command#isTimedOut() isTimedOut()}
   * method for time-sensitive commands.
   *
   * <p>Returning false will result in the command never ending automatically. It may still be
   * cancelled manually or interrupted by another command. Returning true will result in the
   * command executing once and finishing immediately. We recommend using {@link InstantCommand}
   * for this.
   *
   * @return whether this command is finished.
   * @see Command#isTimedOut() isTimedOut()
   */
  protected abstract boolean isFinished();

  /**
   * Called when the command ended peacefully. This is where you may want to wrap up loose ends,
   * like shutting off a motor that was being used in the command.
   */
  protected void end() {}

  /**
   * A shadow method called after {@link Command#end() end()}.
   */
  @SuppressWarnings("MethodName")
  void _end() {
  }

  /**
   * Called when the command ends because somebody called {@link Command#cancel() cancel()} or
   * another command shared the same requirements as this one, and booted it out.
   *
   * <p>This is where you may want to wrap up loose ends, like shutting off a motor that was being
   * used in the command.
   *
   * <p>Generally, it is useful to simply call the {@link Command#end() end()} method within this
   * method, as done here.
   */
  protected void interrupted() {
    end();
  }

  /**
   * A shadow method called after {@link Command#interrupted() interrupted()}.
   */
  @SuppressWarnings("MethodName")
  void _interrupted() {}

  /**
   * Called to indicate that the timer should start. This is called right before {@link
   * Command#initialize() initialize()} is, inside the {@link Command#run() run()} method.
   */
  private void startTiming() {
    m_startTime = Timer.getFPGATimestamp();
  }

  /**
   * Returns whether or not the {@link Command#timeSinceInitialized() timeSinceInitialized()} method
   * returns a number which is greater than or equal to the timeout for the command. If there is no
   * timeout, this will always return false.
   *
   * @return whether the time has expired
   */
  protected synchronized boolean isTimedOut() {
    return m_timeout != -1 && timeSinceInitialized() >= m_timeout;
  }

  /**
   * Returns the requirements (as an {@link Enumeration Enumeration} of {@link Subsystem
   * Subsystems}) of this command.
   *
   * @return the requirements (as an {@link Enumeration Enumeration} of {@link Subsystem
   * Subsystems}) of this command
   */
  synchronized Enumeration getRequirements() {
    return m_requirements == null ? emptyEnumeration : m_requirements.getElements();
  }

  /**
   * Prevents further changes from being made.
   */
  synchronized void lockChanges() {
    m_locked = true;
  }

  /**
   * If changes are locked, then this will throw an {@link IllegalUseOfCommandException}.
   *
   * @param message the message to say (it is appended by a default message)
   */
  synchronized void validate(String message) {
    if (m_locked) {
      throw new IllegalUseOfCommandException(message
          + " after being started or being added to a command group");
    }
  }

  /**
   * Sets the parent of this command. No actual change is made to the group.
   *
   * @param parent the parent
   * @throws IllegalUseOfCommandException if this {@link Command} already is already in a group
   */
  synchronized void setParent(CommandGroup parent) {
    if (m_parent != null) {
      throw new IllegalUseOfCommandException(
          "Can not give command to a command group after already being put in a command group");
    }
    lockChanges();
    m_parent = parent;
    if (m_table != null) {
      m_table.putBoolean("isParented", true);
    }
  }

  /**
   * Clears list of subsystem requirements. This is only used by
   * {@link ConditionalCommand} so cancelling the chosen command works properly
   * in {@link CommandGroup}.
   */
  protected void clearRequirements() {
    m_requirements = new Set();
  }

  /**
   * Starts up the command. Gets the command ready to start. <p> Note that the command will
   * eventually start, however it will not necessarily do so immediately, and may in fact be
   * canceled before initialize is even called. </p>
   *
   * @throws IllegalUseOfCommandException if the command is a part of a CommandGroup
   */
  public synchronized void start() {
    lockChanges();
    if (m_parent != null) {
      throw new IllegalUseOfCommandException(
          "Can not start a command that is a part of a command group");
    }
    Scheduler.getInstance().add(this);
  }

  /**
   * This is used internally to mark that the command has been started. The lifecycle of a command
   * is:
   *
   * <p>startRunning() is called. run() is called (multiple times potentially) removed() is called.
   *
   * <p>It is very important that startRunning and removed be called in order or some assumptions of
   * the code will be broken.
   */
  synchronized void startRunning() {
    m_running = true;
    m_startTime = -1;
    if (m_table != null) {
      m_table.putBoolean("running", true);
    }
  }

  /**
   * Returns whether or not the command is running. This may return true even if the command has
   * just been canceled, as it may not have yet called {@link Command#interrupted()}.
   *
   * @return whether or not the command is running
   */
  public synchronized boolean isRunning() {
    return m_running;
  }

  /**
   * This will cancel the current command. <p> This will cancel the current command eventually. It
   * can be called multiple times. And it can be called when the command is not running. If the
   * command is running though, then the command will be marked as canceled and eventually removed.
   * </p> <p> A command can not be canceled if it is a part of a command group, you must cancel the
   * command group instead. </p>
   *
   * @throws IllegalUseOfCommandException if this command is a part of a command group
   */
  public synchronized void cancel() {
    if (m_parent != null) {
      throw new IllegalUseOfCommandException("Can not manually cancel a command in a command "
          + "group");
    }
    _cancel();
  }

  /**
   * This works like cancel(), except that it doesn't throw an exception if it is a part of a
   * command group. Should only be called by the parent command group.
   */
  @SuppressWarnings("MethodName")
  synchronized void _cancel() {
    if (isRunning()) {
      m_canceled = true;
    }
  }

  /**
   * Returns whether or not this has been canceled.
   *
   * @return whether or not this has been canceled
   */
  public synchronized boolean isCanceled() {
    return m_canceled;
  }

  /**
   * Returns whether or not this command can be interrupted.
   *
   * @return whether or not this command can be interrupted
   */
  public synchronized boolean isInterruptible() {
    return m_interruptible;
  }

  /**
   * Sets whether or not this command can be interrupted.
   *
   * @param interruptible whether or not this command can be interrupted
   */
  protected synchronized void setInterruptible(boolean interruptible) {
    m_interruptible = interruptible;
  }

  /**
   * Checks if the command requires the given {@link Subsystem}.
   *
   * @param system the system
   * @return whether or not the subsystem is required, or false if given null
   */
  public synchronized boolean doesRequire(Subsystem system) {
    return m_requirements != null && m_requirements.contains(system);
  }

  /**
   * Returns the {@link CommandGroup} that this command is a part of. Will return null if this
   * {@link Command} is not in a group.
   *
   * @return the {@link CommandGroup} that this command is a part of (or null if not in group)
   */
  public synchronized CommandGroup getGroup() {
    return m_parent;
  }

  /**
   * Sets whether or not this {@link Command} should run when the robot is disabled.
   *
   * <p>By default a command will not run when the robot is disabled, and will in fact be canceled.
   *
   * @param run whether or not this command should run when the robot is disabled
   */
  public void setRunWhenDisabled(boolean run) {
    m_runWhenDisabled = run;
  }

  /**
   * Returns whether or not this {@link Command} will run when the robot is disabled, or if it will
   * cancel itself.
   *
   * @return True if this command will run when the robot is disabled.
   */
  public boolean willRunWhenDisabled() {
    return m_runWhenDisabled;
  }

  /**
   * An empty enumeration given whenever there are no requirements.
   */
  private static Enumeration emptyEnumeration = new Enumeration() {

    public boolean hasMoreElements() {
      return false;
    }

    public Object nextElement() {
      throw new NoSuchElementException();
    }
  };

  /**
   * The string representation for a {@link Command} is by default its name.
   *
   * @return the string representation of this object
   */
  @Override
  public String toString() {
    return getName();
  }


  public String getSmartDashboardType() {
    return "Command";
  }

  private final ITableListener m_listener = (table1, key, value, isNew) -> {
    if (((Boolean) value).booleanValue()) {
      start();
    } else {
      cancel();
    }
  };
  private ITable m_table;

  @Override
  public void initTable(ITable table) {
    if (m_table != null) {
      m_table.removeTableListener(m_listener);
    }
    m_table = table;
    if (table != null) {
      table.putString("name", getName());
      table.putBoolean("running", isRunning());
      table.putBoolean("isParented", m_parent != null);
      table.addTableListener("running", m_listener, false);
    }
  }

  @Override
  public ITable getTable() {
    return m_table;
  }

}

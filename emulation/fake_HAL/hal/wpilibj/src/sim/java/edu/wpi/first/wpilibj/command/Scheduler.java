/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.command;

import java.util.Enumeration;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.Set;
import java.util.Vector;

import edu.wpi.first.wpilibj.NamedSendable;
import edu.wpi.first.wpilibj.buttons.Trigger.ButtonScheduler;
import edu.wpi.first.wpilibj.networktables2.type.NumberArray;
import edu.wpi.first.wpilibj.networktables2.type.StringArray;
import edu.wpi.first.wpilibj.tables.ITable;

/**
 * The {@link Scheduler} is a singleton which holds the top-level running commands. It is in charge
 * of both calling the command's {@link Command#run() run()} method and to make sure that there are
 * no two commands with conflicting requirements running.
 *
 * <p>It is fine if teams wish to take control of the {@link Scheduler} themselves, all that needs
 * to be done is to call {@link Scheduler#getInstance() Scheduler.getInstance()}.{@link
 * Scheduler#run() run()} often to have {@link Command Commands} function correctly. However, this
 * is already done for you if you use the CommandBased Robot template.</p>
 *
 * @see Command
 */
public class Scheduler implements NamedSendable {

  /**
   * The Singleton Instance.
   */
  private static Scheduler instance;

  /**
   * Returns the {@link Scheduler}, creating it if one does not exist.
   *
   * @return the {@link Scheduler}
   */
  public static synchronized Scheduler getInstance() {
    return instance == null ? instance = new Scheduler() : instance;
  }

  /**
   * A hashtable of active {@link Command Commands} to their {@link LinkedListElement}.
   */
  private Hashtable m_commandTable = new Hashtable();
  /**
   * The {@link Set} of all {@link Subsystem Subsystems}.
   */
  private Set<Subsystem> m_subsystems = new HashSet<>();
  /**
   * The first {@link Command} in the list.
   */
  private LinkedListElement m_firstCommand;
  /**
   * The last {@link Command} in the list.
   */
  private LinkedListElement m_lastCommand;
  /**
   * Whether or not we are currently adding a command.
   */
  private boolean m_adding = false;
  /**
   * Whether or not we are currently disabled.
   */
  private boolean m_disabled = false;
  /**
   * A list of all {@link Command Commands} which need to be added.
   */
  private Vector m_additions = new Vector();
  private ITable m_table;
  /**
   * A list of all {@link edu.wpi.first.wpilibj.buttons.Trigger.ButtonScheduler Buttons}. It is
   * created lazily.
   */
  private Vector m_buttons;
  private boolean m_runningCommandsChanged;

  /**
   * Instantiates a {@link Scheduler}.
   */
  private Scheduler() {
  }

  /**
   * Adds the command to the {@link Scheduler}. This will not add the {@link Command} immediately,
   * but will instead wait for the proper time in the {@link Scheduler#run()} loop before doing so.
   * The command returns immediately and does nothing if given null.
   *
   * <p>Adding a {@link Command} to the {@link Scheduler} involves the {@link Scheduler} removing
   * any {@link Command} which has shared requirements.</p>
   *
   * @param command the command to add
   */
  public void add(Command command) {
    if (command != null) {
      m_additions.addElement(command);
    }
  }

  /**
   * Adds a button to the {@link Scheduler}. The {@link Scheduler} will poll the button during its
   * {@link Scheduler#run()}.
   *
   * @param button the button to add
   */
  public void addButton(ButtonScheduler button) {
    if (m_buttons == null) {
      m_buttons = new Vector();
    }
    m_buttons.addElement(button);
  }

  /**
   * Adds a command immediately to the {@link Scheduler}. This should only be called in the {@link
   * Scheduler#run()} loop. Any command with conflicting requirements will be removed, unless it is
   * uninterruptable. Giving <code>null</code> does nothing.
   *
   * @param command the {@link Command} to add
   */
  @SuppressWarnings("MethodName")
  private void _add(Command command) {
    if (command == null) {
      return;
    }

    // Check to make sure no adding during adding
    if (m_adding) {
      System.err.println("WARNING: Can not start command from cancel method.  Ignoring:" + command);
      return;
    }

    // Only add if not already in
    if (!m_commandTable.containsKey(command)) {

      // Check that the requirements can be had
      Enumeration requirements = command.getRequirements();
      while (requirements.hasMoreElements()) {
        Subsystem lock = (Subsystem) requirements.nextElement();
        if (lock.getCurrentCommand() != null && !lock.getCurrentCommand().isInterruptible()) {
          return;
        }
      }

      // Give it the requirements
      m_adding = true;
      requirements = command.getRequirements();
      while (requirements.hasMoreElements()) {
        Subsystem lock = (Subsystem) requirements.nextElement();
        if (lock.getCurrentCommand() != null) {
          lock.getCurrentCommand().cancel();
          remove(lock.getCurrentCommand());
        }
        lock.setCurrentCommand(command);
      }
      m_adding = false;

      // Add it to the list
      LinkedListElement element = new LinkedListElement();
      element.setData(command);
      if (m_firstCommand == null) {
        m_firstCommand = m_lastCommand = element;
      } else {
        m_lastCommand.add(element);
        m_lastCommand = element;
      }
      m_commandTable.put(command, element);

      m_runningCommandsChanged = true;

      command.startRunning();
    }
  }

  /**
   * Runs a single iteration of the loop. This method should be called often in order to have a
   * functioning {@link Command} system. The loop has five stages:
   *
   * <ol> <li> Poll the Buttons </li> <li> Execute/Remove the Commands </li> <li> Send values to
   * SmartDashboard </li> <li> Add Commands </li> <li> Add Defaults </li> </ol>
   */
  public void run() {

    m_runningCommandsChanged = false;

    if (m_disabled) {
      return;
    } // Don't run when disabled

    // Get button input (going backwards preserves button priority)
    if (m_buttons != null) {
      for (int i = m_buttons.size() - 1; i >= 0; i--) {
        ((ButtonScheduler) m_buttons.elementAt(i)).execute();
      }
    }
    // Loop through the commands
    LinkedListElement linkedListElement = m_firstCommand;
    while (linkedListElement != null) {
      Command command = linkedListElement.getData();
      linkedListElement = linkedListElement.getNext();
      if (!command.run()) {
        remove(command);
        m_runningCommandsChanged = true;
      }
    }

    // Add the new things
    for (int i = 0; i < m_additions.size(); i++) {
      _add((Command) m_additions.elementAt(i));
    }
    m_additions.removeAllElements();

    // Add in the defaults
    Enumeration locks = new Vector<>(m_subsystems).elements();
    while (locks.hasMoreElements()) {
      Subsystem lock = (Subsystem) locks.nextElement();
      if (lock.getCurrentCommand() == null) {
        _add(lock.getDefaultCommand());
      }
      lock.confirmCommand();
    }

    updateTable();
  }

  /**
   * Registers a {@link Subsystem} to this {@link Scheduler}, so that the {@link Scheduler} might
   * know if a default {@link Command} needs to be run. All {@link Subsystem Subsystems} should call
   * this.
   *
   * @param system the system
   */
  void registerSubsystem(Subsystem system) {
    if (system != null) {
      m_subsystems.add(system);
    }
  }

  /**
   * Removes the {@link Command} from the {@link Scheduler}.
   *
   * @param command the command to remove
   */
  void remove(Command command) {
    if (command == null || !m_commandTable.containsKey(command)) {
      return;
    }
    LinkedListElement linkedListElement = (LinkedListElement) m_commandTable.get(command);
    m_commandTable.remove(command);

    if (linkedListElement.equals(m_lastCommand)) {
      m_lastCommand = linkedListElement.getPrevious();
    }
    if (linkedListElement.equals(m_firstCommand)) {
      m_firstCommand = linkedListElement.getNext();
    }
    linkedListElement.remove();

    Enumeration requirements = command.getRequirements();
    while (requirements.hasMoreElements()) {
      ((Subsystem) requirements.nextElement()).setCurrentCommand(null);
    }

    command.removed();
  }

  /**
   * Removes all commands.
   */
  public void removeAll() {
    // TODO: Confirm that this works with "uninteruptible" commands
    while (m_firstCommand != null) {
      remove(m_firstCommand.getData());
    }
  }

  /**
   * Disable the command scheduler.
   */
  public void disable() {
    m_disabled = true;
  }

  /**
   * Enable the command scheduler.
   */
  public void enable() {
    m_disabled = false;
  }

  @Override
  public String getName() {
    return "Scheduler";
  }

  public String getType() {
    return "Scheduler";
  }

  private StringArray m_commands;
  private NumberArray m_ids;
  private NumberArray m_toCancel;

  @Override
  public void initTable(ITable subtable) {
    m_table = subtable;
    m_commands = new StringArray();
    m_ids = new NumberArray();
    m_toCancel = new NumberArray();

    m_table.putValue("Names", m_commands);
    m_table.putValue("Ids", m_ids);
    m_table.putValue("Cancel", m_toCancel);
  }

  private void updateTable() {
    if (m_table != null) {
      // Get the commands to cancel
      m_table.retrieveValue("Cancel", m_toCancel);
      if (m_toCancel.size() > 0) {
        for (LinkedListElement e = m_firstCommand; e != null; e = e.getNext()) {
          for (int i = 0; i < m_toCancel.size(); i++) {
            if (e.getData().hashCode() == m_toCancel.get(i)) {
              e.getData().cancel();
            }
          }
        }
        m_toCancel.setSize(0);
        m_table.putValue("Cancel", m_toCancel);
      }

      if (m_runningCommandsChanged) {
        m_commands.setSize(0);
        m_ids.setSize(0);
        // Set the the running commands
        for (LinkedListElement e = m_firstCommand; e != null; e = e.getNext()) {
          m_commands.add(e.getData().getName());
          m_ids.add(e.getData().hashCode());
        }
        m_table.putValue("Names", m_commands);
        m_table.putValue("Ids", m_ids);
      }
    }
  }

  @Override
  public ITable getTable() {
    return m_table;
  }

  @Override
  public String getSmartDashboardType() {
    return "Scheduler";
  }
}

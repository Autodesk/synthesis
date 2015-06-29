/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2014. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj.test;

import java.util.logging.Level;
import java.util.logging.Logger;

import org.junit.BeforeClass;
import org.junit.Rule;
import org.junit.rules.TestWatcher;
import org.junit.runner.Description;
import org.junit.runners.model.MultipleFailureException;

import edu.wpi.first.wpilibj.DriverStation;
import edu.wpi.first.wpilibj.RobotBase;
import edu.wpi.first.wpilibj.Timer;
import edu.wpi.first.wpilibj.communication.FRCNetworkCommunicationsLibrary;
import edu.wpi.first.wpilibj.livewindow.LiveWindow;

/**
 * This class serves as a superclass for all tests that involve the hardware on
 * the roboRIO. It uses an {@link BeforeClass} statement to initialize network
 * communications. Any test that needs to use the hardware <b>MUST</b> extend
 * from this class, to ensure that all of the hardware will be able to run.
 * 
 * @author Fredric Silberberg
 * @author Jonathan Leitschuh
 */
public abstract class AbstractComsSetup {
	/** Stores whether network coms have been initialized */
	private static boolean initialized = false;
	
	/**
	 * This sets up the network communications library to enable the driver
	 * station. After starting network coms, it will loop until the driver
	 * station returns that the robot is enabled, to ensure that tests will be
	 * able to run on the hardware.
	 * 
	 */
	static{
		if (!initialized) {
			// Set some implementations so that the static methods work properly
			RobotBase.initializeHardwareConfiguration();
			FRCNetworkCommunicationsLibrary.FRCNetworkCommunicationObserveUserProgramStarting();
			
			LiveWindow.setEnabled(false);
			TestBench.out().println("Started coms");

			// Wait until the robot is enabled before starting the tests
			int i = 0;
			while (!DriverStation.getInstance().isEnabled()) {
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				//Prints the message on one line overwriting itself each time
				TestBench.out().print("\rWaiting for enable: " + i++);
			}
			TestBench.out().println();

			// Ready to go!
			initialized = true;
			TestBench.out().println("Running!");
		}
	}


	protected abstract Logger getClassLogger();
	
	/** This causes a stack trace to be printed as the test is running as well as at the end */
	@Rule
	public final TestWatcher getTestWatcher(){
		return getOverridenTestWatcher();
	}
	
	/**
	 * Given as a way to provide a custom test watcher for a test or set of tests
	 * @return the test watcher to use
	 */
	protected TestWatcher getOverridenTestWatcher(){
		return new DefaultTestWatcher();
	}
	
	protected class DefaultTestWatcher extends TestWatcher {
		/**
		 * Allows a failure to supply a custom status message to be displayed along with the stack trace.
		 */
		protected void failed(Throwable e, Description description, String status){
			TestBench.out().println();
			//Instance of is the best way I know to retrieve this data.
			if(e instanceof MultipleFailureException){
				/* MultipleFailureExceptions hold multiple exceptions in one exception. In order to properly display these stack traces
				 * we have to cast the throwable and work with the list of thrown exceptions stored within it.
				 */
				int i = 1; //Running exception count
				int failureCount = ((MultipleFailureException)e).getFailures().size();
				for(Throwable singleThrown : ((MultipleFailureException)e).getFailures()){
					getClassLogger().logp(Level.SEVERE, description.getClassName(), description.getMethodName(), (i++) + "/" + failureCount
							+ " "+ description.getDisplayName() + " failed " + singleThrown.getMessage() + "\n" + status, singleThrown);
				}
				
			} else {
				getClassLogger().logp(Level.SEVERE, description.getClassName(), description.getMethodName(), description.getDisplayName() + " failed " + e.getMessage() + "\n" + status, e);
			}
			super.failed(e, description);
		}
		
		/* (non-Javadoc)
		 * @see org.junit.rules.TestWatcher#failed(java.lang.Throwable, org.junit.runner.Description)
		 */		
		@Override
		protected void failed(Throwable e, Description description) {
			failed(e, description, "");
		}

		
		/* (non-Javadoc)
		 * @see org.junit.rules.TestWatcher#starting(org.junit.runner.Description)
		 */
		@Override
	    protected void starting(Description description ) {
			TestBench.out().println();
			// Wait until the robot is enabled before starting the next tests
			int i = 0;
			while (!DriverStation.getInstance().isEnabled()) {
				try {
					Thread.sleep(100);
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
				//Prints the message on one line overwriting itself each time
				TestBench.out().print("\rWaiting for enable: " + i++);
			}
			getClassLogger().logp(Level.INFO, description.getClassName(), description.getMethodName(), "Starting");
	        super.starting(description);
	    }

		@Override
		protected void succeeded(Description description) {
			simpleLog(Level.INFO, "TEST PASSED!");
			super.succeeded(description);
		}

	};
	
	/**
	 * Logs a simple message without the logger formatting associated with it.
	 * @param level The level to log the message at
	 * @param message The message to log
	 */
	protected void simpleLog(Level level, String message){
		if(getClassLogger().isLoggable(level)){
			TestBench.out().println(message);
		}
	}
	
	/**
	 * Provided as a replacement to lambda functions to allow for repeatable checks to see if a correct state is reached
	 * @author Jonathan Leitschuh
	 *
	 */
	public abstract class BooleanCheck{
		public BooleanCheck(){}
		/**
		 * Runs the enclosed code and evaluates it to determine what state it should return.
		 * @return true if the code provided within the method returns true
		 */
		abstract public boolean getAsBoolean();
	};
	
	/**
	 * Delays until either the correct state is reached or we reach the timeout.
	 * @param level The level to log the message at.
	 * @param timeout How long the delay should run before it should timeout and allow the test to continue
	 * @param message The message to accompany the delay. The message will display 'message' took 'timeout' seconds if it passed.
	 * @param correctState A method to determine if the test has reached a state where it is valid to continue
	 * @return a double representing how long the delay took to run in seconds.
	 */
	public double delayTillInCorrectStateWithMessage(Level level, double timeout, String message, BooleanCheck correctState){
		int i = 0;
		//As long as we are not in the correct state and the timeout has not been reached then continue to run this loop
		for(i = 0; i < (timeout * 100) && !correctState.getAsBoolean(); i++){
			Timer.delay(.01);
		}
		if(correctState.getAsBoolean()){
			simpleLog(level, message + " took " + (i * .01) + " seconds");
		} else {
			simpleLog(level, message + " timed out after " + (i * .01) + " seconds");
		}
		return i*.01;
	}

}

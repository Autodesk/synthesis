/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2014. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;

import static org.junit.Assert.assertTrue;

import java.util.Collection;
import java.util.logging.Logger;

import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.junit.runners.Parameterized.Parameters;

import edu.wpi.first.wpilibj.fixtures.FakeEncoderFixture;
import edu.wpi.first.wpilibj.test.AbstractComsSetup;
import edu.wpi.first.wpilibj.test.TestBench;


/**
 * Test to see if the FPGA properly recognizes a mock Encoder input
 * 
 * @author Jonathan Leitschuh
 *
 */
@RunWith(Parameterized.class)
public class EncoderTest extends AbstractComsSetup {
	private static final Logger logger = Logger.getLogger(EncoderTest.class.getName());
	private static FakeEncoderFixture encoder = null;
	
	private final boolean flip; //Does this test need to flip the inputs
	private final int inputA;
	private final int inputB;
	private final int outputA;
	private final int outputB;
	
	@Override
	protected Logger getClassLogger(){
		return logger;
	}
	
	/**
	 * Test data generator. This method is called the the JUnit
	 * parameterized test runner and returns a Collection of Arrays. For each
	 * Array in the Collection, each array element corresponds to a parameter
	 * in the constructor.
	 */
	@Parameters
	public static Collection<Integer[]> generateData() {
		return TestBench.getInstance().getEncoderDIOCrossConnectCollection();
	}
	
	/**
	 * Constructs a parameterized Encoder Test
	 * @param inputA The port number for inputA
	 * @param outputA The port number for outputA
	 * @param inputB The port number for inputB
	 * @param outputB The port number for outputB
	 * @param flip whether or not these set of values require the encoder to be reversed (0 or 1)
	 */
	public EncoderTest(int inputA, int outputA, int inputB, int outputB, int flip){
		this.inputA = inputA;
		this.inputB = inputB;
		this.outputA = outputA;
		this.outputB = outputB;
		
		//If the encoder from a previous test is allocated then we must free its members
		if(encoder != null) encoder.teardown();
		this.flip = flip==0;
		encoder = new FakeEncoderFixture(inputA, outputA, inputB, outputB);
	}

	/**
	 * @throws java.lang.Exception
	 */
	@AfterClass
	public static void tearDownAfterClass() throws Exception {
		encoder.teardown();
		encoder=null;
	}

	/**
	 * Sets up the test and verifies that the test was reset to the default state
	 * @throws java.lang.Exception
	 */
	@Before
	public void setUp() throws Exception {
		encoder.setup();
		testDefaultState();
	}

	/**
	 * @throws java.lang.Exception
	 */
	@After
	public void tearDown() throws Exception {
		encoder.reset();
	}
	
	/**
	 * Tests to see if Encoders initialize to zero
	 */
	@Test
	public void testDefaultState(){
		int value = encoder.getEncoder().get();
		assertTrue(errorMessage(0, value), value == 0);
	}

	/**
	 * Tests to see if Encoders can count up sucsessfully
	 */
	@Test
	public void testCountUp() {
		int goal = 100;
		encoder.getFakeEncoderSource().setCount(goal);
		encoder.getFakeEncoderSource().setForward(flip);
		encoder.getFakeEncoderSource().execute();
		int value = encoder.getEncoder().get();
		assertTrue(errorMessage(goal, value), value == goal);
		
	}
	/**
	 * Tests to see if Encoders can count down sucsessfully
	 */
	@Test
	public void testCountDown() {
		int goal = -100;
		encoder.getFakeEncoderSource().setCount(goal); //Goal has to be positive
		encoder.getFakeEncoderSource().setForward(!flip);
		encoder.getFakeEncoderSource().execute();
		int value = encoder.getEncoder().get();
		assertTrue(errorMessage(goal, value), value == goal);
		
	}
	
	/**
	 * Creates a simple message with the error that was encounterd for the Encoders
	 * @param goal The goal that was trying to be reached
	 * @param trueValue The actual value that was reached by the test
	 * @return A fully constructed message with data about where and why the the test failed
	 */
	private String errorMessage(int goal, int trueValue){
		return "Encoder ({In,Out}): {" + inputA + ", " + outputA + "},{" + inputB + ", " + outputB + "} Returned: " + trueValue + ", Wanted: " + goal;
	}
}

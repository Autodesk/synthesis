/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.communication;

import com.ni.rio.NiFpga;
import com.sun.jna.Function;
import com.sun.jna.NativeLibrary;
import com.sun.jna.Pointer;

/**
 * Class for calibrating the analog inputs.
 * 
 * @author jhersh
 */
public class AICalibration {
	// UINT32 FRC_NetworkCommunication_nAICalibration_getLSBWeight(const UINT32
	// aiSystemIndex, const UINT32 channel, INT32 *status);

	private static final Function getLSBWeightFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction(
			"FRC_NetworkCommunication_nAICalibration_getLSBWeight");

	/**
	 * Get the weight of the least significant bit.
	 * 
	 * @param aiSystemIndex
	 *            The system index.
	 * @param channel
	 *            The analog channel.
	 * @return The LSB weight.
	 */
	public static long getLSBWeight(int aiSystemIndex, int channel) {
		// TODO: implement error handling if you care.
		long lsbWeight = getLSBWeightFn.invokeLong(new Object[] {
				aiSystemIndex, channel, Pointer.NULL }) & 0xFFFFFFFFl;
		return lsbWeight;
	}

	// INT32 FRC_NetworkCommunication_nAICalibration_getOffset(const UINT32
	// aiSystemIndex, const UINT32 channel, INT32 *status);
	private static final Function getOffsetFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction(
			"FRC_NetworkCommunication_nAICalibration_getOffset");

	/**
	 * Get the offset.
	 * 
	 * @param aiSystemIndex
	 *            The system index.
	 * @param channel
	 *            The analog channel.
	 * @return The offset.
	 */
	public static int getOffset(int aiSystemIndex, int channel) {
		// TODO: implement error handling if you care.
		int offset = getOffsetFn.invokeInt(new Object[] { aiSystemIndex,
				channel, Pointer.NULL });
		return offset;
	}
}

/**
   \file       NiRioEntryPoints.h
   \author     Erik Hons <erik.hons@ni.com>
   \date       12/14/2004

   \brief Declarations for RIO services client DLL entry points

   Intended to be called from a C client, or the LabVIEW
   interface.

   � Copyright 2004. National Instruments. All rights reserved.
 */

package com.ni.rio;

import com.sun.jna.Function;
import com.sun.jna.NativeLibrary;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.IntByReference;

/**
 * The NiFpga class provides access to the FPGA on the cRIO. This is a wrapper
 * around the accessors in NiFpga.h
 */
public class NiFpga implements NiRioConstants {
	public static final String LIBRARY_NAME = "FakeFPGA";
	// ---------------------------
	// Fifo Operations:

	private static final Function configFifoFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_ConfigureFifo");

	/**
	 * Specifies the depth of the host memory part of the DMA FIFO. This method
	 * is optional. In order to see the actual depth configured, use
	 * NiFpga_ConfigureFifo2.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param channel
	 *            FIFO to configure
	 * @param fifoDepthInElements
	 *            requested number of elements in the host memory part of the
	 *            DMA FIFO
	 * @param status
	 *            result of the call
	 */
	public static void configureFifo(int hClient, int channel,
			int fifoDepthInElements, NiRioStatus status) {
		mergeStatus(
				status,
				configFifoFn.invokeInt(new Object[] { hClient, channel,
						fifoDepthInElements }));
	}

	private static final Function startFifoFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_StartFifo");

	/**
	 * Starts a FIFO. This method is optional.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param channel
	 *            FIFO to start
	 * @param status
	 *            result of the call
	 */
	public static void startFifo(int hClient, int channel, NiRioStatus status) {
		mergeStatus(status,
				startFifoFn.invokeInt(new Object[] { hClient, channel }));
	}

	private static final Function stopFifoFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_StopFifo");

	/**
	 * Stops a FIFO. This method is optional.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param channel
	 *            FIFO to start
	 * @param status
	 *            result of the call
	 */
	public static void stopFifo(int hClient, int channel, NiRioStatus status) {
		mergeStatus(status,
				stopFifoFn.invokeInt(new Object[] { hClient, channel }));
	}

	private static final Function readFifoU32Fn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_ReadFifoU32");

	/**
	 * Reads from a target-to-host FIFO of unsigned 32-bit integers.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param channel
	 *            target-to-host FIFO from which to read
	 * @param buf
	 *            outputs the data that was read
	 * @param num
	 *            number of elements to read
	 * @param timeout
	 *            timeout in milliseconds, or NiFpga_InfiniteTimeout
	 * @param remaining
	 *            outputs the number of elements remaining in the host memory
	 *            part of the DMA FIFO
	 * @param status
	 *            result of the call
	 */
	public static void readFifoU32(int hClient, int channel, Pointer buf,
			int num, int timeout, IntByReference remaining, NiRioStatus status) {
		mergeStatus(
				status,
				readFifoU32Fn.invokeInt(new Object[] { hClient, channel,
						Pointer.nativeValue(buf), num, timeout,
						Pointer.nativeValue(buf) }));
	}

	// ---------------------------
	// I/O:

	/**
	 * Conditionally sets the status to a new value. The previous status is
	 * preserved unless the new status is more of an error, which means that
	 * warnings and errors overwrite successes, and errors overwrite warnings.
	 * New errors do not overwrite older errors, and new warnings do not
	 * overwrite older warnings.
	 * 
	 * @param status
	 *            status to conditionally set
	 * @param newStatus
	 *            int value new status value that may be set
	 */
	static void mergeStatus(NiRioStatus statusA, int statusB) {
		statusA.setStatus(statusB);
	}

	private static final Function writeU32Fn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_WriteU32");

	/**
	 * Writes an unsigned 32-bit integer value to a given control or indicator.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param offset
	 *            control or indicator to which to write
	 * @param value
	 *            value to write
	 * @param status
	 *            result of the call
	 */
	public static void writeU32(int hClient, int offset, int value,
			NiRioStatus status) {
//		 System.out.print("write offset = 0x");
//		 System.out.print(Long.toString(offset, 16) + "");
//		 System.out.print("value = ");
//		 System.out.println(Long.toString(((long)value) & 0xFFFFFFFFL, 10));
		mergeStatus(status,
				writeU32Fn.invokeInt(new Object[] { hClient, offset, value }));
	}

	private static IntByReference readValue = new IntByReference(0);

	private static final Function readU32Fn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_ReadU32");

	/**
	 * Reads an unsigned 32-bit integer value from a given offset
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param offset
	 *            indicator or control from which to read
	 * @param status
	 *            result of the call
	 * @return outputs the value that was read
	 */
	public static synchronized int readU32(int hClient, int offset,
			NiRioStatus status) {
		// System.out.print("read offset = 0x");
		// System.out.println(Long.toString(offset, 16));
		mergeStatus(
				status,
				readU32Fn.invokeInt(new Object[] { hClient, offset,
						Pointer.nativeValue(readValue.getPointer()) }));
		// System.out.print("value = 0x");
		// System.out.println(Long.toString(((long)value) & 0xFFFFFFFFL, 16));
		return readValue.getValue();
	}

	// ---------------------------
	// IRQs:

	private static final Function reserveIrqContextFn = NativeLibrary
			.getInstance(NiFpga.LIBRARY_NAME).getFunction(
					"NiFpga_ReserveIrqContext");

	/**
	 * IRQ contexts are single-threaded; only one thread can wait with a
	 * particular context at any given time. Clients must reserve as many
	 * contexts as the application requires.
	 * 
	 * If a context is successfully reserved (the returned status is not an
	 * error), it must be unreserved later. Otherwise a memory leak will occur.
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param context
	 *            outputs the IRQ context
	 * @param NiRioStatus
	 *            result of the call
	 */
	public static void reserveIrqContext(int hClient, IntByReference context,
			NiRioStatus status) {
		mergeStatus(
				status,
				reserveIrqContextFn.invokeInt(new Object[] { hClient,
						Pointer.nativeValue(context.getPointer()) }));
	}

	private static final Function unreserveIrqContextFn = NativeLibrary
			.getInstance(NiFpga.LIBRARY_NAME).getFunction(
					"NiFpga_UnreserveIrqContext");

	/**
	 * Unreserves an IRQ context obtained from reserveIrqContext.
	 * 
	 * @param session
	 *            handle to a currently open session
	 * @param context
	 *            IRQ context to unreserve
	 * @return result of the call
	 */
	public static void unreserveIrqContext(int hClient, IntByReference context,
			NiRioStatus status) {
		mergeStatus(
				status,
				unreserveIrqContextFn.invokeInt(new Object[] { hClient,
						context.getValue() }));
	}

	private static IntByReference irqsAsserted = new IntByReference(0);

	private static final Function waitOnIrqsFn = NativeLibrary.getInstance(
			NiFpga.LIBRARY_NAME).getFunction("NiFpga_WaitOnIrqs");

	/**
	 * This is a blocking function that stops the calling thread until the FPGA
	 * asserts any IRQ in the irqs parameter, or until the function call times
	 * out. Before calling this function, you must use NiFpga_ReserveIrqContext
	 * to reserve an IRQ context. No other threads can use the same context when
	 * this function is called.
	 * 
	 * You can use the irqsAsserted parameter to determine which IRQs were
	 * asserted for each function call.
	 * 
	 * @todo If this really blocks, then waitOnIrqsFn should probably be a
	 *       BlockingFunction
	 * 
	 * @param hClient
	 *            handle to a currently open session
	 * @param context
	 *            IRQ context with which to wait
	 * @param irqs
	 *            bitwise OR of NiFpga_Irqs
	 * @param timeout
	 *            timeout in milliseconds, or NiFpga_InfiniteTimeout
	 * @param irqsAsserted
	 *            if non-NULL, outputs bitwise OR of IRQs that were asserted
	 * @param timedOut
	 *            if non-NULL, outputs whether the timeout expired
	 * @return bitwise OR of IRQs that were asserted
	 */
	public static synchronized int waitOnIrqs(int hClient,
			IntByReference context, int irqs, int timeout, NiRioStatus status) {
		irqsAsserted.setValue(0);
		mergeStatus(
				status,
				waitOnIrqsFn.invokeInt(new Object[] { hClient,
						context.getValue(), irqs, timeout,
						Pointer.nativeValue(irqsAsserted.getPointer()), 0 }));
		return irqsAsserted.getValue();
	}

	private NiFpga() {
	}

}

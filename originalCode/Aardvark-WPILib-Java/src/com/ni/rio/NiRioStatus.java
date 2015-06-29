/**
   \file       NiRioStatus.h
   \author     Dave Madden <david.madden@ni.com>
   \date       02/21/2006

   � Copyright 2006-2007. National Instruments. All rights reserved.
*/

package com.ni.rio;

import com.sun.jna.Pointer;
import com.sun.jna.ptr.IntByReference;

/**
 * The NiRioStatus class encapsulates a cRIO status value.
 * 
 * The NiRioStatus class also defines the various status constants used by the cRIO. 
 * 
 * @todo Why are we storing the status value in an IntByReference? We should be able to use a simple int field.
 */
public class NiRioStatus
{
   public static final class FatalStatusException extends IllegalStateException
   {
      NiRioStatus m_status;
      public FatalStatusException(NiRioStatus status, String message)
      {
         super(message);
         m_status = status;
      }
      public NiRioStatus getStatus()
      {
         return m_status;
      }
   }

   public NiRioStatus()
   {
      status = new IntByReference(kRioStatusSuccess);
   }

   private IntByReference status;

   public Pointer getPointer()
   {
      return status.getPointer();
   }

   // Valid RIO ranges: [-63199, -63000], [63000,63199]
   public static final int kRioStatusOffset = -63000;

   /// The operation was successful
   public static final int kRioStatusSuccess = 0;

   // Errors

   // -------------------------------------------------------------------------
   // FIFO Error messages...
   // -------------------------------------------------------------------------

   /// DMA to the FPGA target is not supported by the controller associated with the FPGA (e.g. cRIO-9002/4).
   public static final int kRioStatusDmaOutputNotSupported        = kRioStatusOffset - 1;

   // -------------------------------------------------------------------------
   // IO Manager
   // -------------------------------------------------------------------------

   /// <internal>The parameters do not describe a valid address range</internal>
   public static final int kRIOStatusIOInvalidAddressRange        = kRioStatusOffset - 10;

   /// <internal>The buffer supplied for the I/O operation in invalid</internal>
   public static final int kRIOStatusIOInvalidBuffer              = kRioStatusOffset - 11;

   // -------------------------------------------------------------------------
   // Device Errors
   // -------------------------------------------------------------------------

   /// The operation could not complete because another session has reconfigured the device.
   ///
   /// DEPRECATED in 230
   public static final int kRIOStatusDeviceReconfigured           = kRioStatusOffset - 30;

   /// <internal>The operation is not allowed because another session is accessing the device. Close all other sessions and retry.</internal>
   public static final int kRIOStatusDeviceInvariant              = kRioStatusOffset - 31;

   /// <internal>Download is not allowed because another session is accessing the device. Close all other sessions and retry</internal>.
   public static final int kRIOStatusDeviceInvalidStateTransition = kRioStatusOffset - 32;

   /// The operation was prohibited by the device access settings on the remote system.
   public static final int kRIOStatusAccessDenied                 = kRioStatusOffset - 33;


   // -------------------------------------------------------------------------
   // RPC/Network Errors
   // -------------------------------------------------------------------------

   /// An RPC connection could not be made to the remote device. The device may be offline, disconnected, or NI-RIO software may be missing or configured incorrectly
   public static final int kRIOStatusRPCConnectionError           = kRioStatusOffset - 40;

   /// The RPC server had an error. Close any open sessions and reconnect. The current operation cannot complete.
   public static final int kRIOStatusRPCServerError               = kRioStatusOffset - 41;

   /// A fault on the network caused the operation to fail.
   public static final int kRIOStatusNetworkFault                 = kRioStatusOffset - 42;

   /// The current session is invalid. The target may have reset or been
   /// rebooted. The current operation cannot complete. Try again.
   public static final int kRIOStatusRioRpcSessionError           = kRioStatusOffset - 43;

   // -------------------------------------------------------------------------
   // Session Errors
   // -------------------------------------------------------------------------
   /// The specified trigger line is already reserved
   public static final int kRIOStatusTriggerReserved              = kRioStatusOffset - 50;

   /// The specified trigger line is not reserved by the current session.
   public static final int kRIOStatusTriggerNotReserved           = kRioStatusOffset - 51;

   /// Trigger lines are not supported or enabled. For PXI chasses, identify the controller and chassis via MAX.
   public static final int kRIOStatusTriggerNotSupported          = kRioStatusOffset - 52;


   // -------------------------------------------------------------------------
   // Event Errors
   // -------------------------------------------------------------------------

   /// <internal>The specified event type is invalid.</internal>
   public static final int kRIOStatusEventInvalid                 = kRioStatusOffset - 70;

   /// The specified RIO event has already been enabled for this session.
   public static final int kRIOStatusEventEnabled                 = kRioStatusOffset - 71;

   /// The specified RIO event has not been enabled for this session.
   public static final int kRIOStatusEventNotEnabled              = kRioStatusOffset - 72;

   ///  The specified RIO event did not complete within the specified time limit.
   public static final int kRIOStatusEventTimedOut                = kRioStatusOffset - 73;

   /// <internal>The specified operation on a the specified Rio event is invalid at this time</internal>.
   public static final int kRIOStatusEventInvalidOperation        = kRioStatusOffset - 74;

   // -------------------------------------------------------------------------
   // API Errors
   // -------------------------------------------------------------------------

   /// Allocated buffer is too small
   public static final int kRIOStatusBufferInvalidSize            = kRioStatusOffset - 80;

   /// Caller did not allocate a buffer
   public static final int kRIOStatusBufferNotAllocated           = kRioStatusOffset - 81;

   /// The fifo is reservered in another session
   public static final int kRIOStatusFifoReserved                 = kRioStatusOffset - 82;

   // -------------------------------------------------------------------------
   // PAL Replacements
   // -------------------------------------------------------------------------

   /// A hardware failure has occurred. The operation could not be completed as specified.
   public static final int kRIOStatusHardwareFault                = kRioStatusOffset - 150;

   /// <internal>The resource was already initialized and cannot be initialized again. The operation could not be completed as specified.</internal>
   public static final int kRIOStatusResourceInitialized          = kRioStatusOffset - 151;

   /// <internal>The requested resource was not found </internal>.
   public static final int kRIOStatusResourceNotFound             = kRioStatusOffset - 152;

   // -------------------------------------------------------------------------
   // Configuration
   // -------------------------------------------------------------------------

   /// An invalid alias was specified. RIO aliases may only contain alphanumerics, '-', and '_'.
   public static final int kRIOStatusInvalidAliasName             = kRioStatusOffset - 180;

   /// The supplied alias was not found.
   public static final int kRIOStatusAliasNotFound                = kRioStatusOffset - 181;

   /// An invalid device access setting was specified. RIO device access patterns may only contain alphanumerics, '-', '_', '.', and '*'.
   public static final int kRIOStatusInvalidDeviceAccess          = kRioStatusOffset - 182;

   /// An invalid port was specified. The RIO Server port must be between 0 and 65535, where 0 indicates a dynamically assigned port. Port 3580 is reserved and cannot be used.
   public static final int kRIOStatusInvalidPort                  = kRioStatusOffset - 183;

   // -------------------------------------------------------------------------
   // Misc.
   // -------------------------------------------------------------------------

   /// This platform does not support connections to remote targets.
   public static final int kRIOStatusRemoteTarget                 = kRioStatusOffset - 187;

   /// The operation is no longer supported
   public static final int kRIOStatusDeprecatedFunction           = kRioStatusOffset - 188;

   /// The supplied search pattern isn't understood
   public static final int kRIOStatusInvalidPattern               = kRioStatusOffset - 189;

   /// <internal>The specified device control code is not recognized.</internal>
   public static final int kRIOStatusBadDeviceControlCode         = kRioStatusOffset - 190;

   /// The supplied resource name is not valid. Use MAX to find the proper resource name.
   public static final int kRIOStatusInvalidResourceName          = kRioStatusOffset - 192;

   /// The requested feature is not supported in the current version of the driver software on either the host or the target.
   public static final int kRIOStatusFeatureNotSupported          = kRioStatusOffset - 193;

   /// The version of the target software is incompatible. Upgrade target software to get full functionality. (For use by host interface).
   public static final int kRIOStatusVersionMismatch              = kRioStatusOffset - 194;

   /// A handle for device communication is invalid. The device connection has been lost.
   public static final int kRIOStatusInvalidHandle                = kRioStatusOffset - 195;

   /// An invalid attribute has been specified.
   public static final int kRIOStatusInvalidAttribute             = kRioStatusOffset - 196;

   /// An invalid attribute value has been specified.
   public static final int kRIOStatusInvalidAttributeValue        = kRioStatusOffset - 197;

   /// The system has run out of resource handles. Try closing some sessions.
   public static final int kRIOStatusOutOfHandles                 = kRioStatusOffset - 198;

   /// <internal>The server does not recognize the command</internal>
   public static final int kRIOStatusInvalidFunction              = kRioStatusOffset - 199;

   // -------------------------------------------------------------------------
   // APAL translation
   // #define nNIAPALS100_tStatus_kOffset    -52000
   // -------------------------------------------------------------------------

   /// nNIAPALS100_tStatus_kMemoryFull. The specified number of bytes could not be allocated
   public static final int kRIOStatusMemoryFull                   = -52000;

   /// nNIAPALS100_tStatus_kPageLockFailed. The memory could not be page locked
   public static final int kRIOStatusPageLockFailed               = -52001;

   /// nNIAPALS100_tStatus_kSoftwareFault. An unexpected software error occurred.
   public static final int kRIOStatusSoftwareFault                = -52003;

   /// nNIAPALS100_tStatus_kDynamicCastFailed. The dynamic cast operation failed.
   public static final int kRIOStatusDynamicCastFailed            = -52004;

   /// nNIAPALS100_tStatus_kInvalidParameter. The parameter recieved by the function is not valid.
   public static final int kRIOStatusInvalidParameter             = -52005;

   /// nNIAPALS100_tStatus_kOperationTimedOut. The requested operation did not complete in time.
   public static final int kRIOStatusOperationTimedOut            = -52007;

   /// nNIAPALS100_tStatus_kOSFault. An unexpected operationg system error occurred.
   public static final int kRIOStatusOSFault                      = -52008;

   /// nNIAPALS100_tStatus_kResourceMarkedForDelete. The requested resource has been marked for deletion and is rejecting new requests.
   public static final int kRIOStatusMarkedForDelete              = -52009;

   /// nNIAPALS100_tStatus_kResourceNotInitialized. The requested resource must be initializaed before use.
   public static final int kRIOStatusResourceNotInitialized       = -52010;

   /// nNIAPALS100_tStatus_kOperationPending. The operation has begun and will complete asynchronously.
   public static final int kRIOStatusOperationPending             = -52011;

   /// nNIAPALS100_tStatus_kEndOfData. There is no more data available to read or no more space available in which to write.
   public static final int kRIOStatusEndOfData                    = -52012;

   /// nNIAPALS100_tStatus_kObjectNameCollision. The name or handle requested has already been reserved by another client
   public static final int kRIOStatusObjectNameCollision          = -52013;

   /// nNIAPALS100_tStatus_kSyncronizationObjectAbandoned. The synchronization object was abandoned by the previous holder
   public static final int kRIOStatusSyncObjectAbandoned          = -52014;

   /// nNIAPALS100_tStatus_kSyncronizationAcquireFailed. The acquisition of the synchronization object failed
   public static final int kRIOStatusSyncAcquireFailed            = -52015;

   /// nNIAPALS100_tStatus_kThreadAlreadyStarted. The thread was running when you attempted to start it (redundant state change).
   public static final int kRIOStatusThreadAlreadyStarted         = -52016;

   /// nNIAPALS100_tStatus_kInvalidStateTransition. The operation you requested is redundant or invalid when the object is in this state.
   public static final int kRIOStatusInvalidStateTransition       = -52017;

   // -------------------------------------------------------------------------
   // Traditional PAL translation
   // #define nNIAPALS100_tStatus_kOffset    -50000
   // -------------------------------------------------------------------------
   /// An attribute whether explicit or implicit is not relevant or is not relevant given the current system state. The operation could not be completed as specified.
   public static final int kRIOStatusIrrelevantAttribute          = -50001;

   /// A selector - usually of an enumerated type - is inappropriate or out of range. The operation could not be completed as specified.
   public static final int kRIOStatusBadSelector                  = -50003;

   /// The specified software component is not available. The component was not loaded.
   public static final int kRIOStatusComponentNotFound            = -50251;

   /// The specified device is not present or is deactivated. The operation could not be completed as specified.
   public static final int kRIOStatusDeviceNotFound               = -50300;


   // -------------------------------------------------------------------------
   // LabVIEW errors
   // -------------------------------------------------------------------------

   /// Generic file I/O error.
   public static final int kRIOStatusFileError                    = -6;

   //
   // Helper functions
   //

   public boolean isFatal()
   {
      return status.getValue() < 0;
   }

   public boolean isNotFatal()
   {
      return status.getValue() >= 0;
   }

   /**
    * Update the status code to the most serious of the existing status code or the new status code. If the 
    * resulting code is "fatal" throw NiRioStatus.FatalStatusException.
    * 
    * @param newStatusCode 
    */
   public void setStatus(int newStatusCode)
   {
      if (status.getValue() >= 0 && (status.getValue() == 0 || newStatusCode < 0))
      {
         status.setValue(newStatusCode);
      }
      assertNonfatal();
   }

   public void setStatus(NiRioStatus newStatus)
   {
      setStatus(newStatus.getStatusCode());
   }

   public int getStatusCode()
   {
      return status.getValue();
   }

   public void assertNonfatal() throws IllegalStateException
   {
      if (isFatal())
      {
         throw new NiRioStatus.FatalStatusException(this, "Fatal status code detected:  " + Integer.toString(getStatusCode()));
      }
   }
}

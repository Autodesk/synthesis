// Class for handling interrupts.
// Copyright (c) National Instruments 2008.  All Rights Reserved.

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;
import com.sun.jna.ptr.IntByReference;

public class tInterruptManager extends tSystem
{

   private static final int kFPGA_INTERRUPT_BASE_ADDRESS = 0x8000;
   private static final int kFPGA_INTERRUPT_ACKNOWLEDGE_ADDRESS = (kFPGA_INTERRUPT_BASE_ADDRESS + 0xC);

   //tInterruptHandler m_handler;
   private int m_interruptMask;
   //int _taskId;
   private IntByReference m_rioContext;
   private boolean m_watcher;
   private boolean m_enabled;
   //void *m_userParam;

   // maintain the interrupts that are already dealt with.
   private static int m_globalInterruptMask = 0;
   //static SEM_ID m_globalInterruptMaskSemaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);

   public tInterruptManager (int interruptMask, boolean watcher)
   {
      super();
      if (status.isFatal()) return;

      //m_handler = NULL;
      m_interruptMask = interruptMask;
      //m_taskId = INVALID_TASK_ID;
      m_rioContext = new IntByReference(0);
      m_watcher = watcher;
      m_enabled = false;
      //m_userParam = NULL;

      // Allocate the appropriate resources in the RIO driver.
      NiFpga.reserveIrqContext(m_DeviceHandle, m_rioContext, status);
   }

   protected void finalize()
   {

//      if (!m_watcher && isEnabled())
//      {
//         // Rendezvous with thread
//         disable();
//      }

      // Free the resources in the RIO driver
      NiFpga.unreserveIrqContext(m_DeviceHandle, m_rioContext, status);
      super.finalize();
   }

   public int watch(int timeoutInMs)
   {
      if (!m_watcher)
      {
         status.setStatus(NiRioStatus.kRIOStatusIrrelevantAttribute);
         return 0;
      }

      reserve();
      if (status.isFatal()) return 0;

      // Acknowldge any pending interrupts.
      acknowledge();

      int intsAsserted = NiFpga.waitOnIrqs(m_DeviceHandle, m_rioContext, m_interruptMask, timeoutInMs, status);
      acknowledge();
      unreserve();

      return intsAsserted;
   }

   protected void acknowledge()
   {
      NiFpga.writeU32(m_DeviceHandle, kFPGA_INTERRUPT_ACKNOWLEDGE_ADDRESS, m_interruptMask, status);
   }

   protected void reserve()
   {
      // TODO: synchronize me
      //semTake(_globalInterruptMaskSemaphore, WAIT_FOREVER);
      if ((m_globalInterruptMask & m_interruptMask) != 0 || m_enabled)
      {
         // Don't look at this interrupt if someone else already is.
         status.setStatus(NiRioStatus.kRIOStatusEventEnabled);
      }
      else
      {
         m_globalInterruptMask |= m_interruptMask;
         m_enabled = true;
      }
      //semGive(_globalInterruptMaskSemaphore);
   }

   protected void unreserve()
   {
      // TODO: synchronize me
      //semTake(_globalInterruptMaskSemaphore, WAIT_FOREVER);
      if (!m_enabled)
      {
         // Don't try to disable if we were never enabled.
         status.setStatus(NiRioStatus.kRIOStatusEventNotEnabled);
      }
      else
      {
         m_enabled = false;
         m_globalInterruptMask &= ~m_interruptMask;
      }
      //semGive(_globalInterruptMaskSemaphore);
   }

}

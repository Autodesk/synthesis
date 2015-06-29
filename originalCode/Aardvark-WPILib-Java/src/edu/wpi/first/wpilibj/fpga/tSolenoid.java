// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;

public class tSolenoid extends tSystem
{

   public tSolenoid()
   {
      super();

   }

   protected void finalize()
   {
      super.finalize();
   }

   public static final int kNumSystems = 1;





//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for DO7_0
//////////////////////////////////////////////////////////////////////////////////////////////////
   public static final int kDO7_0_NumElements = 2;
   public static final int kDO7_0_ElementSize = 8;
   public static final int kDO7_0_ElementMask = 0xFF;
   private static final int kSolenoid_DO7_0_Address = 0x8144;

   public static void writeDO7_0(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kDO7_0_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kSolenoid_DO7_0_Address, status);
      regValue &= ~(kDO7_0_ElementMask << ((kDO7_0_NumElements - 1 - bitfield_index) * kDO7_0_ElementSize));
      regValue |= ((value & kDO7_0_ElementMask) << ((kDO7_0_NumElements - 1 - bitfield_index) * kDO7_0_ElementSize));
      NiFpga.writeU32(m_DeviceHandle, kSolenoid_DO7_0_Address, regValue, status);
   }
   public static short readDO7_0(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kDO7_0_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kSolenoid_DO7_0_Address, status);
      int arrayElementValue = ((result)
          >>> ((kDO7_0_NumElements - 1 - bitfield_index) * kDO7_0_ElementSize)) & kDO7_0_ElementMask;
      return (short)((arrayElementValue) & 0x000000FF);
   }




}

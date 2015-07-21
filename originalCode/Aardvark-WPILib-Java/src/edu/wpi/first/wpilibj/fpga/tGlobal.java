// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;

public class tGlobal extends tSystem
{

   public tGlobal()
   {
      super();

   }

   protected void finalize()
   {
      super.finalize();
   }

   public static final int kNumSystems = 1;








//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Version
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kGlobal_Version_Address = 0x8118;

   public static int readVersion()
   {

      return (int)((NiFpga.readU32(m_DeviceHandle, kGlobal_Version_Address, status)) & 0x0000FFFF);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for LocalTime
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kGlobal_LocalTime_Address = 0x8110;

   public static long readLocalTime()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kGlobal_LocalTime_Address, status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for FPGA_LED
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kGlobal_FPGA_LED_Address = 0x810C;

   public static void writeFPGA_LED(final boolean value)
   {

      NiFpga.writeU32(m_DeviceHandle, kGlobal_FPGA_LED_Address, (value ? 1 : 0), status);
   }
   public static boolean readFPGA_LED()
   {

      return ((NiFpga.readU32(m_DeviceHandle, kGlobal_FPGA_LED_Address, status)) != 0 ? true : false);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Revision
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kGlobal_Revision_Address = 0x8114;

   public static long readRevision()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kGlobal_Revision_Address, status)) & 0xFFFFFFFFl);
   }




}

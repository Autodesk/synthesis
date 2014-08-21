// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;

public class tWatchdog extends tSystem
{

   public tWatchdog()
   {
      super();

   }

   protected void finalize()
   {
      super.finalize();
   }

   public static final int kNumSystems = 1;










//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Status
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kStatus_SystemActive_BitfieldMask = 0x80000000;
   private static final int kStatus_SystemActive_BitfieldOffset = 31;
   private static final int kStatus_Alive_BitfieldMask = 0x40000000;
   private static final int kStatus_Alive_BitfieldOffset = 30;
   private static final int kStatus_SysDisableCount_BitfieldMask = 0x3FFF8000;
   private static final int kStatus_SysDisableCount_BitfieldOffset = 15;
   private static final int kStatus_DisableCount_BitfieldMask = 0x00007FFF;
   private static final int kStatus_DisableCount_BitfieldOffset = 0;
   private static final int kWatchdog_Status_Address = 0x811C;

   public static int readStatus()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kWatchdog_Status_Address, status);
      int regValue = result ;
      return (int)(regValue);
   }
   public static boolean readStatus_SystemActive()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kWatchdog_Status_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kStatus_SystemActive_BitfieldMask) >>> kStatus_SystemActive_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readStatus_Alive()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kWatchdog_Status_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kStatus_Alive_BitfieldMask) >>> kStatus_Alive_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static short readStatus_SysDisableCount()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kWatchdog_Status_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kStatus_SysDisableCount_BitfieldMask) >>> kStatus_SysDisableCount_BitfieldOffset);
      return (short)((bitfieldValue) & 0x00007FFF);
   }
   public static short readStatus_DisableCount()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kWatchdog_Status_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kStatus_DisableCount_BitfieldMask) >>> kStatus_DisableCount_BitfieldOffset);
      return (short)((bitfieldValue) & 0x00007FFF);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Kill
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kWatchdog_Kill_Address = 0x8124;

   public static void strobeKill()
   {

       NiFpga.writeU32(m_DeviceHandle, kWatchdog_Kill_Address, 1, status);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Feed
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kWatchdog_Feed_Address = 0x8120;

   public static void strobeFeed()
   {

       NiFpga.writeU32(m_DeviceHandle, kWatchdog_Feed_Address, 1, status);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Timer
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kWatchdog_Timer_Address = 0x8128;

   public static long readTimer()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kWatchdog_Timer_Address, status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Expiration
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kWatchdog_Expiration_Address = 0x812C;

   public static void writeExpiration(final long value)
   {

      NiFpga.writeU32(m_DeviceHandle, kWatchdog_Expiration_Address, (int)(value), status);
   }
   public static long readExpiration()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kWatchdog_Expiration_Address, status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Immortal
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kWatchdog_Immortal_Address = 0x8130;

   public static void writeImmortal(final boolean value)
   {

      NiFpga.writeU32(m_DeviceHandle, kWatchdog_Immortal_Address, (value ? 1 : 0), status);
   }
   public static boolean readImmortal()
   {

      return ((NiFpga.readU32(m_DeviceHandle, kWatchdog_Immortal_Address, status)) != 0 ? true : false);
   }




}

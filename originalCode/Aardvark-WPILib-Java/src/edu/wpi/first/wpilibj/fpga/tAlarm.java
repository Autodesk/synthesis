// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;

public class tAlarm extends tSystem
{

   public tAlarm()
   {
      super();

   }

   protected void finalize()
   {
      super.finalize();
   }

   public static final int kNumSystems = 1;






//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Enable
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kAlarm_Enable_Address = 0x8448;

   public static void writeEnable(final boolean value)
   {

      NiFpga.writeU32(m_DeviceHandle, kAlarm_Enable_Address, (value ? 1 : 0), status);
   }
   public static boolean readEnable()
   {

      return ((NiFpga.readU32(m_DeviceHandle, kAlarm_Enable_Address, status)) != 0 ? true : false);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for TriggerTime
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kAlarm_TriggerTime_Address = 0x8444;

   public static void writeTriggerTime(final long value)
   {

      NiFpga.writeU32(m_DeviceHandle, kAlarm_TriggerTime_Address, (int)(value), status);
   }
   public static long readTriggerTime()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kAlarm_TriggerTime_Address, status)) & 0xFFFFFFFFl);
   }




}

// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;

public class tDMA extends tSystem
{

   public tDMA()
   {
      super();

   }

   protected void finalize()
   {
      super.finalize();
   }

   public static final int kNumSystems = 1;







//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Rate
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kDMA_Rate_Address = 0x8410;

   public static void writeRate(final long value)
   {

      NiFpga.writeU32(m_DeviceHandle, kDMA_Rate_Address, (int)(value), status);
   }
   public static long readRate()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kDMA_Rate_Address, status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Config
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kConfig_Pause_BitfieldMask = 0x00080000;
   private static final int kConfig_Pause_BitfieldOffset = 19;
   private static final int kConfig_Enable_AI0_Low_BitfieldMask = 0x00040000;
   private static final int kConfig_Enable_AI0_Low_BitfieldOffset = 18;
   private static final int kConfig_Enable_AI0_High_BitfieldMask = 0x00020000;
   private static final int kConfig_Enable_AI0_High_BitfieldOffset = 17;
   private static final int kConfig_Enable_AIAveraged0_Low_BitfieldMask = 0x00010000;
   private static final int kConfig_Enable_AIAveraged0_Low_BitfieldOffset = 16;
   private static final int kConfig_Enable_AIAveraged0_High_BitfieldMask = 0x00008000;
   private static final int kConfig_Enable_AIAveraged0_High_BitfieldOffset = 15;
   private static final int kConfig_Enable_AI1_Low_BitfieldMask = 0x00004000;
   private static final int kConfig_Enable_AI1_Low_BitfieldOffset = 14;
   private static final int kConfig_Enable_AI1_High_BitfieldMask = 0x00002000;
   private static final int kConfig_Enable_AI1_High_BitfieldOffset = 13;
   private static final int kConfig_Enable_AIAveraged1_Low_BitfieldMask = 0x00001000;
   private static final int kConfig_Enable_AIAveraged1_Low_BitfieldOffset = 12;
   private static final int kConfig_Enable_AIAveraged1_High_BitfieldMask = 0x00000800;
   private static final int kConfig_Enable_AIAveraged1_High_BitfieldOffset = 11;
   private static final int kConfig_Enable_Accumulator0_BitfieldMask = 0x00000400;
   private static final int kConfig_Enable_Accumulator0_BitfieldOffset = 10;
   private static final int kConfig_Enable_Accumulator1_BitfieldMask = 0x00000200;
   private static final int kConfig_Enable_Accumulator1_BitfieldOffset = 9;
   private static final int kConfig_Enable_DI_BitfieldMask = 0x00000100;
   private static final int kConfig_Enable_DI_BitfieldOffset = 8;
   private static final int kConfig_Enable_AnalogTriggers_BitfieldMask = 0x00000080;
   private static final int kConfig_Enable_AnalogTriggers_BitfieldOffset = 7;
   private static final int kConfig_Enable_Counters_Low_BitfieldMask = 0x00000040;
   private static final int kConfig_Enable_Counters_Low_BitfieldOffset = 6;
   private static final int kConfig_Enable_Counters_High_BitfieldMask = 0x00000020;
   private static final int kConfig_Enable_Counters_High_BitfieldOffset = 5;
   private static final int kConfig_Enable_CounterTimers_Low_BitfieldMask = 0x00000010;
   private static final int kConfig_Enable_CounterTimers_Low_BitfieldOffset = 4;
   private static final int kConfig_Enable_CounterTimers_High_BitfieldMask = 0x00000008;
   private static final int kConfig_Enable_CounterTimers_High_BitfieldOffset = 3;
   private static final int kConfig_Enable_Encoders_BitfieldMask = 0x00000004;
   private static final int kConfig_Enable_Encoders_BitfieldOffset = 2;
   private static final int kConfig_Enable_EncoderTimers_BitfieldMask = 0x00000002;
   private static final int kConfig_Enable_EncoderTimers_BitfieldOffset = 1;
   private static final int kConfig_ExternalClock_BitfieldMask = 0x00000001;
   private static final int kConfig_ExternalClock_BitfieldOffset = 0;
   private static final int kDMA_Config_Address = 0x8414;

   public static void writeConfig(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, value, status);
   }
   public static void writeConfig_Pause(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Pause_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Pause_BitfieldOffset) & kConfig_Pause_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AI0_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AI0_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AI0_Low_BitfieldOffset) & kConfig_Enable_AI0_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AI0_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AI0_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AI0_High_BitfieldOffset) & kConfig_Enable_AI0_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AIAveraged0_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AIAveraged0_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AIAveraged0_Low_BitfieldOffset) & kConfig_Enable_AIAveraged0_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AIAveraged0_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AIAveraged0_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AIAveraged0_High_BitfieldOffset) & kConfig_Enable_AIAveraged0_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AI1_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AI1_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AI1_Low_BitfieldOffset) & kConfig_Enable_AI1_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AI1_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AI1_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AI1_High_BitfieldOffset) & kConfig_Enable_AI1_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AIAveraged1_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AIAveraged1_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AIAveraged1_Low_BitfieldOffset) & kConfig_Enable_AIAveraged1_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AIAveraged1_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AIAveraged1_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AIAveraged1_High_BitfieldOffset) & kConfig_Enable_AIAveraged1_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_Accumulator0(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_Accumulator0_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_Accumulator0_BitfieldOffset) & kConfig_Enable_Accumulator0_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_Accumulator1(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_Accumulator1_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_Accumulator1_BitfieldOffset) & kConfig_Enable_Accumulator1_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_DI(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_DI_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_DI_BitfieldOffset) & kConfig_Enable_DI_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_AnalogTriggers(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_AnalogTriggers_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_AnalogTriggers_BitfieldOffset) & kConfig_Enable_AnalogTriggers_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_Counters_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_Counters_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_Counters_Low_BitfieldOffset) & kConfig_Enable_Counters_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_Counters_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_Counters_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_Counters_High_BitfieldOffset) & kConfig_Enable_Counters_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_CounterTimers_Low(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_CounterTimers_Low_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_CounterTimers_Low_BitfieldOffset) & kConfig_Enable_CounterTimers_Low_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_CounterTimers_High(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_CounterTimers_High_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_CounterTimers_High_BitfieldOffset) & kConfig_Enable_CounterTimers_High_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_Encoders(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_Encoders_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_Encoders_BitfieldOffset) & kConfig_Enable_Encoders_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_Enable_EncoderTimers(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_Enable_EncoderTimers_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_EncoderTimers_BitfieldOffset) & kConfig_Enable_EncoderTimers_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static void writeConfig_ExternalClock(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      regValue &= ~kConfig_ExternalClock_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_ExternalClock_BitfieldOffset) & kConfig_ExternalClock_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_Config_Address, regValue, status);
   }
   public static int readConfig()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      return (int)(regValue);
   }
   public static boolean readConfig_Pause()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Pause_BitfieldMask) >>> kConfig_Pause_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AI0_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AI0_Low_BitfieldMask) >>> kConfig_Enable_AI0_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AI0_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AI0_High_BitfieldMask) >>> kConfig_Enable_AI0_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AIAveraged0_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AIAveraged0_Low_BitfieldMask) >>> kConfig_Enable_AIAveraged0_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AIAveraged0_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AIAveraged0_High_BitfieldMask) >>> kConfig_Enable_AIAveraged0_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AI1_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AI1_Low_BitfieldMask) >>> kConfig_Enable_AI1_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AI1_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AI1_High_BitfieldMask) >>> kConfig_Enable_AI1_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AIAveraged1_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AIAveraged1_Low_BitfieldMask) >>> kConfig_Enable_AIAveraged1_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AIAveraged1_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AIAveraged1_High_BitfieldMask) >>> kConfig_Enable_AIAveraged1_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_Accumulator0()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_Accumulator0_BitfieldMask) >>> kConfig_Enable_Accumulator0_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_Accumulator1()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_Accumulator1_BitfieldMask) >>> kConfig_Enable_Accumulator1_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_DI()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_DI_BitfieldMask) >>> kConfig_Enable_DI_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_AnalogTriggers()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_AnalogTriggers_BitfieldMask) >>> kConfig_Enable_AnalogTriggers_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_Counters_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_Counters_Low_BitfieldMask) >>> kConfig_Enable_Counters_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_Counters_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_Counters_High_BitfieldMask) >>> kConfig_Enable_Counters_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_CounterTimers_Low()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_CounterTimers_Low_BitfieldMask) >>> kConfig_Enable_CounterTimers_Low_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_CounterTimers_High()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_CounterTimers_High_BitfieldMask) >>> kConfig_Enable_CounterTimers_High_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_Encoders()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_Encoders_BitfieldMask) >>> kConfig_Enable_Encoders_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_Enable_EncoderTimers()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_EncoderTimers_BitfieldMask) >>> kConfig_Enable_EncoderTimers_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readConfig_ExternalClock()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_Config_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_ExternalClock_BitfieldMask) >>> kConfig_ExternalClock_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for ExternalTriggers
//////////////////////////////////////////////////////////////////////////////////////////////////
   public static final int kExternalTriggers_NumElements = 4;
   public static final int kExternalTriggers_ElementSize = 8;
   public static final int kExternalTriggers_ElementMask = 0xFF;
   private static final int kExternalTriggers_ExternalClockSource_Channel_BitfieldMask = 0x000000F0;
   private static final int kExternalTriggers_ExternalClockSource_Channel_BitfieldOffset = 4;
   private static final int kExternalTriggers_ExternalClockSource_Module_BitfieldMask = 0x00000008;
   private static final int kExternalTriggers_ExternalClockSource_Module_BitfieldOffset = 3;
   private static final int kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldMask = 0x00000004;
   private static final int kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldOffset = 2;
   private static final int kExternalTriggers_RisingEdge_BitfieldMask = 0x00000002;
   private static final int kExternalTriggers_RisingEdge_BitfieldOffset = 1;
   private static final int kExternalTriggers_FallingEdge_BitfieldMask = 0x00000001;
   private static final int kExternalTriggers_FallingEdge_BitfieldOffset = 0;
   private static final int kDMA_ExternalTriggers_Address = 0x844C;

   public static void writeExternalTriggers(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, value, status);
   }
   public static void writeExternalTriggers_ExternalClockSource_Channel(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      regValue &= ~kExternalTriggers_ExternalClockSource_Channel_BitfieldMask;
      regValue |= ((value) << kExternalTriggers_ExternalClockSource_Channel_BitfieldOffset) & kExternalTriggers_ExternalClockSource_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, regValue, status);
   }
   public static void writeExternalTriggers_ExternalClockSource_Module(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      regValue &= ~kExternalTriggers_ExternalClockSource_Module_BitfieldMask;
      regValue |= ((value) << kExternalTriggers_ExternalClockSource_Module_BitfieldOffset) & kExternalTriggers_ExternalClockSource_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, regValue, status);
   }
   public static void writeExternalTriggers_ExternalClockSource_AnalogTrigger(final int bitfield_index, final boolean value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      regValue &= ~kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldOffset) & kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, regValue, status);
   }
   public static void writeExternalTriggers_RisingEdge(final int bitfield_index, final boolean value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      regValue &= ~kExternalTriggers_RisingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kExternalTriggers_RisingEdge_BitfieldOffset) & kExternalTriggers_RisingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, regValue, status);
   }
   public static void writeExternalTriggers_FallingEdge(final int bitfield_index, final boolean value)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      regValue &= ~kExternalTriggers_FallingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kExternalTriggers_FallingEdge_BitfieldOffset) & kExternalTriggers_FallingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, regValue, status);
   }
   public static int readExternalTriggers(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      return (int)(regValue);
   }
   public static byte readExternalTriggers_ExternalClockSource_Channel(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      int bitfieldValue = ((regValue & kExternalTriggers_ExternalClockSource_Channel_BitfieldMask) >>> kExternalTriggers_ExternalClockSource_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000000F);
   }
   public static byte readExternalTriggers_ExternalClockSource_Module(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      int bitfieldValue = ((regValue & kExternalTriggers_ExternalClockSource_Module_BitfieldMask) >>> kExternalTriggers_ExternalClockSource_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public static boolean readExternalTriggers_ExternalClockSource_AnalogTrigger(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      int bitfieldValue = ((regValue & kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldMask) >>> kExternalTriggers_ExternalClockSource_AnalogTrigger_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readExternalTriggers_RisingEdge(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      int bitfieldValue = ((regValue & kExternalTriggers_RisingEdge_BitfieldMask) >>> kExternalTriggers_RisingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public static boolean readExternalTriggers_FallingEdge(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kExternalTriggers_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kDMA_ExternalTriggers_Address, status);
      int regValue = result  >>> ((kExternalTriggers_NumElements - 1 - bitfield_index) * kExternalTriggers_ElementSize);
      int bitfieldValue = ((regValue & kExternalTriggers_FallingEdge_BitfieldMask) >>> kExternalTriggers_FallingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }




}

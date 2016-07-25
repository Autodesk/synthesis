// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;

public class tCounter extends tSystem
{

   public tCounter(final int sys_index)
   {
      super();
      m_SystemIndex = sys_index;
      if (status.isNotFatal() && m_SystemIndex >= kNumSystems)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

   }

   protected void finalize()
   {
      super.finalize();
   }

   public int getSystemIndex()
   {
      return m_SystemIndex;
   }

   public static final int kNumSystems = 8;
   public final int m_SystemIndex;








//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Output
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kOutput_Direction_BitfieldMask = 0x80000000;
   private static final int kOutput_Direction_BitfieldOffset = 31;
   private static final int kOutput_Value_BitfieldMask = 0x7FFFFFFF;
   private static final int kOutput_Value_BitfieldOffset = 0;
   private static final int kCounter0_Output_Address = 0x82E8;
   private static final int kCounter1_Output_Address = 0x82FC;
   private static final int kCounter2_Output_Address = 0x8310;
   private static final int kCounter3_Output_Address = 0x8324;
   private static final int kCounter4_Output_Address = 0x8338;
   private static final int kCounter5_Output_Address = 0x834C;
   private static final int kCounter6_Output_Address = 0x8360;
   private static final int kCounter7_Output_Address = 0x8374;
   private static final int kOutput_Addresses [] =
   {
      kCounter0_Output_Address,
      kCounter1_Output_Address,
      kCounter2_Output_Address,
      kCounter3_Output_Address,
      kCounter4_Output_Address,
      kCounter5_Output_Address,
      kCounter6_Output_Address,
      kCounter7_Output_Address,
   };

   public int readOutput()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public boolean readOutput_Direction()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kOutput_Direction_BitfieldMask) >>> kOutput_Direction_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public int readOutput_Value()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kOutput_Value_BitfieldMask) >>> kOutput_Value_BitfieldOffset);
      // Sign extension
      bitfieldValue <<= 1;
      bitfieldValue >>= 1;
      return (int)(bitfieldValue);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Config
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kConfig_UpSource_Channel_BitfieldMask = 0xF0000000;
   private static final int kConfig_UpSource_Channel_BitfieldOffset = 28;
   private static final int kConfig_UpSource_Module_BitfieldMask = 0x08000000;
   private static final int kConfig_UpSource_Module_BitfieldOffset = 27;
   private static final int kConfig_UpSource_AnalogTrigger_BitfieldMask = 0x04000000;
   private static final int kConfig_UpSource_AnalogTrigger_BitfieldOffset = 26;
   private static final int kConfig_DownSource_Channel_BitfieldMask = 0x03C00000;
   private static final int kConfig_DownSource_Channel_BitfieldOffset = 22;
   private static final int kConfig_DownSource_Module_BitfieldMask = 0x00200000;
   private static final int kConfig_DownSource_Module_BitfieldOffset = 21;
   private static final int kConfig_DownSource_AnalogTrigger_BitfieldMask = 0x00100000;
   private static final int kConfig_DownSource_AnalogTrigger_BitfieldOffset = 20;
   private static final int kConfig_IndexSource_Channel_BitfieldMask = 0x000F0000;
   private static final int kConfig_IndexSource_Channel_BitfieldOffset = 16;
   private static final int kConfig_IndexSource_Module_BitfieldMask = 0x00008000;
   private static final int kConfig_IndexSource_Module_BitfieldOffset = 15;
   private static final int kConfig_IndexSource_AnalogTrigger_BitfieldMask = 0x00004000;
   private static final int kConfig_IndexSource_AnalogTrigger_BitfieldOffset = 14;
   private static final int kConfig_IndexActiveHigh_BitfieldMask = 0x00002000;
   private static final int kConfig_IndexActiveHigh_BitfieldOffset = 13;
   private static final int kConfig_UpRisingEdge_BitfieldMask = 0x00001000;
   private static final int kConfig_UpRisingEdge_BitfieldOffset = 12;
   private static final int kConfig_UpFallingEdge_BitfieldMask = 0x00000800;
   private static final int kConfig_UpFallingEdge_BitfieldOffset = 11;
   private static final int kConfig_DownRisingEdge_BitfieldMask = 0x00000400;
   private static final int kConfig_DownRisingEdge_BitfieldOffset = 10;
   private static final int kConfig_DownFallingEdge_BitfieldMask = 0x00000200;
   private static final int kConfig_DownFallingEdge_BitfieldOffset = 9;
   private static final int kConfig_Mode_BitfieldMask = 0x00000180;
   private static final int kConfig_Mode_BitfieldOffset = 7;
   private static final int kConfig_PulseLengthThreshold_BitfieldMask = 0x0000007E;
   private static final int kConfig_PulseLengthThreshold_BitfieldOffset = 1;
   private static final int kConfig_PulseLengthThreshold_FixedPointIntegerShift = 8;
   private static final int kConfig_Enable_BitfieldMask = 0x00000001;
   private static final int kConfig_Enable_BitfieldOffset = 0;
   private static final int kCounter0_Config_Address = 0x82E0;
   private static final int kCounter1_Config_Address = 0x82F4;
   private static final int kCounter2_Config_Address = 0x8308;
   private static final int kCounter3_Config_Address = 0x831C;
   private static final int kCounter4_Config_Address = 0x8330;
   private static final int kCounter5_Config_Address = 0x8344;
   private static final int kCounter6_Config_Address = 0x8358;
   private static final int kCounter7_Config_Address = 0x836C;
   private static final int kConfig_Addresses [] =
   {
      kCounter0_Config_Address,
      kCounter1_Config_Address,
      kCounter2_Config_Address,
      kCounter3_Config_Address,
      kCounter4_Config_Address,
      kCounter5_Config_Address,
      kCounter6_Config_Address,
      kCounter7_Config_Address,
   };

   public void writeConfig(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], value, status);
   }
   public void writeConfig_UpSource_Channel(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_UpSource_Channel_BitfieldMask;
      regValue |= ((value) << kConfig_UpSource_Channel_BitfieldOffset) & kConfig_UpSource_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_UpSource_Module(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_UpSource_Module_BitfieldMask;
      regValue |= ((value) << kConfig_UpSource_Module_BitfieldOffset) & kConfig_UpSource_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_UpSource_AnalogTrigger(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_UpSource_AnalogTrigger_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_UpSource_AnalogTrigger_BitfieldOffset) & kConfig_UpSource_AnalogTrigger_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_DownSource_Channel(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_DownSource_Channel_BitfieldMask;
      regValue |= ((value) << kConfig_DownSource_Channel_BitfieldOffset) & kConfig_DownSource_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_DownSource_Module(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_DownSource_Module_BitfieldMask;
      regValue |= ((value) << kConfig_DownSource_Module_BitfieldOffset) & kConfig_DownSource_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_DownSource_AnalogTrigger(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_DownSource_AnalogTrigger_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_DownSource_AnalogTrigger_BitfieldOffset) & kConfig_DownSource_AnalogTrigger_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_IndexSource_Channel(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_IndexSource_Channel_BitfieldMask;
      regValue |= ((value) << kConfig_IndexSource_Channel_BitfieldOffset) & kConfig_IndexSource_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_IndexSource_Module(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_IndexSource_Module_BitfieldMask;
      regValue |= ((value) << kConfig_IndexSource_Module_BitfieldOffset) & kConfig_IndexSource_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_IndexSource_AnalogTrigger(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_IndexSource_AnalogTrigger_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_IndexSource_AnalogTrigger_BitfieldOffset) & kConfig_IndexSource_AnalogTrigger_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_IndexActiveHigh(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_IndexActiveHigh_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_IndexActiveHigh_BitfieldOffset) & kConfig_IndexActiveHigh_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_UpRisingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_UpRisingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_UpRisingEdge_BitfieldOffset) & kConfig_UpRisingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_UpFallingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_UpFallingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_UpFallingEdge_BitfieldOffset) & kConfig_UpFallingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_DownRisingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_DownRisingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_DownRisingEdge_BitfieldOffset) & kConfig_DownRisingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_DownFallingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_DownFallingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_DownFallingEdge_BitfieldOffset) & kConfig_DownFallingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_Mode(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_Mode_BitfieldMask;
      regValue |= ((value) << kConfig_Mode_BitfieldOffset) & kConfig_Mode_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_PulseLengthThreshold(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_PulseLengthThreshold_BitfieldMask;
      regValue |= ((value >>> kConfig_PulseLengthThreshold_FixedPointIntegerShift) << kConfig_PulseLengthThreshold_BitfieldOffset) & kConfig_PulseLengthThreshold_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_Enable(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_Enable_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Enable_BitfieldOffset) & kConfig_Enable_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public int readConfig()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public byte readConfig_UpSource_Channel()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_UpSource_Channel_BitfieldMask) >>> kConfig_UpSource_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000000F);
   }
   public byte readConfig_UpSource_Module()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_UpSource_Module_BitfieldMask) >>> kConfig_UpSource_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public boolean readConfig_UpSource_AnalogTrigger()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_UpSource_AnalogTrigger_BitfieldMask) >>> kConfig_UpSource_AnalogTrigger_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public byte readConfig_DownSource_Channel()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_DownSource_Channel_BitfieldMask) >>> kConfig_DownSource_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000000F);
   }
   public byte readConfig_DownSource_Module()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_DownSource_Module_BitfieldMask) >>> kConfig_DownSource_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public boolean readConfig_DownSource_AnalogTrigger()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_DownSource_AnalogTrigger_BitfieldMask) >>> kConfig_DownSource_AnalogTrigger_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public byte readConfig_IndexSource_Channel()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_IndexSource_Channel_BitfieldMask) >>> kConfig_IndexSource_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000000F);
   }
   public byte readConfig_IndexSource_Module()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_IndexSource_Module_BitfieldMask) >>> kConfig_IndexSource_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public boolean readConfig_IndexSource_AnalogTrigger()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_IndexSource_AnalogTrigger_BitfieldMask) >>> kConfig_IndexSource_AnalogTrigger_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_IndexActiveHigh()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_IndexActiveHigh_BitfieldMask) >>> kConfig_IndexActiveHigh_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_UpRisingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_UpRisingEdge_BitfieldMask) >>> kConfig_UpRisingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_UpFallingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_UpFallingEdge_BitfieldMask) >>> kConfig_UpFallingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_DownRisingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_DownRisingEdge_BitfieldMask) >>> kConfig_DownRisingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_DownFallingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_DownFallingEdge_BitfieldMask) >>> kConfig_DownFallingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public byte readConfig_Mode()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Mode_BitfieldMask) >>> kConfig_Mode_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000003);
   }
   public short readConfig_PulseLengthThreshold()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_PulseLengthThreshold_BitfieldMask) >>> kConfig_PulseLengthThreshold_BitfieldOffset) << kConfig_PulseLengthThreshold_FixedPointIntegerShift;
      return (short)((bitfieldValue) & 0x00003FFF);
   }
   public boolean readConfig_Enable()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Enable_BitfieldMask) >>> kConfig_Enable_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for TimerOutput
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kTimerOutput_Period_BitfieldMask = 0xFFFFFE00;
   private static final int kTimerOutput_Period_BitfieldOffset = 9;
   private static final int kTimerOutput_Period_FixedPointIntegerShift = 1;
   private static final int kTimerOutput_Count_BitfieldMask = 0x000001FE;
   private static final int kTimerOutput_Count_BitfieldOffset = 1;
   private static final int kTimerOutput_Stalled_BitfieldMask = 0x00000001;
   private static final int kTimerOutput_Stalled_BitfieldOffset = 0;
   private static final int kCounter0_TimerOutput_Address = 0x82F0;
   private static final int kCounter1_TimerOutput_Address = 0x8304;
   private static final int kCounter2_TimerOutput_Address = 0x8318;
   private static final int kCounter3_TimerOutput_Address = 0x832C;
   private static final int kCounter4_TimerOutput_Address = 0x8340;
   private static final int kCounter5_TimerOutput_Address = 0x8354;
   private static final int kCounter6_TimerOutput_Address = 0x8368;
   private static final int kCounter7_TimerOutput_Address = 0x837C;
   private static final int kTimerOutput_Addresses [] =
   {
      kCounter0_TimerOutput_Address,
      kCounter1_TimerOutput_Address,
      kCounter2_TimerOutput_Address,
      kCounter3_TimerOutput_Address,
      kCounter4_TimerOutput_Address,
      kCounter5_TimerOutput_Address,
      kCounter6_TimerOutput_Address,
      kCounter7_TimerOutput_Address,
   };

   public int readTimerOutput()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public int readTimerOutput_Period()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerOutput_Period_BitfieldMask) >>> kTimerOutput_Period_BitfieldOffset) << kTimerOutput_Period_FixedPointIntegerShift;
      return (int)((bitfieldValue) & 0x00FFFFFF);
   }
   public byte readTimerOutput_Count()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerOutput_Count_BitfieldMask) >>> kTimerOutput_Count_BitfieldOffset);
      // Sign extension
      bitfieldValue <<= 24;
      bitfieldValue >>= 24;
      return (byte)(bitfieldValue);
   }
   public boolean readTimerOutput_Stalled()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerOutput_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerOutput_Stalled_BitfieldMask) >>> kTimerOutput_Stalled_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Reset
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kCounter0_Reset_Address = 0x82E4;
   private static final int kCounter1_Reset_Address = 0x82F8;
   private static final int kCounter2_Reset_Address = 0x830C;
   private static final int kCounter3_Reset_Address = 0x8320;
   private static final int kCounter4_Reset_Address = 0x8334;
   private static final int kCounter5_Reset_Address = 0x8348;
   private static final int kCounter6_Reset_Address = 0x835C;
   private static final int kCounter7_Reset_Address = 0x8370;
   private static final int kReset_Addresses [] =
   {
      kCounter0_Reset_Address,
      kCounter1_Reset_Address,
      kCounter2_Reset_Address,
      kCounter3_Reset_Address,
      kCounter4_Reset_Address,
      kCounter5_Reset_Address,
      kCounter6_Reset_Address,
      kCounter7_Reset_Address,
   };

   public void strobeReset()
   {

       NiFpga.writeU32(m_DeviceHandle, kReset_Addresses[m_SystemIndex], 1, status);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for TimerConfig
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kTimerConfig_StallPeriod_BitfieldMask = 0xFFFFFF00;
   private static final int kTimerConfig_StallPeriod_BitfieldOffset = 8;
   private static final int kTimerConfig_StallPeriod_FixedPointIntegerShift = 1;
   private static final int kTimerConfig_AverageSize_BitfieldMask = 0x000000FE;
   private static final int kTimerConfig_AverageSize_BitfieldOffset = 1;
   private static final int kTimerConfig_UpdateWhenEmpty_BitfieldMask = 0x00000001;
   private static final int kTimerConfig_UpdateWhenEmpty_BitfieldOffset = 0;
   private static final int kCounter0_TimerConfig_Address = 0x82EC;
   private static final int kCounter1_TimerConfig_Address = 0x8300;
   private static final int kCounter2_TimerConfig_Address = 0x8314;
   private static final int kCounter3_TimerConfig_Address = 0x8328;
   private static final int kCounter4_TimerConfig_Address = 0x833C;
   private static final int kCounter5_TimerConfig_Address = 0x8350;
   private static final int kCounter6_TimerConfig_Address = 0x8364;
   private static final int kCounter7_TimerConfig_Address = 0x8378;
   private static final int kTimerConfig_Addresses [] =
   {
      kCounter0_TimerConfig_Address,
      kCounter1_TimerConfig_Address,
      kCounter2_TimerConfig_Address,
      kCounter3_TimerConfig_Address,
      kCounter4_TimerConfig_Address,
      kCounter5_TimerConfig_Address,
      kCounter6_TimerConfig_Address,
      kCounter7_TimerConfig_Address,
   };

   public void writeTimerConfig(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], value, status);
   }
   public void writeTimerConfig_StallPeriod(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kTimerConfig_StallPeriod_BitfieldMask;
      regValue |= ((value >>> kTimerConfig_StallPeriod_FixedPointIntegerShift) << kTimerConfig_StallPeriod_BitfieldOffset) & kTimerConfig_StallPeriod_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeTimerConfig_AverageSize(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kTimerConfig_AverageSize_BitfieldMask;
      regValue |= ((value) << kTimerConfig_AverageSize_BitfieldOffset) & kTimerConfig_AverageSize_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeTimerConfig_UpdateWhenEmpty(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kTimerConfig_UpdateWhenEmpty_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kTimerConfig_UpdateWhenEmpty_BitfieldOffset) & kTimerConfig_UpdateWhenEmpty_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public int readTimerConfig()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public int readTimerConfig_StallPeriod()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerConfig_StallPeriod_BitfieldMask) >>> kTimerConfig_StallPeriod_BitfieldOffset) << kTimerConfig_StallPeriod_FixedPointIntegerShift;
      return (int)((bitfieldValue) & 0x01FFFFFF);
   }
   public byte readTimerConfig_AverageSize()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerConfig_AverageSize_BitfieldMask) >>> kTimerConfig_AverageSize_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000007F);
   }
   public boolean readTimerConfig_UpdateWhenEmpty()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kTimerConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kTimerConfig_UpdateWhenEmpty_BitfieldMask) >>> kTimerConfig_UpdateWhenEmpty_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }





}

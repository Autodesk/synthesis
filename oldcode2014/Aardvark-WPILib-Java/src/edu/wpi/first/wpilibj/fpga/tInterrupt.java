// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;

public class tInterrupt extends tSystem
{

   public tInterrupt(final int sys_index)
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
// Accessors for TimeStamp
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kInterrupt0_TimeStamp_Address = 0x83D4;
   private static final int kInterrupt1_TimeStamp_Address = 0x83DC;
   private static final int kInterrupt2_TimeStamp_Address = 0x83E4;
   private static final int kInterrupt3_TimeStamp_Address = 0x83EC;
   private static final int kInterrupt4_TimeStamp_Address = 0x83F4;
   private static final int kInterrupt5_TimeStamp_Address = 0x83FC;
   private static final int kInterrupt6_TimeStamp_Address = 0x8404;
   private static final int kInterrupt7_TimeStamp_Address = 0x840C;
   private static final int kTimeStamp_Addresses [] =
   {
      kInterrupt0_TimeStamp_Address,
      kInterrupt1_TimeStamp_Address,
      kInterrupt2_TimeStamp_Address,
      kInterrupt3_TimeStamp_Address,
      kInterrupt4_TimeStamp_Address,
      kInterrupt5_TimeStamp_Address,
      kInterrupt6_TimeStamp_Address,
      kInterrupt7_TimeStamp_Address,
   };

   public long readTimeStamp()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kTimeStamp_Addresses[m_SystemIndex], status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Config
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kConfig_Source_Channel_BitfieldMask = 0x000001E0;
   private static final int kConfig_Source_Channel_BitfieldOffset = 5;
   private static final int kConfig_Source_Module_BitfieldMask = 0x00000010;
   private static final int kConfig_Source_Module_BitfieldOffset = 4;
   private static final int kConfig_Source_AnalogTrigger_BitfieldMask = 0x00000008;
   private static final int kConfig_Source_AnalogTrigger_BitfieldOffset = 3;
   private static final int kConfig_RisingEdge_BitfieldMask = 0x00000004;
   private static final int kConfig_RisingEdge_BitfieldOffset = 2;
   private static final int kConfig_FallingEdge_BitfieldMask = 0x00000002;
   private static final int kConfig_FallingEdge_BitfieldOffset = 1;
   private static final int kConfig_WaitForAck_BitfieldMask = 0x00000001;
   private static final int kConfig_WaitForAck_BitfieldOffset = 0;
   private static final int kInterrupt0_Config_Address = 0x83D0;
   private static final int kInterrupt1_Config_Address = 0x83D8;
   private static final int kInterrupt2_Config_Address = 0x83E0;
   private static final int kInterrupt3_Config_Address = 0x83E8;
   private static final int kInterrupt4_Config_Address = 0x83F0;
   private static final int kInterrupt5_Config_Address = 0x83F8;
   private static final int kInterrupt6_Config_Address = 0x8400;
   private static final int kInterrupt7_Config_Address = 0x8408;
   private static final int kConfig_Addresses [] =
   {
      kInterrupt0_Config_Address,
      kInterrupt1_Config_Address,
      kInterrupt2_Config_Address,
      kInterrupt3_Config_Address,
      kInterrupt4_Config_Address,
      kInterrupt5_Config_Address,
      kInterrupt6_Config_Address,
      kInterrupt7_Config_Address,
   };

   public void writeConfig(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], value, status);
   }
   public void writeConfig_Source_Channel(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_Source_Channel_BitfieldMask;
      regValue |= ((value) << kConfig_Source_Channel_BitfieldOffset) & kConfig_Source_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_Source_Module(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_Source_Module_BitfieldMask;
      regValue |= ((value) << kConfig_Source_Module_BitfieldOffset) & kConfig_Source_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_Source_AnalogTrigger(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_Source_AnalogTrigger_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_Source_AnalogTrigger_BitfieldOffset) & kConfig_Source_AnalogTrigger_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_RisingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_RisingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_RisingEdge_BitfieldOffset) & kConfig_RisingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_FallingEdge(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_FallingEdge_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_FallingEdge_BitfieldOffset) & kConfig_FallingEdge_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_WaitForAck(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_WaitForAck_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kConfig_WaitForAck_BitfieldOffset) & kConfig_WaitForAck_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public int readConfig()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public byte readConfig_Source_Channel()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Source_Channel_BitfieldMask) >>> kConfig_Source_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x0000000F);
   }
   public byte readConfig_Source_Module()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Source_Module_BitfieldMask) >>> kConfig_Source_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public boolean readConfig_Source_AnalogTrigger()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_Source_AnalogTrigger_BitfieldMask) >>> kConfig_Source_AnalogTrigger_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_RisingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_RisingEdge_BitfieldMask) >>> kConfig_RisingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_FallingEdge()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_FallingEdge_BitfieldMask) >>> kConfig_FallingEdge_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }
   public boolean readConfig_WaitForAck()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_WaitForAck_BitfieldMask) >>> kConfig_WaitForAck_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }





}

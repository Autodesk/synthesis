// Copyright (c) National Instruments 2009.  All Rights Reserved.
// Do Not Edit... this file is generated!

package edu.wpi.first.wpilibj.fpga;

import com.ni.rio.NiFpga;
import com.ni.rio.NiRioStatus;

public class tAI extends tSystem
{

   public tAI(final int sys_index)
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

   public static final int kNumSystems = 2;
   public final int m_SystemIndex;











//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Config
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kConfig_ScanSize_BitfieldMask = 0x1C000000;
   private static final int kConfig_ScanSize_BitfieldOffset = 26;
   private static final int kConfig_ConvertRate_BitfieldMask = 0x03FFFFFF;
   private static final int kConfig_ConvertRate_BitfieldOffset = 0;
   private static final int kAI0_Config_Address = 0x8154;
   private static final int kAI1_Config_Address = 0x8168;
   private static final int kConfig_Addresses [] =
   {
      kAI0_Config_Address,
      kAI1_Config_Address,
   };

   public void writeConfig(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], value, status);
   }
   public void writeConfig_ScanSize(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_ScanSize_BitfieldMask;
      regValue |= ((value) << kConfig_ScanSize_BitfieldOffset) & kConfig_ScanSize_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public void writeConfig_ConvertRate(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      regValue &= ~kConfig_ConvertRate_BitfieldMask;
      regValue |= ((value) << kConfig_ConvertRate_BitfieldOffset) & kConfig_ConvertRate_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], regValue, status);
   }
   public int readConfig()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      return (int)(regValue);
   }
   public byte readConfig_ScanSize()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_ScanSize_BitfieldMask) >>> kConfig_ScanSize_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000007);
   }
   public int readConfig_ConvertRate()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kConfig_Addresses[m_SystemIndex], status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kConfig_ConvertRate_BitfieldMask) >>> kConfig_ConvertRate_BitfieldOffset);
      return (int)((bitfieldValue) & 0x03FFFFFF);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for LoopTiming
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kAI0_LoopTiming_Address = 0x8164;
   private static final int kAI1_LoopTiming_Address = 0x8178;
   private static final int kLoopTiming_Addresses [] =
   {
      kAI0_LoopTiming_Address,
      kAI1_LoopTiming_Address,
   };

   public long readLoopTiming()
   {

      return (long)((NiFpga.readU32(m_DeviceHandle, kLoopTiming_Addresses[m_SystemIndex], status)) & 0xFFFFFFFFl);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for OversampleBits
//////////////////////////////////////////////////////////////////////////////////////////////////
   public static final int kOversampleBits_NumElements = 8;
   public static final int kOversampleBits_ElementSize = 4;
   public static final int kOversampleBits_ElementMask = 0xF;
   private static final int kAI0_OversampleBits_Address = 0x815C;
   private static final int kAI1_OversampleBits_Address = 0x8170;
   private static final int kOversampleBits_Addresses [] =
   {
      kAI0_OversampleBits_Address,
      kAI1_OversampleBits_Address,
   };

   public void writeOversampleBits(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kOversampleBits_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kOversampleBits_Addresses[m_SystemIndex], status);
      regValue &= ~(kOversampleBits_ElementMask << ((kOversampleBits_NumElements - 1 - bitfield_index) * kOversampleBits_ElementSize));
      regValue |= ((value & kOversampleBits_ElementMask) << ((kOversampleBits_NumElements - 1 - bitfield_index) * kOversampleBits_ElementSize));
      NiFpga.writeU32(m_DeviceHandle, kOversampleBits_Addresses[m_SystemIndex], regValue, status);
   }
   public byte readOversampleBits(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kOversampleBits_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kOversampleBits_Addresses[m_SystemIndex], status);
      int arrayElementValue = ((result)
          >>> ((kOversampleBits_NumElements - 1 - bitfield_index) * kOversampleBits_ElementSize)) & kOversampleBits_ElementMask;
      return (byte)((arrayElementValue) & 0x0000000F);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for AverageBits
//////////////////////////////////////////////////////////////////////////////////////////////////
   public static final int kAverageBits_NumElements = 8;
   public static final int kAverageBits_ElementSize = 4;
   public static final int kAverageBits_ElementMask = 0xF;
   private static final int kAI0_AverageBits_Address = 0x8160;
   private static final int kAI1_AverageBits_Address = 0x8174;
   private static final int kAverageBits_Addresses [] =
   {
      kAI0_AverageBits_Address,
      kAI1_AverageBits_Address,
   };

   public void writeAverageBits(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kAverageBits_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kAverageBits_Addresses[m_SystemIndex], status);
      regValue &= ~(kAverageBits_ElementMask << ((kAverageBits_NumElements - 1 - bitfield_index) * kAverageBits_ElementSize));
      regValue |= ((value & kAverageBits_ElementMask) << ((kAverageBits_NumElements - 1 - bitfield_index) * kAverageBits_ElementSize));
      NiFpga.writeU32(m_DeviceHandle, kAverageBits_Addresses[m_SystemIndex], regValue, status);
   }
   public byte readAverageBits(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kAverageBits_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kAverageBits_Addresses[m_SystemIndex], status);
      int arrayElementValue = ((result)
          >>> ((kAverageBits_NumElements - 1 - bitfield_index) * kAverageBits_ElementSize)) & kAverageBits_ElementMask;
      return (byte)((arrayElementValue) & 0x0000000F);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for ScanList
//////////////////////////////////////////////////////////////////////////////////////////////////
   public static final int kScanList_NumElements = 8;
   public static final int kScanList_ElementSize = 3;
   public static final int kScanList_ElementMask = 0x7;
   private static final int kAI0_ScanList_Address = 0x8158;
   private static final int kAI1_ScanList_Address = 0x816C;
   private static final int kScanList_Addresses [] =
   {
      kAI0_ScanList_Address,
      kAI1_ScanList_Address,
   };

   public void writeScanList(final int bitfield_index, final int value)
   {
      if (status.isNotFatal() && bitfield_index >= kScanList_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int regValue = NiFpga.readU32(m_DeviceHandle, kScanList_Addresses[m_SystemIndex], status);
      regValue &= ~(kScanList_ElementMask << ((kScanList_NumElements - 1 - bitfield_index) * kScanList_ElementSize));
      regValue |= ((value & kScanList_ElementMask) << ((kScanList_NumElements - 1 - bitfield_index) * kScanList_ElementSize));
      NiFpga.writeU32(m_DeviceHandle, kScanList_Addresses[m_SystemIndex], regValue, status);
   }
   public byte readScanList(final int bitfield_index)
   {
      if (status.isNotFatal() && bitfield_index >= kScanList_NumElements)
      {
         status.setStatus(NiRioStatus.kRIOStatusBadSelector);
      }

      int result = NiFpga.readU32(m_DeviceHandle, kScanList_Addresses[m_SystemIndex], status);
      int arrayElementValue = ((result)
          >>> ((kScanList_NumElements - 1 - bitfield_index) * kScanList_ElementSize)) & kScanList_ElementMask;
      return (byte)((arrayElementValue) & 0x00000007);
   }


//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for Output
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kAI_Output_Address = 0x8150;

   public static int readOutput()
   {

      return (int)(NiFpga.readU32(m_DeviceHandle, kAI_Output_Address, status));
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for LatchOutput
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kAI_LatchOutput_Address = 0x814C;

   public static void strobeLatchOutput()
   {

       NiFpga.writeU32(m_DeviceHandle, kAI_LatchOutput_Address, 1, status);
   }

//////////////////////////////////////////////////////////////////////////////////////////////////
// Accessors for ReadSelect
//////////////////////////////////////////////////////////////////////////////////////////////////
   private static final int kReadSelect_Channel_BitfieldMask = 0x0000001C;
   private static final int kReadSelect_Channel_BitfieldOffset = 2;
   private static final int kReadSelect_Module_BitfieldMask = 0x00000002;
   private static final int kReadSelect_Module_BitfieldOffset = 1;
   private static final int kReadSelect_Averaged_BitfieldMask = 0x00000001;
   private static final int kReadSelect_Averaged_BitfieldOffset = 0;
   private static final int kAI_ReadSelect_Address = 0x8148;

   public static void writeReadSelect(final int value)
   {

      NiFpga.writeU32(m_DeviceHandle, kAI_ReadSelect_Address, value, status);
   }
   public static void writeReadSelect_Channel(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      regValue &= ~kReadSelect_Channel_BitfieldMask;
      regValue |= ((value) << kReadSelect_Channel_BitfieldOffset) & kReadSelect_Channel_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kAI_ReadSelect_Address, regValue, status);
   }
   public static void writeReadSelect_Module(final int value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      regValue &= ~kReadSelect_Module_BitfieldMask;
      regValue |= ((value) << kReadSelect_Module_BitfieldOffset) & kReadSelect_Module_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kAI_ReadSelect_Address, regValue, status);
   }
   public static void writeReadSelect_Averaged(final boolean value)
   {

      int regValue = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      regValue &= ~kReadSelect_Averaged_BitfieldMask;
      regValue |= (((value ? 1 : 0)) << kReadSelect_Averaged_BitfieldOffset) & kReadSelect_Averaged_BitfieldMask;
      NiFpga.writeU32(m_DeviceHandle, kAI_ReadSelect_Address, regValue, status);
   }
   public static int readReadSelect()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      int regValue = result ;
      return (int)(regValue);
   }
   public static byte readReadSelect_Channel()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kReadSelect_Channel_BitfieldMask) >>> kReadSelect_Channel_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000007);
   }
   public static byte readReadSelect_Module()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kReadSelect_Module_BitfieldMask) >>> kReadSelect_Module_BitfieldOffset);
      return (byte)((bitfieldValue) & 0x00000001);
   }
   public static boolean readReadSelect_Averaged()
   {

      int result = NiFpga.readU32(m_DeviceHandle, kAI_ReadSelect_Address, status);
      int regValue = result ;
      int bitfieldValue = ((regValue & kReadSelect_Averaged_BitfieldMask) >>> kReadSelect_Averaged_BitfieldOffset);
      return ((bitfieldValue) != 0 ? true : false);
   }




}

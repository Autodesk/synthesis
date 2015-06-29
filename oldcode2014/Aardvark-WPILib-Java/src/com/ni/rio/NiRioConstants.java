/**
    \file       NiRioConstants.h
    \author     Erik Hons <erik.hons@ni.com>
    \date       12/14/2004

    \brief Public constants for the RIO services

    � Copyright 2004. National Instruments. All rights reserved.
*/

package com.ni.rio;

public interface NiRioConstants
{

   //  defines...
   //  type declarations (typedefs)...

   //
   // Basic constants.
   //
   public static final int kRioInvalid = 0xffffffff;
   public static final int kRioMaxLen = 256;
   public static final int kRioInvalidHandle = 0;

   //
   // Device Access
   //

   public static final byte kRioDeviceAccessDeny  = '-';
   public static final byte kRioDeviceAccessAllow = '+';

   /**
         DevUserData registered data structure for non R-series devices that
         utilize RIO technology.
   */

   public static final int kRIODevUserDataKey  = 0x4352;  /// 'CR' for CompactRio

   //  constants...

   // Timeouts
   public static final int kRioTimeoutZero = 0;
   public static final int kRioTimeoutInfinite = 0xFFFFFFFF;

   //
   // Device attribute identifiers
   //
   // NOTE: *** Maintain the enum ordering for client compatibility. (e.g.: the
   // RIO attribute ring control in the FPGA plug-in code. ***
   //

   // 32-bit attributes
   // Name                        =  X, // AddedVersion: Comments
   public static final int kRioInitialized                =  0; // 200
   public static final int kRioInterfaceNum               =  1; // 200
   public static final int kRioProductNum                 =  2; // 200
   public static final int kRioVendorNum                  =  3; // 200
   public static final int kRioSerialNum                  =  4; // 200
   public static final int kRioSignature                  =  5; // 200
   public static final int kRioRsrcType                   =  6; // 200
   public static final int kRioDeviceMgr                  =  7; // 200: Obsolete in 230
   public static final int kRioDefaultTimeout             =  8; // 200
   public static final int kRioLocalFifoRatio             =  9; // 200: Obsolete in 230

   // HACK HACK: PCI-SPECIFIC.
   public static final int kRioBusNum                     = 10; // 200
   public static final int kRioDeviceNum                  = 11; // 200
   public static final int kRioFuncNum                    = 12; // 200

   public static final int kRioRestrictedAccess           = 13; // 220: Obsolete in 230
   public static final int kRioCurrentVersion             = 14; // 220
   public static final int kRioOldestCompatibleVersion    = 15; // 220
   public static final int kRioClientIsBigEndian          = 16; // 220: kFalse means little-endian
   public static final int kRioFpgaInterruptControlOffset = 17; // 220

   public static final int kRioNumMemWindows              = 18; // 230

   // ------------- HACK HACK: Keep in sequential order Until IO Mgr ---
   // HACK HACK until we have the IO Manager.  DO NOT INCLUDE INTO THE
   // RING CONTROL!!!
   public static final int kRioMemBaseBar0                = 19; // 230
   public static final int kRioMemBaseBar1                = 20; // 230
   public static final int kRioMemSizeBar0                = 21; // 230
   public static final int kRioMemSizeBar1                = 22; // 230
   // --------------- End HACK HACK ------------------------------------

   public static final int kRioSessionState               = 23; // 230
   public static final int kRioPersonalityLockTimeout     = 24; // 230
   public static final int kRioAddressSpace               = 25; // 230

   public static final int kRioChassisNum                 = 27; // 230
   public static final int kRioSlotNum                    = 28; // 230
   public static final int kRioLocalFifoDefaultDepth      = 29; // 230

   public static final int kRioTriggerBusNum              = 30; // 230
   public static final int kRioTriggerReserveLine         = 31; // 230
   public static final int kRioTriggerUnreserveLine       = 32; // 230
   public static final int kRioTriggerReservedLines       = 33; // 230

   public static final int kRioIrqNodeReserve             = 34; // 230
   public static final int kRioFpgaInterruptEnable        = 35; // 230

   public static final int kRioIsItOkToDownload           = 36; // 230

   public static final int kRioFpgaResetOffset            = 37; // 230
   public static final int kRioFpgaResetWidthInBits       = 38; // 230
   public static final int kRioFpgaControlOffset          = 39; // 230

   public static final int kRioResetIfLastSession         = 40; // 230

   public static final int kRioHasDeviceAccess            = 41; // 230

   public static final int kRioBusInterfaceType           = 42; // 240

// String attributes
   public static final int kRioProductName            = 0;
   public static final int kRioWhatFpgaIsDoing        = 1;   // 230
   public static final int kRioResourceName           = 2;   // 230

// Host attributes
   public static final int kRioHostCurrentVersion          = 0;
   public static final int kRioHostOldestCompatibleVersion = 1;
   public static final int kRioHostRpcServerPort           = 2;
   public static final int kRioHostRpcTimeout              = 3; // seconds
   public static final int kRioHostDeviceDiscoveryTimeout  = 4;
   public static final int kRioHostHasDeviceAccess         = 5;
   public static final int kRioHostRpcSessionTimeout       = 6; // seconds

   public static final int kRioHostAliases       = 0;
   public static final int kRioHostAlias         = 1;
   public static final int kRioHostDeviceAccess  = 2;
   public static final int kRioHostRecentDevices = 3;

   public static final int  kRioDynamicRpcServerPort = 0;

   //
   // IO Window types
   //

   public static final int  kRioAddressSpaceMite = 1;
   public static final int  kRioAddressSpaceFpga = 2;

   //
   // Device block[Read|Write] attributes
   //

   public static final int  kRioIoAttributeAccessByteWidthMask      = 0x0F;
   public static final int  kRioIoAttributeFpgaIncrement            = 0x10;
   public static final int  kRioIoAttributeDustMiteNtFlashBitstream = 0x20;

   //
   // Event types
   //

   public static final int kRioEventInvalidEvent      = -1;
   public static final int kRioEventFirstEvent        = 0;
   public static final int kRioEventPCIInterrupt      = 0;
   public static final int kRioEventRemoval           = 1;
   public static final int kRioEventMaxNumberOfEvents = 2; // Add all other events before this one

   //
   // Session states
   //

   public static final int kRioSSNothing     = 1;
   public static final int kRioSSExclusive   = 2;
   public static final int kRioSSInvariant   = 4;
   public static final int kRioSSOverride    = 6;


   /// Device signature information.

   // The lower 32 bits are divided by:
   // bits 28-31  busType , see nRioBusType
   // bits 16-27  TBD
   // bits 8-15   bus (0 - 255 ),
   // bits 3-7    device number (0-31),
   // bits 0-2    function number (0-7)
   //
   /// RIO Bus types.
   public static final int kRioBusTypePci = 0x00000001;

   public static final int kRioBusTypeShift     = 28;
   public static final int kRioBusTypeMask      = 0xF;   // bus type range: 0 - 15 (4 bits)

   /// Signature bus number shift
   public static final int  kRioBusNumShift      = 8;
   public static final int kRioBusNumMask       = 0xFF;  // range: 0 - 255 (8 bits)

   /// Signature device number shift
   public static final int kRioSocketNumShift   = 3;
   public static final int kRioSocketNumMask    = 0x1F;  // range: 0 - 31 (5 bits)

   /// Signature function number shift
   public static final int kRioFunctionNumShift = 0;
   public static final int kRioFunctionNumMask  = 0x7;   // range: 0 - 7 (3 bits)

   /// RIO Device categories.
   public static final int kRioDeviceCRio    = 0x00000001;   //!< compactRIO device
   public static final int kRioDevicePxi     = 0x00000002;   //!< PXI device
   public static final int kRioDeviceFW      = 0x00000004;   //!< FireWire (1394) device
   public static final int kRioDeviceBB      = 0x00000008;   //!< BlueBonnet device
   public static final int kRioDeviceSync    = 0x73796E63;   //!< 'sync' (NI-PAL ID)
   public static final int kRioDeviceDaq     = 0x20646171;   //!< 'daq ' (NI-PAL ID for new sync)
   public static final int kRioDeviceIMAQ    = 0x696D6171;   //!< 'imaq' (NI-PAL ID)

   public static final int kRioDustMite      = 0x00000001;
   public static final int kRioDustMiteNT    = 0x00000002;
   public static final int kRioSTC2          = 0x00000003;

   //  declarations for globally-scoped globals...
   //  prototypes...
   //  inline methods and function macros...
}

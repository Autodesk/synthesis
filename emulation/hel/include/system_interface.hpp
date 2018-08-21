#ifndef _SYSTEM_INTERFACE_HPP_
#define _SYSTEM_INTERFACE_HPP_

#include "FRC_FPGA_ChipObject/tSystem.h"
#include "FRC_FPGA_ChipObject/tSystemInterface.h"

namespace hel{
    /**
     * \cond HIDDEN_SYMBOLS
     */
    struct SystemInterface: public nFPGA::tSystemInterface{
        const uint16_t getExpectedFPGAVersion();
        const uint32_t getExpectedFPGARevision();

        const uint32_t* const getExpectedFPGASignature();

        void getHardwareFpgaSignature(uint32_t*, tRioStatusCode*);

        uint32_t getLVHandle(tRioStatusCode*);

        uint32_t getHandle();

        void reset(tRioStatusCode*);

        void getDmaDescriptor(int, tDMAChannelDescriptor*);
    };
    /**
     * \endcond
     */
}

#endif

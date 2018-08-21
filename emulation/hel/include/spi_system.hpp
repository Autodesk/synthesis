#ifndef _SPI_SYSTEM_HPP_
#define _SPI_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSPI.h"

namespace hel{

    /**
     * \brief Data model for SPI system
     *
     * This is currently unsupported by HEL
     */

    struct SPISystem{
        /**
         * \cond HIDDEN_SYMBOLS
         */
    private:
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoTriggerConfig auto_trigger_config;
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoByteCount auto_byte_count;
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tChipSelectActiveHigh chip_select_active_high;
        uint8_t auto_chip_select;
        bool auto_spi_1_select;
        uint32_t auto_rate;
        uint8_t enabled_dio;

    public:
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoTriggerConfig getAutoTriggerConfig()const;
        void setAutoTriggerConfig(nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoTriggerConfig);
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoByteCount getAutoByteCount()const;
        void setAutoByteCount(nFPGA::nRoboRIO_FPGANamespace::tSPI::tAutoByteCount);
        nFPGA::nRoboRIO_FPGANamespace::tSPI::tChipSelectActiveHigh getChipSelectActiveHigh()const;
        void setChipSelectActiveHigh(nFPGA::nRoboRIO_FPGANamespace::tSPI::tChipSelectActiveHigh);
        uint8_t getAutoChipSelect()const;
        void setAutoChipSelect(uint8_t);
        bool getAutoSPI1Select()const;
        void setAutoSPI1Select(bool);
        uint32_t getAutoRate()const;
        void setAutoRate(uint32_t);
        uint8_t getEnabledDIO()const;
        void setEnabledDIO(uint8_t);
        SPISystem()noexcept;
        SPISystem(const SPISystem&)noexcept;
        /**
         * \endcond
         */
    };
}

#endif

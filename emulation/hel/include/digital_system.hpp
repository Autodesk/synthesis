#ifndef _DIGITAL_SYSTEM_HPP_
#define _DIGITAL_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDIO.h"

namespace hel{
    struct DigitalSystem{
		static constexpr int32_t NUM_DIGITAL_HEADERS = 10; //hal::kNumDigitalHeaders
		static constexpr int32_t NUM_DIGITAL_MXP_CHANNELS = 16; //hal::kNumDigitalMXPChannels
		static constexpr int32_t NUM_DIGITAL_PWM_OUTPUTS = 6; //hal::kNumDigitalPWMOutputs
    private:

        /**
         * \var
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO outputs;

        /**
         * \var
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable enabled_outputs;

        /**
         * \var
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse pulses;

        /**
         * \var
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI inputs;

        /**
         * \var uint16_t mxp_special_functions_enabled
         * \brief Bit mask for MXP pins representing if their non-DIO option should be active
         * Note: the bitmask is default high for DIO, low for MXP special function
         */

        uint16_t mxp_special_functions_enabled;//enabled low, double check that

        /**
         * \var
         */

        uint8_t pulse_length;

        std::array<uint8_t, NUM_DIGITAL_PWM_OUTPUTS> pwm; //TODO unclear whether these are mxp pins or elsewhere (there are only six here whereas there are ten on the mxp)

    public:

        /**
         * \fn
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO getOutputs()const;

        /**
         * \fn
         */

        void setOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDO);

        /**
         * \fn
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable getEnabledOutputs()const;

        /**
         * \fn
         */

        void setEnabledOutputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tOutputEnable);

        /**
         * \fn
         */

        uint16_t getMXPSpecialFunctionsEnabled()const;

        /**
         * \fn
         */

        void setMXPSpecialFunctionsEnabled(uint16_t);

        /**
         * \fn
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse getPulses()const;

        /**
         * \fn
         */

        void setPulses(nFPGA::nRoboRIO_FPGANamespace::tDIO::tPulse);

        /**
         * \fn
         */

        nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI getInputs()const;

        /**
         * \fn
         */

        void setInputs(nFPGA::nRoboRIO_FPGANamespace::tDIO::tDI);

        /**
         * \fn
         */

        uint8_t getPulseLength()const;

        /**
         * \fn
         */

        void setPulseLength(uint8_t);

        /**
         * \fn
         */

        uint8_t getPWMDutyCycle(uint8_t)const;

        /**
         * \fn
         */

        void setPWMDutyCycle(uint8_t, uint8_t);

        DigitalSystem();
    };
}

#endif

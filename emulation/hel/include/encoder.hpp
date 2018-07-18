#ifndef _ENCODER_HPP_
#define _ENCODER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tEncoder.h"

namespace hel{

    /**
     * \struct Encoder
     * \brief Data model for encoder input data
     * Holds all the data for encoder inputs
     */

    struct Encoder{
        static constexpr const int32_t NUM_ENCODERS = 8; //hal::kNumEncoders
    private:

        /**
         * \var tEnoder::tOutput output
         * \brief
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput output;

        /**
         * \var tEnoder::tConfig config
         * \brief Configuration for count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig config;

        /**
         * \var tEnoder::tTimerOutput timer_output
         * \brief Time-based count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput timer_output;

        /**
         * \var tEnoder::tTimerConfig timer_config
         * \brief Configuration for time-based count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig timer_config;

    public:
        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput getOutput()const;
        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput);
        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig getConfig()const;
        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig);
        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput getTimerOutput()const;
        void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput);
        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig getTimerConfig()const;
        void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig);
        Encoder();
    };
}

#endif

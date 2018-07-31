#ifndef _FPGA_ENCODER_HPP_
#define _FPGA_ENCODER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tEncoder.h"

namespace hel{

    /**
     * \struct FPGAEncoder
     * \brief Data model for incremental encoder input data
     * Holds all the data for encoder inputs
     */

    struct FPGAEncoder{
        static constexpr const int32_t NUM_ENCODERS = 8; //hal::kNumFPGAEncoders
    private:

        /**
         * \var tEnoder::tOutput output
         * \brief The encoder's raw count
         * Direction is the direction of motion
         * Value is the encoder tick count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput output;

        /**
         * \var tEnoder::tConfig config
         * \brief Configuration for count
         * IndexSource specifies which input will reset the encoder count
         * Reverse reversed the encoder's direction
         * ASource and BSource indicate which port the encoder should pull from
         *     Module indicates whether it's on the MXP
         *     Channel is the remapped hardware channel itself (For analog triggers, the first two bits indicate the analog trigger type, the others indicate the actual channel)
         *     AnalogTrigger indicates that it's an analog input rather than DIO (will be triggered )
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig config;

        /**
         * \var tEnoder::tTimerOutput timer_output
         * \brief The period of the most recent pulse
         * Stalled represents the encoder stalled state (i.e. it it's stopped turning)
         * Period is the time taken to count \bCount encoder ticks in units of system ticks (40 microseconds per system tick). It increments by 2's but uses the first bit, so it must be left bit-shifted one before interpretation.
         * Count is the encoder count during the polling period
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput timer_output;

        /**
         * \var tEnoder::tTimerConfig timer_config
         * \brief Configuration for encoder period measurement
         * StallPeriod is the time that must pass until the encoder is continued stalled (in system ticks)
         * AverageSize sets the number of samples the timer uses when calculating the period (expects 1 to 127)
         * UpdateWhenEmpty is like the UpdateWhenEmpty of tCounter's TimerConfig, but it is unused
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
        FPGAEncoder();
    };
}

#endif

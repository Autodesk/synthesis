#ifndef _FPGA_ENCODER_HPP_
#define _FPGA_ENCODER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tEncoder.h"

namespace hel{

    /**
     * \brief Data model for incremental encoder input data
     * Holds all the data for encoder inputs
     */

    struct FPGAEncoder{

        /**
         * \brief The number of FPGA encoders HAL supports
         */

        static constexpr const int32_t NUM_ENCODERS = 8; //hal::kNumFPGAEncoders

    private:
        /**
         * \brief The counter's count at the point it was reset
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput zeroed_output;

        /**
         * \brief The encoder's raw count
         * Direction is the direction of motion
         * Value is the encoder tick count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput output;

        /**
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
         * \brief The period of the most recent pulse
         * Stalled represents the encoder stalled state (i.e. it it's stopped turning)
         * Period is the time taken to reach Count encoder ticks in units of system ticks (40 microseconds per system tick). It increments by 2's but uses the first bit, so it must be left bit-shifted one before interpretation.
         * Count is the encoder count during the polling period
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput timer_output;

        /**
         * \brief Configuration for encoder period measurement
         * StallPeriod is the time that must pass until the encoder is continued stalled (in system ticks)
         * AverageSize sets the number of samples the timer uses when calculating the period (expects 1 to 127)
         * UpdateWhenEmpty is like the UpdateWhenEmpty of tCounter's TimerConfig, but it is unused
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig timer_config;

    public:
        /**
         * \brief Reset the counter
         *
         * Resets the zero point of the count
         */

        void reset()noexcept;
        /**
         * \brief Get the current count of the encoder including resets
         * \return The current encoder count
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput getCurrentOutput()const noexcept;

        /**
         * \brief Get the raw count of the encoder ignoring resets
         * \return The raw count of the encoder
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput getRawOutput()const noexcept;

        /**
         * \fn void setRawOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput out)noexcept
         * \brief Set the raw count of the encoder
         * \param out The count to set the encoder to
         */

        void setRawOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tOutput)noexcept;

        /**
         * \brief Get the configuration of the encoder
         * \return The configuration of the encoder
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig getConfig()const noexcept;

        /**
         * \fn void setConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig c)noexcept
         * \brief Set the encoders' configuration
         * \param c The configuration to use
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tConfig)noexcept;

        /**
         * \brief Get the sample timing of the encoder
         * \return The encoder sample timing
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput getTimerOutput()const noexcept;

        /**
         * \fn void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput timer_out)noexcept
         * \brief Set the encoder's sample timing
         * \param timer_out The sample timing to use
         */

        void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerOutput)noexcept;

        /**
         * \brief Get the encoder's sample timing configuration
         * \return The encoder's sample timing configuration
         */

        nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig getTimerConfig()const noexcept;

        /**
         * \fn void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig timer_c)noexcept
         * \brief Set the encoder's sample timing configuration
         * \param timer_c The sample timing configuration to use
         */

        void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tEncoder::tTimerConfig)noexcept;

        /**
         * Constructor for FPGAEncoder
         */

        FPGAEncoder()noexcept;

        /**
         * Constructor for FPGAEncoder
         * \param source An FPGAEncoder object to copy
         */

        FPGAEncoder(const FPGAEncoder&)noexcept;
    };
}

#endif

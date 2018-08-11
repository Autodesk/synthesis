#ifndef _COUNTER_HPP_
#define _COUNTER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tCounter.h"

namespace hel{

    /**
     * \brief Models a RoboRIO counter
     */

    struct Counter{
        static constexpr uint8_t MAX_COUNTER_COUNT = nFPGA::nRoboRIO_FPGANamespace::tCounter::kNumSystems;
    private:

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput zeroed_output
         * \brief The counter's count at the point it was reset
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput zeroed_output;
        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput output
         * \brief The counter's count
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput output;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig config
         * \brief Configuration for the counter
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig config;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput timer_output
         * \brief The time count (period)
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput timer_output;

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig timer_config
         * \brief Configuration for the time counter
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig timer_config;

    public:

        /**
         * \fn void reset()noexcept
         * \brief Reset the counter
         *
         * Resets the zero point of the count
         */

        void reset()noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getCurrentOutput()const noexcept
         * \brief Get the current count of the counter including resets
         * \return The current encoder count
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getCurrentOutput()const noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getRawOutput()const noexcept
         * \brief Get the counter's raw count
         * \return The raw count of the counter
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getRawOutput()const noexcept;

        /**
         * \fn void setRawOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput output)noexcept
         * \brief Set the raw count of the counter
         * \param output The count to set the counter to
         */

        void setRawOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig getConfig()const noexcept
         * \brief Get the configuration for the counter
         * \return The counter's configuration
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig getConfig()const noexcept;

        /**
         * \fn void setConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig config)noexcept
         * \brief Set the configuration for the counter
         * \param config The configuration to set for the counter
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput getTimerOutput()const noexcept
         * \brief Get the time count of the counter
         * \return The time count
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput getTimerOutput()const noexcept;

        /**
         * \fn void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput timer_output)noexcept
         * \brief Set the time count of the counter
         * \param timer_output The time count to set for the counter
         */

        void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput)noexcept;

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig getTimerConfig()const noexcept
         * \brief Get the configuration for the timed count
         * \return The timer configuration of the counter
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig getTimerConfig()const noexcept;

        /**
         * \fn void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig timer_config)noexcept
         * \brief Set the configuration for the timed count
         * \param timer_config The timer configuration to set for the counter
         */

        void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig)noexcept;

        /**
         * Constructor for Counter
         */

        Counter()noexcept;

        /**
         * Constructor for Counter
         * \param source A Counter object to copy
         */

        Counter(const Counter&)noexcept;
    };
}

#endif

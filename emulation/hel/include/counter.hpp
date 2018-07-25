#ifndef _COUNTER_HPP_
#define _COUNTER_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tCounter.h"

namespace hel{

    struct Counter{
        static constexpr uint8_t MAX_COUNTER_COUNT = nFPGA::nRoboRIO_FPGANamespace::tCounter::kNumSystems;
    private:

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
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getOutput()const
         * \brief Get the counter's count
         * \return the count
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getOutput()const;

        /**
         * \fn void setOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput output)
         * \brief Set the counter's count
         * \param output the count to set the counter to
         */

        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput);

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig getConfig()const
         * \brief Get the configuration for the counter
         * \return the counter's configuration
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig getConfig()const;

        /**
         * \fn void setConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig config)
         * \brief Set the configuration for the counter
         * \param config the configuration to set for the counter
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig);

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput getTimerOutput()const
         * \brief Get the time count of the counter
         * \return the time count
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput getTimerOutput()const;

        /**
         * \fn void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput timer_output)
         * \brief Set the time count of the counter
         * \param timer)output the time count to set for the counter
         */

        void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput);

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig getTimerConfig()const
         * \brief Get the configuration for the timed count
         * \return the timer configuration of the counter
         */

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig getTimerConfig()const;

        /**
         * \fn void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig timer_config)
         * \brief Set the configuration for the timed count
         * \param timer_config the timer configuration to set for the counter
         */

        void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig);

        Counter();
    };
}

#endif

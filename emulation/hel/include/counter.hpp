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
        nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput getOutput()const;
        void setOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tOutput);

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig getConfig()const;
        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tConfig);

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput getTimerOutput()const;
        void setTimerOutput(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerOutput);

        nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig getTimerConfig()const;
        void setTimerConfig(nFPGA::nRoboRIO_FPGANamespace::tCounter::tTimerConfig);

        Counter();
    };
}

#endif

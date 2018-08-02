#ifndef _GLOBAL_HPP_
#define _GLOBAL_HPP_

#include <cstdint>

namespace hel{

    /**
     * \struct Global
     * \brief Manages data associated with Ni FPGA's tGlobal class
     */

    struct Global{
    private:
        /**
         * \var uint64_t fpga_start_time
         * \brief The time in microseconds at which the emulated FPGA started
         */

        uint64_t fpga_start_time;

    public:

        /**
         * \fn uint64_t getFPGAStartTime()const
         * \brief Get the time in microseconds at which the emulated FPGA started
         */

        uint64_t getFPGAStartTime()const noexcept;

        /**
         * \fn static uint64_t getCurrentTime()
         * \brief Get the time in microseconds for which the emulated FPGA has been running
         */

        static uint64_t getCurrentTime()noexcept;
        Global()noexcept;
    };
}

#endif

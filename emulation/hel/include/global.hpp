#ifndef _GLOBAL_HPP_
#define _GLOBAL_HPP_

#include <cstdint>

namespace hel{

	/**
	 * \brief The localhost port to use for gRPC communication
	 */

    extern int gRPCPort;

    /**
     * \brief Manages data associated with Ni FPGA's tGlobal class
     */

    struct Global{
    private:
        /**
         * \brief The time in microseconds at which the emulated FPGA started
         */

        uint64_t fpga_start_time;

    public:

        /**
         * \brief Get the time in microseconds at which the emulated FPGA started
         */

        uint64_t getFPGAStartTime()const noexcept;

        /**
         * \brief Get the time in microseconds for which the emulated FPGA has been running
         */

        static uint64_t getCurrentTime()noexcept;

        /**
         * Constructor for Global
         */

        Global()noexcept;

        /**
         * Constructor for Global
         * \param source A Global object to copy
         */

        Global(const Global&)noexcept;
    };
}

#endif

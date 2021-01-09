#ifndef _NET_COMM_HPP_
#define _NET_COMM_HPP_

#include <functional>

namespace hel{

    /**
     * \brief Container for Driver Station networking data
     */

    struct NetComm{
        /**
         * \brief The handle for the occur function
         */

        uint32_t ref_num;

        /**
         * \brief The function called to signal HAL is has received new data from the Driver Station
         */

        std::function<void(uint32_t)> occurFunction;

        /**
         * Constructor for NetComm
         */

        NetComm()noexcept;

        /**
         * Constructor for NetComm
         * \param source A NetComm object to copy
         */

        NetComm(const NetComm&)noexcept;
    };
}

#endif

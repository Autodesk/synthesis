#ifndef _PDP_HPP_
#define _PDP_HPP_

#include "can_device.hpp"

namespace hel{

    /**
     * \brief Data model for the Power Distribution Panel (PDP)
     * Currently unsupported by Synthesis
     */

    struct PDP{ //TODO handle emulated data

        /**
         * \brief Interpretation definitions for CAN message data bytes
         */

        enum MessageData{
            SIZE = 8
        };

        /**
         * Constructor for PDP
         */

        PDP()noexcept;
        /**
         * Constructor for PDP
         * \param source A PDP object to copy
         */

        PDP(const PDP&)noexcept;
    };
}

#endif

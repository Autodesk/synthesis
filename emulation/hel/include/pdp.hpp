#ifndef _PDP_HPP_
#define _PDP_HPP_

#include "can_device.hpp"

namespace hel{

    struct PDP{ //TODO add implement data

        enum MessageData{
            SIZE = 8
        };

        enum SendCommandByteMask: uint8_t{};

        enum ReceiveCommandIDMask: uint32_t{};

		PDP()noexcept;
		PDP(const PDP&)noexcept;
    };
}

#endif

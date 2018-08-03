#ifndef _SOLENOID_HPP_
#define _SOLENOID_HPP_

#include "can_device.hpp"

namespace hel{

    struct Solenoid{ //TODO may need to store more data

		static constexpr uint8_t NUM_SOLENOIDS = 8;

        enum MessageData{ //TODO move to CANDevice?
            SOLENOIDS = 2,
            SIZE = 8
        };

        enum SendCommandByteMask: uint8_t{
        };

        enum ReceiveCommandIDMask: uint32_t{
        };
    };
}

#endif

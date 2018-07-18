#ifndef _ENGINE_INTERFACE_HPP_
#define _ENGINE_INTERFACE_HPP_

#include <string>

#include "roborio.hpp"

namespace hel{
    struct EngineInterface{
        std::array<double, tPWM::kNumHdrRegisters> pwm_hdrs;

        void update();

        std::string serialize();
        static EngineInterface deserialize(std::string);

        std::string toString()const;
    };
}

#endif

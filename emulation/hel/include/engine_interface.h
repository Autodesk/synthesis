#ifndef _ENGINE_INTERFACE_H_
#define _ENGINE_INTERFACE_H_

#include <string>

#include "roborio.h"

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

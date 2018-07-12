#ifndef _ROBORIO_INTERFACE_H_
#define _ROBORIO_INTERFACE_H_

#include "roborio.h"

namespace hel{
    struct RoboRIOInterface{
        std::array<double, tPWM::kNumHdrRegisters> pwm_hdrs;

        void update();

        std::string toString()const;
    };
}

#endif

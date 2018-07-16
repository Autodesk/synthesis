#ifndef _RECIEVE_DATA_H_
#define _RECIEVE_DATA_H_

#include "roborio.h"
#include "mxp_data.h"

namespace hel{
    struct ReceiveData{
    private:
        std::array<std::vector<int32_t>, hal::kNumAnalogInputs> analog_inputs; //TODO manage analog history vector
        std::array<bool, hal::kNumDigitalHeaders> digital_hdrs;
        std::array<MXPData, hal::kNumDigitalMXPChannels> digital_mxp;
        std::array<RoboRIO::Joystick, RoboRIO::Joystick::MAX_JOYSTICK_COUNT>  joysticks;
    public:
        void update()const;

        std::string toString()const;

        void deserialize(std::string);
    };
}

#endif

#ifndef _RECIEVE_DATA_HPP_
#define _RECIEVE_DATA_HPP_

#include "roborio.hpp"
#include "mxp_data.hpp"
#include "bounds_checked_array.hpp"

namespace hel{
    struct ReceiveData{
    private:
        //std::array<std::vector<int32_t>, hal::kNumAnalogInputs> analog_inputs; //TODO manage analog history vector
        BoundsCheckedArray<bool, hal::kNumDigitalHeaders> digital_hdrs;
        BoundsCheckedArray<MXPData, hal::kNumDigitalMXPChannels> digital_mxp;
        BoundsCheckedArray<Joystick, Joystick::MAX_JOYSTICK_COUNT>  joysticks;
    public:
        void update()const;

        std::string toString()const;

        void deserialize(std::string);
    };
}

#endif

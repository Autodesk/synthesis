#ifndef _RECIEVE_DATA_HPP_
#define _RECIEVE_DATA_HPP_

#include "bounds_checked_array.hpp"
#include "digital_system.hpp"
#include "joystick.hpp"
#include "mxp_data.hpp"

namespace hel{
    struct ReceiveData{
    private:
        //std::array<std::vector<int32_t>, hal::kNumAnalogInputs> analog_inputs; //TODO manage analog history vector
        BoundsCheckedArray<bool, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;
        BoundsCheckedArray<MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS> digital_mxp;
        BoundsCheckedArray<Joystick, Joystick::MAX_JOYSTICK_COUNT>  joysticks;
    public:
        void update()const;

        std::string toString()const;

        void deserialize(std::string);
    };
}

#endif

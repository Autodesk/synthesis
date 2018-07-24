#ifndef _RECIEVE_DATA_HPP_
#define _RECIEVE_DATA_HPP_

#include <memory>
#include <mutex>

#include "bounds_checked_array.hpp"
#include "digital_system.hpp"
#include "joystick.hpp"
#include "mxp_data.hpp"

namespace hel{
    struct ReceiveData{
    private:
        std::string last_received_data;

        //std::array<std::vector<int32_t>, hal::kNumAnalogInputs> analog_inputs; //TODO manage analog history vector
        BoundsCheckedArray<bool, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;
        BoundsCheckedArray<MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS> digital_mxp;
        BoundsCheckedArray<Joystick, Joystick::MAX_JOYSTICK_COUNT>  joysticks;
    public:
        void update()const;

        std::string toString()const;

        void deserialize(std::string);
    };

    class ReceiveDataManager{
    public:
        static std::pair<std::shared_ptr<ReceiveData>, std::unique_lock<std::recursive_mutex>> getInstance() {
            std::unique_lock<std::recursive_mutex> lock(m);
            if(instance == nullptr){
                instance = std::make_shared<ReceiveData>();
            }
            return std::make_pair(instance, std::move(lock));
        }

    private:
        static std::shared_ptr<ReceiveData> instance;
        static std::recursive_mutex m;
    };
}

#endif

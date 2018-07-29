#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

#include "can_bus.hpp"
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \struct CANDevice
     * \brief Models a CAN device on the CAN bus
     * Holds data for generic CAN devices and CAN motor controllers
     */

    struct CANDevice{

        enum class Type{VICTOR_SPX,TALON_SRX,UNKNOWN};

        /**
         * \var static constexpr uint8_t MAX_CAN_BUS_ADDRESS
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62, but using 0 is not recommended
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

    private:

        static constexpr uint32_t TALON_SRX_ZERO_ADDRESS = 33816704;

        static constexpr uint32_t VICTOR_SPX_ZERO_ADDRESS = 17039488;

        Type type;

        uint8_t id;

        double speed;

    public:

        std::string toString()const;

        Type getType()const;

        void setSpeed(BoundsCheckedArray<uint8_t,CANBus::Message::MAX_DATA_SIZE>);

        double getSpeed()const;

        CANDevice();

        CANDevice(uint32_t);
    };

    bool checkCANID(uint32_t,uint32_t,uint32_t);

    std::string to_string(CANDevice::Type);
}

#endif

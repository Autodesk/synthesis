#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

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

        static constexpr uint8_t MAX_MESSAGE_DATA_SIZE = 8;

        /**
         * \var static constexpr int32_t CAN_SEND_PERIOD_NO_REPEAT
         * \brief a send period communicating the message should not be repeated
         */

        static constexpr const int32_t CAN_SEND_PERIOD_NO_REPEAT = 0;

        /**
         * \var static constexpr int32_t CAN_SEND_PERIOD_STOP_REPEATING
         * \brief a send period communicating the message with the associated ID should stop repeating
         */

        static constexpr int32_t CAN_SEND_PERIOD_STOP_REPEATING = -1;

        /**
         * \var static constexpr uint32_t CAN_IS_FRAME_REMOTE
         * \brief used to identify a message ID as that of a remote frame
         * Remote CAN frames are requests for data from a different source.
         */

        static constexpr uint32_t CAN_IS_FRAME_REMOTE = 0x80000000;

        /**
         * \var static constexpr uint32_t CAN_IS_FRAME_11BIT
         * \brief used to identify a message ID as using 11-bit, base formatting
         */

        static constexpr uint32_t CAN_IS_FRAME_11BIT = 0x40000000;

        /**
         * \var static constexpr uint32_t CAN_29BIT_MESSAGE_ID_MASK
         * \brief used as a message ID mask to communicate the message ID is in 29-bit, extended formatting
         */

        static constexpr uint32_t CAN_29BIT_MESSAGE_ID_MASK = 0x1FFFFFFF;

        /**
         * \var static constexpr uint32_t CAN_11BIT_MESSAGE_ID_MASK
         * \brief used as a message ID mask to communicate the message ID is in 11-bit, base formatting
         */

        static constexpr uint32_t CAN_11BIT_MESSAGE_ID_MASK = 0x000007FF;

    private:

        static constexpr uint32_t TALON_SRX_ZERO_ADDRESS = 33816704; //base talon srx arbitration id 0x02040000

        static constexpr uint32_t VICTOR_SPX_ZERO_ADDRESS = 17039488; //base victor spx arbitration id 0x01040000

        Type type;

        uint8_t id;

        double speed;

    public:

        std::string toString()const;

        Type getType()const;

        void setSpeed(BoundsCheckedArray<uint8_t,MAX_MESSAGE_DATA_SIZE>);

        double getSpeed()const;

        /**
         * \fn std::string serialize()const
         * \brief Convert the CANDevice to a JSON object
         * \return a string representing the data in JSON format
         */

        std::string serialize()const;

        /**
         * \fn static CANDevice deserialize(std::string input)
         * \brief Convert a JSON object string to a CANDevice object
         * \param input the data to parse
         * \return the generated CANDevice object
         */

        static CANDevice deserialize(std::string);

        CANDevice();

        CANDevice(uint32_t);
    };

    bool checkCANID(uint32_t,uint32_t,uint32_t);

    std::string to_string(CANDevice::Type);

    CANDevice::Type s_to_can_device_type(std::string);
}

#endif

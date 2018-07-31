#ifndef _CAN_MOTOR_CONTROLLER_HPP_
#define _CAN_MOTOR_CONTROLLER_HPP_

#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \struct CANMotorController
     * \brief Models a CAN device on the CAN bus
     * Holds data for generic CAN devices and CAN motor controllers
     */

    struct CANMotorController{

        enum class Type{VICTOR_SPX,TALON_SRX,UNKNOWN};

        /**
         * \var static constexpr uint8_t MAX_CAN_BUS_ADDRESS
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62, but using 0 is not recommended
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

        static constexpr uint32_t UNKNOWN_DEVICE_ID = 262271;

        enum MessageData{
            COMMAND_BYTE = 7,
            SIZE = 8
        };

        enum SendCommandByteMask: uint8_t{
            SET_POWER_PERCENT = 5,
            SET_INVERTED = 6
        };

        enum ReceiveCommandIDMask: uint32_t{
            GET_POWER_PERCENT = 0b1010000000000
        };

    private:
        enum IDMask: uint32_t{
            DEVICE_ID = 0b01111111,
            DEVICE_TYPE = 0b11000001000000000000000000,
            TALON_SRX_TYPE = 0x02040000,
            VICTOR_SPX_TYPE = 0x01040000
        };

        Type type;

        uint8_t id;

        double speed;

        bool inverted;

    public:
        static uint8_t pullDeviceID(uint32_t);
        static Type pullDeviceType(uint32_t);

        std::string toString()const;

        Type getType()const;

        uint8_t getID()const;

        void setSpeedData(BoundsCheckedArray<uint8_t,MessageData::SIZE>);

        void setSpeed(double);

        double getSpeed()const;

        BoundsCheckedArray<uint8_t,MessageData::SIZE> getSpeedData()const;

        void setInverted(bool);

        /**
         * \fn std::string serialize()const
         * \brief Convert the CANMotorController to a JSON object
         * \return a string representing the data in JSON format
         */

        std::string serialize()const;

        /**
         * \fn static CANMotorController deserialize(std::string input)
         * \brief Convert a JSON object string to a CANMotorController object
         * \param input the data to parse
         * \return the generated CANMotorController object
         */

        static CANMotorController deserialize(std::string);

        CANMotorController();

        CANMotorController(uint32_t);
    };

    std::string to_string(CANMotorController::Type);

    CANMotorController::Type s_to_can_device_type(std::string);
}

#endif

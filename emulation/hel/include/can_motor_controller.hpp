#ifndef _CAN_MOTOR_CONTROLLER_HPP_
#define _CAN_MOTOR_CONTROLLER_HPP_

#include "bounds_checked_array.hpp"
#include "can_device.hpp"

namespace hel{

    /**
     * \struct CANMotorController
     * \brief Models a CAN motor controller
     * Holds data for generic CAN motor controllers
     */

    struct CANMotorController{

        enum MessageData{ //TODO move to CANDevice?
            COMMAND_BYTE = 7,
            SIZE = 8
        };

        enum SendCommandByteMask: uint8_t{ //TODO move to CANDevice?
            SET_POWER_PERCENT = 5,
            SET_INVERTED = 6
        };

        enum ReceiveCommandIDMask: uint32_t{ //TODO move to CANDevice?
            GET_POWER_PERCENT = 0b1010000000000
        };

    private:
        CANDevice::Type type;

        uint8_t id;

        double speed;

        bool inverted;

    public:
        std::string toString()const;

        CANDevice::Type getType()const noexcept;

        uint8_t getID()const noexcept;

        void setSpeedData(BoundsCheckedArray<uint8_t,MessageData::SIZE>)noexcept;

        void setSpeed(double)noexcept;

        double getSpeed()const noexcept;

        BoundsCheckedArray<uint8_t,MessageData::SIZE> getSpeedData()const noexcept;

        void setInverted(bool)noexcept;

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

        CANMotorController()noexcept;

        CANMotorController(const CANMotorController&)noexcept;

        CANMotorController(uint32_t)noexcept;
    };
}

#endif

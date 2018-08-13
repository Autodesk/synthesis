#ifndef _CAN_MOTOR_CONTROLLER_HPP_
#define _CAN_MOTOR_CONTROLLER_HPP_

#include "bounds_checked_array.hpp"
#include "can_device.hpp"

namespace hel{

    /**
     * \brief Models a CAN motor controller
     * Holds data for generic CAN motor controllers
     */

    struct CANMotorController{

        /**
         * \brief Interpretation definitions for CAN message data bytes
         */

        enum MessageData{
            COMMAND_BYTE = 7,
            SIZE = 8
        };

        /**
         * \brief Interpretation definitions for command byte bitmask
         */

        enum SendCommandByteMask: uint8_t{
            SET_POWER_PERCENT = 5,
            SET_INVERTED = 6
        };

        /**
         * \brief Interpretation definitions for message ID bitmask for CAN frames requesting data
         */

        enum ReceiveCommandIDMask: uint32_t{
            GET_POWER_PERCENT = 0b1010000000000
        };

    private:
        /**
         * \brief The type of CAN device
         *
         * Should be a motor controller type
         */

        CANDevice::Type type;

        /**
         * \brief The CAN device ID
         */

        uint8_t id;

        /**
         * \brief The set percent output of the motor controller
         */

        double percent_output;

        /**
         * \brief Whether to invert the percent_output signal or not
         */

        bool inverted;

    public:
        /**
         * \brief Format the CANMotorController as a string
         * \return A string representing the CANMotorController data
         */

        std::string toString()const;

        /**
         * \brief Get the type of motor controller
         * \return The motor controller's type as a CANDevice::Type
         */

        CANDevice::Type getType()const noexcept;

        /**
         * \brief Get the device ID of the motor controller
         * \return The device ID of the motor controller
         */

        uint8_t getID()const noexcept;

        /**
         * \brief Set the percent output of the motor controller using the byte format CTRE CAN protocol uses
         * \param data The percent output to set to
         */

        void setPercentOutputData(BoundsCheckedArray<uint8_t,MessageData::SIZE>)noexcept;

        /**
         * \brief Set the percent output of the motor controller
         * \param out The percent output to set to
         */

        void setPercentOutput(double)noexcept;

        /**
         * \brief Get the percent output of the motor controller
         * \return The percent output of the motor controller
         */

        double getPercentOutput()const noexcept;

        /**
         * \fn BoundsCheckedArray<uint8_t,MessageData::SIZE> getPercentOutputData()const noexcept
         * \brief Fetch the percent output in the byte format the CTRE CAN protocol uses
         * \return A BoundsCheckedArray representing the percent output in byte format
         */

        BoundsCheckedArray<uint8_t,MessageData::SIZE> getPercentOutputData()const noexcept;

        /**
         * \fn void setInverted(bool inverted)noexcept
         * \brief Set the inverted flag of the motor controller
         * \param i Whether to invert motor controller output
         */

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

        /**
         * Constructor for CANMotorController
         */

        CANMotorController()noexcept;

        /**
         * Constructor for CANMotorController
         * \param source A CANMotorController object to copy
         */

        CANMotorController(const CANMotorController&)noexcept;

        /**
         * Constructor for CANMotorController
         * \param messageID A CAN message ID to parse device ID and type information from
         */

        CANMotorController(uint32_t)noexcept;
        /**
         * Constructor for CANMotorController
         * \param id The device ID to use
         * \param type The device type to use
         */

        CANMotorController(uint8_t,CANDevice::Type)noexcept;
    };
}

#endif

#ifndef _CAN_MOTOR_CONTROLLER_HPP_
#define _CAN_MOTOR_CONTROLLER_HPP_

#include <vector>
#include "can_device.hpp"

namespace hel{

    /**
     * \brief Models a CAN motor controller
     * Holds data for generic CAN motor controllers
     */

    struct CANMotorControllerBase: public CANDevice{
    private:
        /**
         * \brief The set percent output of the motor controller
         */

        double percent_output;

    public:
        /**
         * \brief Format the CANMotorControllerBase as a string
         * \return A string representing the CANMotorControllerBase data
         */

        virtual std::string toString()const;

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
         * Constructor for CANMotorControllerBase
         */

        CANMotorControllerBase()noexcept;

        /**
         * Constructor for CANMotorControllerBase
         * \param source A CANMotorController object to copy
         */

        CANMotorControllerBase(const CANMotorControllerBase&)noexcept;

      /**
         * Constructor for CANMotorControllerBase
         * \param device The device information to use
         */

        CANMotorControllerBase(CANMessageID)noexcept;
    };

    namespace ctre{
        struct CANMotorController: public CANMotorControllerBase{
            static constexpr uint32_t HEARTBEAT_ID = 63;
            static constexpr uint32_t HEARTBEAT_API_ID = 1;

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
                INVERT = 6
            };

            enum CommandAPIID: int32_t{
                GET_PERCENT_OUTPUT = 0x007
            };

            void parseCANPacket(const int32_t&, const std::vector<uint8_t>&);

            std::vector<uint8_t> generateCANPacket(const int32_t&)const;

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
             * \param device The device information to use
             */

            CANMotorController(CANMessageID)noexcept;
        };
    }

    namespace rev{
        struct CANMotorController: public CANMotorControllerBase{
            // Firmware version can packet strucute: Major | Minor | Build 2 | Build 1 | Debug | Hardware Revision - REV swaps Build 2 and build 1 bytes when determining build version
            static constexpr uint32_t MIN_FIRMWARE_VERSION = 0x0101001C;
            static constexpr bool USE_FIRMWARE_DEBUG_BUILD = false;
            static constexpr uint8_t HARDWARE_REVISION = 0x00;

            static constexpr uint32_t HEARTBEAT_ID = 0;

            enum CommandAPIID: int32_t{
                DC_SET = 0x002,
                SMART_VEL_SET = 0x013,
                POS_SET = 0x032,
                VOLT_SET = 0x042,
                CURRENT_SET = 0x043,
                SMARTMOTION_SET = 0x052,
                CLEAR_FAULTS = 0x06E,
                HEARTBEAT = 0x092,
                FIRMWARE  = 0x098,
                PARAM_ACCESS = 0x300 // least significant eight bits of the API ID with this identifier select for the parameter
            };

            void parseCANPacket(const int32_t&, const std::vector<uint8_t>&);

            std::vector<uint8_t> generateCANPacket(const int32_t&)const;

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
             * \param device The device information to use
             */

            CANMotorController(CANMessageID)noexcept;
        };
    }
}
#endif

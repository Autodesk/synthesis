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

        /**
         * \brief Whether to invert the percent_output signal or not
         */

        bool inverted;

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
         * \fn void setInverted(bool inverted)noexcept
         * \brief Set the inverted flag of the motor controller
         * \param i Whether to invert motor controller output
         */

        void setInverted(bool)noexcept;

        /**
         * \fn std::string serialize()const
         * \brief Convert the CANMotorControllerBase to a JSON object
         * \return a string representing the data in JSON format
         */

        virtual std::string serialize()const;

        /**
         * \fn static CANMotorControllerBase deserialize(std::string input)
         * \brief Convert a JSON object string to a CANMotorControllerBase object
         * \param input the data to parse
         * \return the generated CANMotorControllerBase object
         */

      static std::shared_ptr<CANMotorControllerBase> deserialize(std::string);

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

            /**
             * \brief Interpretation definitions for message ID bitmask for CAN frames requesting data
             */

            enum ReceiveCommandIDMask: uint32_t{
                GET_POWER_PERCENT = 0b1010000000000
            };

            void parseCANPacket(const int32_t&, const std::vector<uint8_t>&)noexcept;

            std::vector<uint8_t> generateCANPacket(const int32_t&)noexcept;

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
            void parseCANPacket(const int32_t&, const std::vector<uint8_t>&)noexcept;

            std::vector<uint8_t> generateCANPacket(const int32_t&)noexcept;

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

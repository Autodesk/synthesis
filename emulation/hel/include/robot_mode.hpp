#ifndef _ROBOT_MODE_HPP_
#define _ROBOT_MODE_HPP_

#include "FRC_NetworkCommunication/FRCComm.h"

#include <string>

namespace hel{

    /**
     * \brief Represents match phase and robot enabled state
     */

    struct RobotMode{

        /**
         * \brief Represents robot running mode
         */

        enum class Mode{
            AUTONOMOUS,TELEOPERATED,TEST
        };

    private:

        /**
         * \brief The robot running mode
         */

        Mode mode;

        /**
         * \brief Robot enabled state
         */

        bool enabled;

        /**
         * \brief Robot emergency stopped state
         */

        bool emergency_stopped;

        /**
         * \brief Whether the FMS is attached or not
         */

        bool fms_attached;

        /**
         * \brief Whether the driver station is attached or not
         */

        bool ds_attached;

    public:

        /**
         * \brief Get robot running mode
         * \return A Mode vaue representing the robot running state
         */

        Mode getMode()const noexcept;

        /**
         * \brief Set robot running state
         * \param m The new robot mode to use
         */

        void setMode(Mode)noexcept;

        /**
         * \brief Get robot enabled state
         * \return True if the robot is enabled
         */

        bool getEnabled()const noexcept;

        /**
         * \brief Set the robot enabled state
         * \param e A new value to use for the robot enabled state
         */

        void setEnabled(bool)noexcept;

        /**
         * \brief Get robot emergency stopped state
         * \return True if the robot is emergency stopped
         */

        bool getEmergencyStopped()const noexcept;

        /**
         * \brief Set the robot emergency stopped state
         * \param e A new value to use for the robot emergency stopped state
         */

        void setEmergencyStopped(bool)noexcept;

        /**
         * \brief Get robot FMS connection state
         * \return True if the robot is connected to the FMS
         */

        bool getFMSAttached()const noexcept;

        /**
         * \brief Set robot FMS connection state
         * \param attached A new value to use for the robot FMS connection state
         */

        void setFMSAttached(bool)noexcept;

        /**
         * \brief Get robot driver station connection state
         * \return True if the robot is connected to the driver station
         */

        bool getDSAttached()const noexcept;

        /**
         * \brief Set robot driver station connection state
         * \param attached A new value to use for the robot driver station connection state
         */

        void setDSAttached(bool)noexcept;

        /**
         * \brief Populate a new ControlWord_t object from this RobotMode object
         * \return A new ControlWord_t object populated from this RobotMode object
         */

        ControlWord_t toControlWord()const noexcept;

        /**
         * \brief Deserialize a RobotMode object from a JSON string
         * \param input The JSON string to parse
         * \return The parsed RobotMode object
         */

        static RobotMode deserialize(std::string);

        /**
         * \brief Serialize RobotMode as a JSON string
         * \return The serialized string
         */

        std::string serialize()const;

        /**
         * \brief Format RobotMode data as a string
         * \return A string containing the converted data
         */

        std::string toString()const;

        /**
         * Constructor for RobotMode
         */

        RobotMode()noexcept;

        /**
         * Constructor for RobotMode
         * \param source A RobotMode object to copy
         */

        RobotMode(const RobotMode&)noexcept;
    };
}

#endif

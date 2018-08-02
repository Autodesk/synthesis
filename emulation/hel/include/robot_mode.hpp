#ifndef _ROBOT_MODE_HPP_
#define _ROBOT_MODE_HPP_

#include "FRC_NetworkCommunication/FRCComm.h"

#include <string>

namespace hel{

    /**
     * \struct RobotMode
     * \brief Represents match phase and robot enabled state
     */

    struct RobotMode{

        /**
         * \enum Mode
         * \brief Represents robot running mode
         * Represents whether robot is in autonomous, teleoperated, or test/practice mode.
         */

        enum class Mode{AUTONOMOUS,TELEOPERATED,TEST};

    private:

        /**
         * \var Mode mode
         * \brief The robot running mode
         */

        Mode mode;

        /**
         * \var bool enabled
         * \brief Robot enabled state
         */

        bool enabled;

        /**
         * \var bool emergency_stopped
         * \brief Robot emergency stopped state
         */

        bool emergency_stopped;

        /**
         * \var bool fms_attached
         * \brief Whether the FMS is attached or not
         */

        bool fms_attached;

        /**
         * \var bool ds_attached
         * \brief Whether the driver station is attached or not
         */

        bool ds_attached;

    public:

        /**
         * \fn Mode getMode()const
         * \brief Get robot running mode
         * \return a State object representing the robot running state
         */

        Mode getMode()const noexcept;

        /**
         * \fn void setMode(Mode mode)
         * \brief Set robot running state
         * \param state a State object representing the robot running state
         */

        void setMode(Mode)noexcept;

        /**
         * \fn bool getEnabled()const
         * \brief Get robot enabled state
         * \return true if the robot is enabled
         */

        bool getEnabled()const noexcept;

        /**
         * \fn void setEnabled(bool enabled)
         * \brief Set the robot enabled state
         * \param enabled a bool representing the robot enabled state
         */

        void setEnabled(bool)noexcept;

        /**
         * \fn bool getEmergencyStopped()const
         * \brief Get robot emergency stopped state
         * \return true if the robot is emergency stopped
         */

        bool getEmergencyStopped()const noexcept;

        /**
         * \fn void setEmergencyStopped(bool emergency_stopped)
         * \brief Set the robot emergency stopped state
         * \param emergency_stopped a bool representing the robot emergency stopped state
         */

        void setEmergencyStopped(bool)noexcept;

        /**
         * \fn bool getFMSAttached()const
         * \brief Get robot FMS connection state
         * \return true if the robot is connected to the FMS
         */

        bool getFMSAttached()const noexcept;

        /**
         * \fn void setFMSAttached(bool fms_attached)
         * \brief Set robot FMS connection state
         * \param fms_attached a bool representing the robot FMS connection state
         */

        void setFMSAttached(bool)noexcept;

        /**
         * \fn bool getDSAttached()const
         * \brief Get robot driver station connection state
         * \return true if the robot is connected to the driver station
         */

        bool getDSAttached()const noexcept;

        /**
         * \fn void setDSAttached(bool ds_attached)
         * \brief Set robot driver station connection state
         * \param ds_attached a bool representing the robot driver station connection state
         */

        void setDSAttached(bool)noexcept;

        /**
         * \fn ControlWord_t toControlWord()const
         * \brief Populate a new ControlWord_t object from this RobotMode object
         * \return a new ControlWord_t object populated from this RobotMode object
         */

        ControlWord_t toControlWord()const noexcept;

        static RobotMode deserialize(std::string);

        std::string serialize()const;

        std::string toString()const;

        RobotMode()noexcept;
    };
}

#endif

#ifndef __ALARM_HPP__
#define __ALARM_HPP__

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAlarm.h"

namespace hel{

    /**
     * \brief Data model for a time-based trigger/alarm
     */

    struct Alarm{
    private:

        /**
         * \var bool enabled
         * \brief Whether the alarm is enabled and will trigger
         */

        bool enabled;

        /**
         * \var uint32_t trigger_time
         * \brief Represents the trigger time in microseconds
         */

        uint32_t trigger_time;

    public:

        /**
         * \fn bool getEnabled()const noexcept
         * \brief Get the enabled state of the alarm
         * \return True if the alarm is enabled
         */

        bool getEnabled()const noexcept;

        /**
         * \fn void setEnabled(bool enabled)noexcept
         * \brief Set the enabled state of the alarm
         * \param enabled True to enable the alarm
         */

        void setEnabled(bool)noexcept;

        /**
         * \fn uint32_t getTriggerTime()const noexcept
         * \brief Get the time the alarm will trigger
         * \return The time the alarm will trigger
         */

        uint32_t getTriggerTime()const noexcept;

        /**
         * \fn void setTriggerTime(uint32_t trigger_time)noexcept
         * \brief Set the time the alarm will trigger
         * \param trigger_time The time at which to trigger the alarm
         */

        void setTriggerTime(uint32_t)noexcept;

        /**
         * Constructor for an Alarm
         */

        Alarm()noexcept;

        /**
         * Constructor for an Alarm
         *
         * \param source An Alarm object to copy
         */

        Alarm(const Alarm&)noexcept;
    };
}

#endif

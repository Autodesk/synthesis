#ifndef __ALARM_HPP__
#define __ALARM_HPP__

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAlarm.h"

namespace hel{

    /**
     * \struct Alarm
     * \brief Data model for a time-based trigger/alarm
     */

    struct Alarm{
    private:
        bool enabled;

        /**
         * \var uint32_t trigger_time
         * \brief
         * Represents the time in microseconds
         */

        uint32_t trigger_time;

    public:
        bool getEnabled()const noexcept;
        void setEnabled(bool)noexcept;
        uint32_t getTriggerTime()const noexcept;
        void setTriggerTime(uint32_t)noexcept;

        Alarm()noexcept;
        Alarm(const Alarm&)noexcept;
    };
}

#endif

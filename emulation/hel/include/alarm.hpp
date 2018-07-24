#ifndef __ALARM_HPP__
#define __ALARM_HPP__

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAlarm.h"

namespace hel{
    struct Alarm{
    private:
        bool enabled;
        uint32_t trigger_time;

    public:
        bool getEnabled()const;
        void setEnabled(bool);
        uint32_t getTriggerTime()const;
        void setTriggerTime(uint32_t);
    };
}

#endif

#ifndef _PCM_HPP_
#define _PCM_HPP_

#include "can_device.hpp"
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \brief Data model for the Pneumatics Control Module (PCM)
     */

    struct PCM{ //TODO add support for other PCM data
        /**
         * \var static constexpr uint8_t NUM_SOLENOIDS
         * \brief The maximum number of pneumatic solenoid valves supported by the PCM
         */
        static constexpr uint8_t NUM_SOLENOIDS = 8;

        /**
         * \enum MessageData
         * \brief Interpretation definitions for CAN message data bytes
         */

        enum MessageData{
            SOLENOIDS = 2,
            SIZE = 8
        };

    private:
        /**
         * \var BoundsCheckedArray<bool,NUM_SOLENOIDS> solenoids
         * \brief The states of the solenoids
         */

        BoundsCheckedArray<bool,NUM_SOLENOIDS> solenoids;

    public:
        /**
         * \fn BoundsCheckedArray<bool, NUM_SOLENOIDS> getSolenoids()const noexcept
         * \brief Get the states of the solenoids
         * \return The states of the solenoids
         */

        BoundsCheckedArray<bool, NUM_SOLENOIDS> getSolenoids()const noexcept;

        /**
         * \fn void setSolenoids(uint8_t index, bool value)
         * \brief Set a given solenoid to a given value
         * \param index The index to the solenoid to set
         * \param value The value to set the solenoid to
         */

        void setSolenoid(uint8_t,bool);

        /**
         * \fn void setSolenoids(uint8_t values)
         * \brief Set the states of all solenoids
         * \param values A bitmask of the values to set for the solenoids
         */

        void setSolenoids(uint8_t);

        /**
         * \fn void setSolenoids(const BoundsCheckedArray<bool,NUM_SOLENOIDS>& values)
         * \brief Set the states of all solenoids
         * \param values The values to set for the solenoids
         */

        void setSolenoids(const BoundsCheckedArray<bool,NUM_SOLENOIDS>&);

        /**
         * Constructor for PCM
         */

        PCM()noexcept;

        /**
         * Constructor for PCM
         * \param source A PCM object to copy
         */

        PCM(const PCM&)noexcept;
    };
}

#endif

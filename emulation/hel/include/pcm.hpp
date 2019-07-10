#ifndef _PCM_HPP_
#define _PCM_HPP_

#include "can_device.hpp"
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \brief Data model for the Pneumatics Control Module (PCM)
     */

  struct PCM: public CANDevice{ //TODO add support for other PCM data
        /**
         * \brief The maximum number of pneumatic solenoid valves supported by the PCM
         */

        static constexpr uint8_t NUM_SOLENOIDS = 8;

        /**
         * \brief Interpretation definitions for CAN message data bytes
         */

        enum MessageData{
            SOLENOIDS = 2,
            SIZE = 8
        };

    private:
        /**
         * \brief The states of the solenoids
         */

        BoundsCheckedArray<bool,NUM_SOLENOIDS> solenoids;

    public:
        void parseCANPacket(const int32_t&, const std::vector<uint8_t>&);

        std::vector<uint8_t> generateCANPacket(const int32_t&)const;

        /**
         * \fn BoundsCheckedArray<bool, NUM_SOLENOIDS> getSolenoids()const noexcept
         * \brief Get the states of the solenoids
         * \return The states of the solenoids
         */

        BoundsCheckedArray<bool, NUM_SOLENOIDS> getSolenoids()const noexcept;

        /**
         * \brief Set a given solenoid to a given value
         * \param index The index to the solenoid to set
         * \param value The value to set the solenoid to
         */

        void setSolenoid(uint8_t,bool);

        /**
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
         * \brief Format the PCM object as a string
         * \return A string containing the PCM information
         */

        std::string toString()const;

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

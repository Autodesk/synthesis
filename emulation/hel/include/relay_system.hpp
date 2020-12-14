#ifndef _RELAY_SYSTEM_HPP_
#define _RELAY_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tRelay.h"

#include <string>

namespace hel{
    /**
     * \brief Data model for Relay system.
     * Holds all internal data to model relay outputs.
     */

    struct RelaySystem{
        /**
         * \brief The number of relay headers on the RoboRIO
         */

        static constexpr int32_t NUM_RELAY_HEADERS = 4; //hal::kNumRelayHeaders

        /**
         * \brief Simple representation of the relay state
         */

        enum class State{
            OFF, REVERSE,
            FORWARD, ERROR
        };

    private:

        /**
         * \brief Relay output data
         */

        nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value;

    public:

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue getValue()const
         * \brief Get relay output.
         * \return A tValue object representing the reverse and forward channel outputs.
         */

        nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue getValue()const noexcept;

        /**
         * \fn void setValue(nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value)
         * \brief Set relay output.
         * \param value A tValue object representing the reverse and forward channel outputs.
         */

        void setValue(nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue)noexcept;

        /**
         * \brief Get the state of a given relay channel
         * \param index The relay header to fetch data for
         * \return The state of the header
         */

        State getState(uint8_t)noexcept;

        /**
         * Constructor for RelaySystem
         */

        RelaySystem()noexcept;

        /**
         * Constructor for RelaySystem
         * \param source A RelaySystem object to copy
         */

        RelaySystem(const RelaySystem&)noexcept;
    };

    /**
     * \fn std::string asString(RelaySystem::State state)
     * \brief Format a RelaySystem::State as a string
     * \param state The state to convert
     * \return The resulting string
     */

    std::string asString(RelaySystem::State);
}

#endif

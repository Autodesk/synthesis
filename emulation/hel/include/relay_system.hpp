#ifndef _RELAY_SYSTEM_HPP_
#define _RELAY_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tRelay.h"

namespace hel{
    /**
     * \struct RelaySystem
     * \brief Data model for Relay system.
     * Holds all internal data to model relay outputs.
     */

    struct RelaySystem{
    private:

        /**
         * \var nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value
         * \brief Relay output data
         */

        nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value;

    public:

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue getValue()const
         * \brief Get relay output.
         * Returns the relay output
         * \return a nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue object representing the reverse and forward channel outputs.
         */

        nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue getValue()const;

        /**
         * \fn void setValue(nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value)
         * \brief Set relay output.
         * Sets the relay output to \b value
         * \param value a nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue object representing the reverse and forward channel outputs.
         */

        void setValue(nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue);

        RelaySystem();
    };
}

#endif

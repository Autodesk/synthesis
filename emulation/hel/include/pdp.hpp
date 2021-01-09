#ifndef _PDP_HPP_
#define _PDP_HPP_

#include "can_device.hpp"

namespace hel{

    /**
     * \brief Data model for the Power Distribution Panel (PDP)
     * Currently unsupported by Synthesis
     */

    struct PDP: public CANDevice{

        /**
         * \brief Interpretation definitions for CAN message data bytes
         */

        enum MessageData{
            SIZE = 8
        };

		/**
		 * \brief Format the PDP object as a string
		 * \return The PDP object as a string
		 */

        std::string toString()const;

		/**
		 * \brief Parse a CAN message and update the PDP state
		 * \param api_id The API ID
		 * \param data The CAN data bytes
		 */

        void parseCANPacket(const int32_t&, const std::vector<uint8_t>&);

		/**
		 * \brief Generate a CAN message given an API ID
		 * \param api_id The API ID to use
		 * \return The generated data bytes as determined by the API ID
		 */

        std::vector<uint8_t> generateCANPacket(const int32_t&)const;

        /**
         * Constructor for PDP
         */

        PDP()noexcept;
        /**
         * Constructor for PDP
         * \param source A PDP object to copy
         */

        PDP(const PDP&)noexcept;
    };
}

#endif

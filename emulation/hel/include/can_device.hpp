#ifndef _CAN_DEVICE_HPP_
#define _CAN_DEVICE_HPP_

#include <string>
#include <cstdint>
#include <vector>

namespace hel{

    /**
     * \brief Interprets the CAN message IDs created by HAL
     */

    struct CANMessageID{
		
		/**
		 * \brief The type of CAN device from those supported by HEL
		 */

        enum class Type{
            TALON_SRX,
            VICTOR_SPX,
            SPARK_MAX,
            PCM,
            PDP,
            UNKNOWN
        };

		/**
		 * \brief the Manufacturer of the CAN Device, as defined by HAL
		 */

        enum class Manufacturer: int32_t{
            BROADCAST = 0,
            NI = 1,
            LM = 2,
            DEKA = 3,
            CTRE = 4,
            REV = 5,
            MS = 7,
            TEAM_USE = 8,
         };

        /**
         * \brief The maximum CAN bus address allowed on the RoboRIO
         * Valid addresses are 0-62
         */

        static constexpr uint8_t MAX_CAN_BUS_ADDRESS = 62;

    private:

		/**
		 * \brief The type of the CAN device targeted by this message
		 */

        Type type;
		
		/**
		 * \brief The manufacturer of the CAN device targeted by this message
		 */

        Manufacturer manufacturer;

		/**
		 * \brief The API ID in the message
		 */

        int32_t api_id;

		
		/**
		 * \brief The type of CAN device targeted by this message
		 */

        uint8_t id;

    public:
        
		/**
		 * \brief Parses a bit-packed CAN message ID into separate filed
		 * \param message_id The message ID to parse
		 * \return The parsed message ID
		 */

		static CANMessageID parse(uint32_t);
        
		/**
		 * \brief Generated a bit-packed message ID given certain fields
		 * \param type The type of CAN device
		 * \param manufacturer The manufacturer of the CAN device
		 * \param api_id The API ID in the message
		 * \param id The ID of the CAN device
		 * \return
		 */

        static uint32_t generate(Type, Manufacturer, int32_t, uint8_t)noexcept;
        
        /**
         * \brief Format the CANMessage object as a string
         * \return A string containing the CANMessage information
         */
		
		std::string toString()const;
        
		/**
		 * \brief Fetch the type in the message
		 * \return The type
		 */

        Type getType()const noexcept;
        
		/**
		 * \brief Fetch the manufacturer in the message
		 * \return The manufacturer
		 */

        Manufacturer getManufacturer()const noexcept;
        
		/**
		 * \brief Fetch the API ID in the message
		 * \return The API ID
		 */

        int32_t getAPIID()const noexcept;
        
		/**
		 * \brief Fetch the ID in the message
		 * \return The ID
		 */

        uint8_t getID()const noexcept;
    };
        
	/**
	 * \brief Convert the manufacturer value to a string
	 * \param manufacturer The manufacturer value to convert
	 * \return The manufacturer value as a string
	 */

    std::string asString(CANMessageID::Manufacturer);
        
	/**
	 * \brief Convert the CAN device type value to a string
	 * \param type The CAN device type value to convert
	 * \return The type value as a string
	 */

    std::string asString(CANMessageID::Type);
    
	/**
	 * \brief Parse a string representation of a CAN device type to a CANMessageID Type
	 * \param input The string to parse
	 * \return The CANMessageID Type parsed
	 */

    CANMessageID::Type s_to_can_device_type(std::string);

    /**
     * \brief Models a CAN device on the CAN bus
     */

    struct CANDevice{
    private:

        /**
         * \brief The type of CAN device
         */

        CANMessageID::Type type;

        /**
         * \brief The CAN device ID
         */

        uint8_t id;

    public:
		
		/**
		 * \brief Format the CANDevice as a string
		 * \return The CANDevice as a string
		 */

        virtual std::string toString()const = 0;

		/**
		 * \brief Fetch the type of the CANDevice
		 * \return The type of the CANDevice
		 */

        CANMessageID::Type getType()const noexcept;

		/**
		 * \brief Fetch the ID of the CANDevice
		 * \return The type of the CANDevice
		 */

        uint8_t getID()const noexcept;

		/**
		 * \brief Parse a CAN message and update the CAN device state
		 * \param api_id The API ID
		 * \param data The CAN data bytes
		 */

        virtual void parseCANPacket(const int32_t&, const std::vector<uint8_t>&) = 0;

		/**
		 * \brief Generate a CAN message given an API ID
		 * \param api_id The API ID to use
		 * \return The generated data bytes as determined by the API ID
		 */

        virtual std::vector<uint8_t> generateCANPacket(const int32_t&)const = 0;

		/**
		 * \brief Default constructor for a CANDevice
		 */

        CANDevice()noexcept;

		/**
		 * \brief Copy-constructor for a CANDevice
		 * \param other The CANDevice to copy
		 */

        CANDevice(const CANDevice&)noexcept;

		/**
		 * \brief Constructor for a CANDevice
		 * \param message_id The message ID to use to configure the CAN device
		 */

        CANDevice(CANMessageID)noexcept;
    };
}

#endif

#ifndef _ENCODER_MANAGER_HPP_
#define _ENCODER_MANAGER_HPP_

#include <cstdint>
#include <string>
#include "util.hpp"

namespace hel{

    /**
     * \brief Manager for HEL encoder data
     *
     * The Ni FPGA references either a tCounter or tEncoder object when polling encoder data depending on
     * configuration. This class is used to map engine encoder data to either an FPGAEncoder or a
     * Counter depending on robot model export data to ensure the user code finds encoder data where
     * it expects it
     */

    struct EncoderManager{
        /**
         * \brief The types of ports encoders can use
         */

        enum class PortType{
            DI,
            AI
        };

        /**
         * \brief The type of encoder
         *
         * Whether the encoder manager maps to a tCounter, a tEncoder, or there is a misconfiguration
         */

        enum class Type{
            UNKNOWN,
            FPGA_ENCODER,
            COUNTER
        };

    private:
        /**
         * \brief The type of encoder
         */

        Type type;

        /**
         * \brief The index into the FPGAEncoder or Counter array to map to
         */

        uint8_t index;

        /**
         * \brief The port index the encoder a channel is attached to
         */

        uint8_t a_channel;

        /**
         * \brief The type of port the encoder a channel is attached to
         */

        PortType a_type;

        /**
         * \brief The port index the encoder b channel is attached to
         */

        uint8_t b_channel;

        /**
         * \brief The type of port the encoder b channel is attached to
         */

        PortType b_type;

        /**
         * \brief The number of ticks the encoder has counted
         */

        int32_t ticks;

        /**
         * \brief Check the EncoderManager configuration against tEncoder or tCounter configurations
         * This is used to determine what device to map the EncoderManager to
         * \param a The configuration's a channel value
         * \param a_module The configuration's a module value
         * \param a_analog The configuration's b analog value
         * \param b The configuration's b channel value
         * \param b_module The configuration's b module value
         * \param b_analog The configuration's b analog value
         * \return True if the devices match
         */

        bool checkDevice(uint8_t, bool, bool, uint8_t, bool, bool)const noexcept;

        /**
         * \brief Update the EncoderManager's type and index given the FPGAEncoders and Counters
         */

        void updateDevice();

    public:
        /**
         * \brief Get the type of encoder the manager is mapped to
         * \return The encoder type
         */

        Type getType()const noexcept;

        /**
         * \brief Get the index into the FPGAEncoder or Counter array the manager is mapped to
         * \return The index to reference
         */

        uint8_t getIndex()const noexcept;

        /**
         * \brief Get the port of the encoder's a channel
         * \return The port index
         */

        uint8_t getAChannel()const noexcept;

        /**
         * \brief Set the encoder port for the a channel
         * \param a The port to use as the a channel
         */

        void setAChannel(uint8_t)noexcept;

        /**
         * \brief Get the port type channel a is attached to
         * \return The type of port channel a is attached to
         */

        PortType getAType()const noexcept;

        /**
         * \brief Set the port type channel a is attached to
         * \param a_t The type of port to use for channel a
         */

        void setAType(PortType)noexcept;

        /**
         * \brief Get the port of the encoder's b channel
         * \return The port index
         */

        uint8_t getBChannel()const noexcept;

        /**
         * \brief Set the encoder port for the b channel
         * \param b The port to use as the b channel
         */

        void setBChannel(uint8_t)noexcept;

        /**
         * \brief Get the port type channel b is attached to
         * \return The type of port channel b is attached to
         */

        PortType getBType()const noexcept;

        /**
         * \brief Set the port type channel b is attached to
         * \param b_t The type of port to use for channel b
         */

        void setBType(PortType)noexcept;

        /**
         * \brief Set the ticks of the encoder
         * \param ticks The number of encoders
         */

        void setTicks(int32_t)noexcept;

        /**
         * \brief Get the number of ticks the encoder has counted
         * \return The ticks the encoder has counted
         */

        int32_t getTicks()const noexcept;

        /**
         * \brief Updates the EncoderManager's type and index and the ticks of its corresponding FPGAEncoder or Counter
         */

        void update();

        /**
         * \brief Format the EncoderManager data as a string
         * \return The EncoderManager data in string format
         */

        std::string toString()const;

        /**
         * Constructor for EncoderManager
         */

        EncoderManager()noexcept;

        /**
         * Constructor for EncoderManager
         * \param source An EncoderManager object to copy
         */

        EncoderManager(const EncoderManager&)noexcept;

        /**
         * Constructor for EncoderManager
         * \param a_channel The port the encoder's a channel is attached to
         * \param a_type The type of port the encoder's a channel is attached to
         * \param b_channel The port the encoder's b channel is attached to
         * \param b_type The type of port the encoder's b channel is attached to
         */

        EncoderManager(uint8_t,PortType,uint8_t,PortType)noexcept;
    };

    /**
     * \fn std::string asString(EncoderManager::Type type)
     * \brief Convert an EncoderManager::Type to a string
     * \param type The Encoder::Manager::Type to convert
     * \return The type as a string
     */

    std::string asString(EncoderManager::Type);

    /**
     * \fn std::string asString(EncoderManager::PortType port_type)
     * \brief Convert the EncoderManager::PortType to a string
     * \param port_type The EncoderManager::PortType to convert
     * \return The port type as a string
     */

    std::string asString(EncoderManager::PortType);
}

#endif

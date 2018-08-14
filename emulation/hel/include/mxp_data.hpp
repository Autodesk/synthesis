#ifndef _MXP_DATA_HPP_
#define _MXP_DATA_HPP_

#include <string>

namespace hel{
    /**
     * \brief Represents a RoboRIO MXP configurations and values
     */

    struct MXPData{
        /**
         * \enum Config
         * \brief Represents the various possible MXP configurations
         */

        enum class Config{
            DI, DO, PWM, SPI, I2C
        };

        /**
         * \brief The configuration of the port
         */

        Config config;

        /**
         * \brief The value of the port
         * Must be interpreted using the context of the configuration
         */

        double value;

        /**
         * \brief Format the MXPData object as a string
         * \return A string containing the MXPData information
         */

        std::string toString()const;

        /**
         * \brief Convert the port to a JSON object
         * \return A string representing the data in JSON format
         */

        std::string serialize()const;

        /**
         * \brief Convert a JSON object string to an MXPData object
         * \param input The data to parse
         * \return The generated MXPData object
         */

        static MXPData deserialize(std::string);

        /**
         * Constructor for MXPData
         */

        MXPData()noexcept;

        /**
         * Constructor for MXPData
         * \param source An MXPData object to copy
         */

        MXPData(const MXPData&)noexcept;
    };

    /**
     * \brief Convert a string to an MXPData::Config
     * \param input The string to parse
     * \return The parsed MXPData::Config
     */

    MXPData::Config s_to_mxp_config(std::string);

    /**
     * std::string as_string(MXPData::Config config)
     * \brief Convert an MXP::Config value to a string
     * \param config The value to convert
     * \return The value in string format
     */

    std::string as_string(MXPData::Config);
}

#endif

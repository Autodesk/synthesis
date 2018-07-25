#ifndef _MXP_DATA_HPP_
#define _MXP_DATA_HPP_

#include <string>

namespace hel{
    /**
     * \struct MXPData
     * \brief Represents a RoboRIO MXP configurations and values
     */

    struct MXPData{
        /**
         * \enum class Config
         * \brief Represents the various possible MXP configurations
         */

        enum class Config{
            DI, DO, PWM, SPI, I2C
        };

        /**
         * \var Config config
         * \brief The configuration of the port
         */

        Config config;

        /**
         * \var double value
         * \brief The value of the porti
         * Must be interpreted using the context of the configuration
         */

        double value;

        /**
         * \fn std::string serialize()const
         * \brief Convert the port to a JSON object
         * \return a string representing the data in JSON format
         */

        std::string serialize()const;

        /**
         * \fn static MXPData deserialize(std::string input)
         * \brief Convert a JSON object string to an MXPData object
         * \param input the data to parse
         * \return the generated MXPData object
         */

        static MXPData deserialize(std::string);

        MXPData();
    };

    MXPData::Config s_to_mxp_config(std::string);

    std::string to_string(MXPData::Config);
}

#endif

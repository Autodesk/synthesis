#ifndef _MATCH_INFO_HPP_
#define _MATCH_INFO_HPP_

#include "FRC_NetworkCommunication/FRCComm.h"

#include <string>

namespace hel{

    /**
     * \brief A data container for match/driver station information
     * Holds all of the match data communicated to the robot via the driver station
     */

    struct MatchInfo{
    private:

        /**
         * \var std::string event_name
         * \brief A string representing the name of the event
         */

        std::string event_name;

        /**
         * \var std::string game_specific_message
         * \brief Represents any game-specific information
         * The FMS will generate any game-specific information and communicate it to the robots.
         */

        std::string game_specific_message;

        /**
         * \var MatchType_t match_type
         * \brief Represents which type of match is running
         */

        MatchType_t match_type;

        /**
         * \var uint16_t match_number
         * \brief Represents the match number at the event
         */

        uint16_t match_number;

        /**
         * \var uint8_t replay_number
         * \brief An byte representing if the match is a replay and which it is
         */

        uint8_t replay_number;

        /**
         * \var AllianceStationID_t alliance_station_id
         * \brief Represents which driver station position the robot is running from
         */

        AllianceStationID_t alliance_station_id;

        /**
         * \var double match_time
         * \brief Represents match time in seconds
         */

        double match_time;

    public:

        /**
         * \fn std::string getEventName()const noexcept
         * \brief Fetch a string representing the event name
         * \return A string representing the event name
         */

        std::string getEventName()const noexcept;

        /**
         * \fn void setEventName(std::string event_name)noexcept
         * \brief Set the name of the event
         * \param event_name A string representing the name of the event
         */

        void setEventName(std::string)noexcept;

        /**
         * \fn std::string getGameSpecificMessage()const noexcept
         * \brief Fetch any game specific message for the match
         * \return A string representing any game specific message
         */

        std::string getGameSpecificMessage()const noexcept;

        /**
         * \fn void setGameSpecificMessage(std::string game_specific_message)noexcept
         * \brief Set the game specific message for the match
         * \param game_specific_message The game specific message for the match
         */

        void setGameSpecificMessage(std::string)noexcept;

        /**
         * \fn MatchType_t getMatchType()const noexcept
         * \brief Fetch the type of match
         * \return A MatchType_t object representing the type of match
         */

        MatchType_t getMatchType()const noexcept;

        /**
         * \fn void setMatchType(MatchType_t match_type)noexcept
         * \brief Set the type of match
         * \param match_type The type of match running
         */

        void setMatchType(MatchType_t)noexcept;

        /**
         * \fn uint16_t getMatchNumber()const noexcept
         * \brief Fetch the match number at the event
         * \return A 16-bit integer representing the match number
         */

        uint16_t getMatchNumber()const noexcept;

        /**
         * \fn void setMatchNumber(uint16_t match_number)noexcept
         * \brief Set the match number at the event
         * \param match_number The running match number
         */

        void setMatchNumber(uint16_t)noexcept;

        /**
         * \fn uint8_t getReplayNumber()const noexcept
         * \brief Get the replay number for the running match
         * \return A byte representing the replay number of the running match (0 if not a replay)
         */

        uint8_t getReplayNumber()const noexcept;

        /**
         * \fn void setReplayNumber(uint8_t replay_number)noexcept
         * \brief Set the replay number for the running match
         * \param replay_number A byte representing the replay number for the running match (0 if not a replay)
         */

        void setReplayNumber(uint8_t)noexcept;

        /**
         * \fn AllianceStationID_t getAllianceStationID()const noexcept
         * \brief Fetch the driver station position controlling the robot
         * \return An AllianceStationID_t object representing the robot's driver station ID
         */

        AllianceStationID_t getAllianceStationID()const noexcept;

        /**
         * \fn void setAllianceStationID(AllianceStationID_t alliance_station_id)noexcept
         * \brief Set the driver station position for the robot's drivers
         * \param alliance_station_id The driver station position for the robot
         */

        void setAllianceStationID(AllianceStationID_t)noexcept;

        /**
         * \fn double getMatchTime()const noexcept
         * \brief Get the match time in seconds
         * \return A double representing the match time in seconds
         */

        double getMatchTime()const noexcept;

        /**
         * \fn void setMatchTime(double match_time)noexcept
         * \brief Set the match time
         * \param match_time A double representing the match time in seconds
         */

        void setMatchTime(double)noexcept;

        /**
         * \fn static MatchInfo deserialize(std::string input)
         * \brief Deserialize a JSON string into a MatchInfo object
         * \param input The JSON to parse
         * \return The parsed MatchInfo object
         */

        static MatchInfo deserialize(std::string);

        /**
         * \fn std::string serialize()const
         * \brief Format the match information as JSON
         * \return The match information in JSON format
         */

        std::string serialize()const;

        /**
         * \fn std::string toString()const
         * \brief Convert the match information to string format
         * \return A string containing the match info
         */

        std::string toString()const;

        /**
         * Constructor for MatchInfo
         */

        MatchInfo()noexcept;

        /**
         * Constructor for MatchInfo
         * \param source A MatchInfo object to copy
         */

        MatchInfo(const MatchInfo&)noexcept;
    };
}

#endif

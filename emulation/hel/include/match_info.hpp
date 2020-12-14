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
        /**
         * \brief The Maximum size of the event name string in characters
         */

        static constexpr unsigned MAX_EVENT_NAME_SIZE = 64;

        /**
         * \brief The Maximum size of the game specific message string in characters
         */

        static constexpr unsigned MAX_GAME_SPECIFIC_MESSAGE_SIZE = 64;
    private:

        /**
         * \brief A string representing the name of the event
         */

        std::string event_name;

        /**
         * \brief Represents any game-specific information
         * The FMS will generate any game-specific information and communicate it to the robots.
         */

        std::string game_specific_message;

        /**
         * \brief Represents which type of match is running
         */

        MatchType_t match_type;

        /**
         * \brief Represents the match number at the event
         */

        uint16_t match_number;

        /**
         * \brief An byte representing if the match is a replay and which it is
         */

        uint8_t replay_number;

        /**
         * \brief Represents which driver station position the robot is running from
         */

        AllianceStationID_t alliance_station_id;

        /**
         * \brief Represents match time in seconds
         */

        double match_time;

    public:

        /**
         * \brief Fetch a string representing the event name
         * \return A string representing the event name
         */

        std::string getEventName()const noexcept;

        /**
         * \brief Set the name of the event
         * \param name A string representing the name of the event
         */

        void setEventName(std::string)noexcept;

        /**
         * \brief Fetch any game specific message for the match
         * \return A string representing any game specific message
         */

        std::string getGameSpecificMessage()const noexcept;

        /**
         * \brief Set the game specific message for the match
         * \param message The game specific message for the match
         */

        void setGameSpecificMessage(std::string)noexcept;

        /**
         * \brief Fetch the type of match
         * \return A MatchType_t object representing the type of match
         */

        MatchType_t getMatchType()const noexcept;

        /**
         * \brief Set the type of match
         * \param type The type of match running
         */

        void setMatchType(MatchType_t)noexcept;

        /**
         * \brief Get the match number for the running match
         * \return A byte representing the match number of the running match
         */

        uint16_t getMatchNumber()const noexcept;

        /**
         * \brief Set the match number at the event
         * \param number The running match number
         */

        void setMatchNumber(uint16_t)noexcept;

        /**
         * \brief Get the replay number for the running match
         * \return A byte representing the replay number of the running match (0 if not a replay)
         */

        uint8_t getReplayNumber()const noexcept;

        /**
         * \brief Set the replay number for the running match
         * \param number A byte representing the replay number for the running match (0 if not a replay)
         */

        void setReplayNumber(uint8_t)noexcept;

        /**
         * \brief Fetch the driver station position controlling the robot
         * \return An AllianceStationID_t object representing the robot's driver station ID
         */

        AllianceStationID_t getAllianceStationID()const noexcept;

        /**
         * \brief Set the driver station position for the robot's drivers
         * \param id The driver station position for the robot
         */

        void setAllianceStationID(AllianceStationID_t)noexcept;

        /**
         * \brief Get the match time in seconds
         * \return A double representing the match time in seconds
         */

        double getMatchTime()const noexcept;

        /**
         * \brief Set the match time
         * \param time A double representing the match time in seconds
         */

        void setMatchTime(double)noexcept;

        /**
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

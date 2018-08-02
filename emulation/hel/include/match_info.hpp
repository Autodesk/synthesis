#ifndef _MATCH_INFO_HPP_
#define _MATCH_INFO_HPP_

#include "FRC_NetworkCommunication/FRCComm.h"

#include <string>

namespace hel{

    /**
     * \struct MatchInfo
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
         * \fn std::string getEventName()const
         * \brief Fetch a string representing the event name
         * return a standard string representing the event name
         */

        std::string getEventName()const noexcept;

        /**
         * \fn void setEventName(std::string event_name)
         * \brief Set the name of the event
         * \param event_name a standard string representing the name of the event
         */

        void setEventName(std::string)noexcept;

        /**
         * \fn std::string getGameSpecificMessage()const
         * \brief Fetch any game specific message for the match
         * \return a standard string representing any game specific message
         */

        std::string getGameSpecificMessage()const noexcept;

        /**
         * \fn void setGameSpecificMessage(std::string game_specific_message)
         * \brief Set the game specific message for the match
         * \param game_specific_message the game specific message for the match
         */

        void setGameSpecificMessage(std::string)noexcept;

        /**
         * \fn MatchType_t getMatchType()const
         * \brief Fetch the type of match
         * \return a MatchType_t object representing the type of match
         */

        MatchType_t getMatchType()const noexcept;

        /**
         * \fn void setMatchType(MatchType_t match_type)
         * \brief Set the tye of match
         * \param match_type the type of match running
         */

        void setMatchType(MatchType_t)noexcept;

        /**
         * \fn uint16_t getMatchNumber()const
         * \brief Fetch the match number at the event
         * \return a 16-bit integer representing the match number
         */

        uint16_t getMatchNumber()const noexcept;

        /**
         * \fn void setMatchNumber(uint16_t match_number)
         * \brief Set the match number at the event
         * \param match_number the running match number
         */

        void setMatchNumber(uint16_t)noexcept;

        /**
         * \fn uint8_t getReplayNumber()const
         * \brief Get the replay number for the running match
         * \return a byte representing the replay number of the running match (0 if not a replay)
         */

        uint8_t getReplayNumber()const noexcept;

        /**
         * \fn void setReplayNumber(uint8_t replay_number)
         * \brief Set the replay number for the running match
         * \param replay_number a byte representing the replay number for the running match (0 if not a replay)
         */

        void setReplayNumber(uint8_t)noexcept;

        /**
         * \fn AllianceStationID_t getAllianceStationID()const
         * \brief Fetch the driver station position controlling the robot
         * \return an AllianceStationID_t object representing the robot's driver station ID
         */

        AllianceStationID_t getAllianceStationID()const noexcept;

        /**
         * \fn void setAllianceStationID(AllianceStationID_t alliance_station_id)
         * \brief Set the driver station position for the robot's drivers
         * \param alliance_station_id the driver station position for the robot
         */

        void setAllianceStationID(AllianceStationID_t)noexcept;

        /**
         * \fn double getMatchTime()const
         * \brief Get the match time in seconds
         * \return a double representing the match time in seconds
         */

        double getMatchTime()const noexcept;

        /**
         * \fn void SetMatchTime(double match_time)
         * \brief Set the match time
         * \param match_time a double representing the match time in seconds
         */

        void setMatchTime(double)noexcept;

        static MatchInfo deserialize(std::string);

        std::string serialize()const;

        std::string toString()const;

        MatchInfo()noexcept;
    };
}

#endif

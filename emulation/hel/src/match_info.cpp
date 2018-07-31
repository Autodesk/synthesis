#include "match_info.hpp"

namespace hel{
    std::string MatchInfo::getEventName()const{
        return event_name;
    }

    void MatchInfo::setEventName(std::string name){
        event_name = name;
    }

    std::string MatchInfo::getGameSpecificMessage()const{
        return game_specific_message;
    }

    void MatchInfo::setGameSpecificMessage(std::string message){
        game_specific_message = message;
    }

    MatchType_t MatchInfo::getMatchType()const{
        return match_type;
    }

    void MatchInfo::setMatchType(MatchType_t type){
        match_type = type;
    }

    uint16_t MatchInfo::getMatchNumber()const{
        return match_number;
    }

    void MatchInfo::setMatchNumber(uint16_t number){
        match_number = number;
    }

    uint8_t MatchInfo::getReplayNumber()const{
        return replay_number;
    }

    void MatchInfo::setReplayNumber(uint8_t number){
        replay_number = number;
    }

    AllianceStationID_t MatchInfo::getAllianceStationID()const{
        return alliance_station_id;
    }

    void MatchInfo::setAllianceStationID(AllianceStationID_t id){
        alliance_station_id = id;
    }

    double MatchInfo::getMatchTime()const{
        return match_time;
    }

    void MatchInfo::setMatchTime(double time){
        match_time = time;
    }

    MatchInfo::MatchInfo():event_name(),game_specific_message(),match_type(),match_number(0),replay_number(0),alliance_station_id(),match_time(){}
}

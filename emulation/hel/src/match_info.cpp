#include "match_info.hpp"

#include "error.hpp"
#include "util.hpp"

namespace hel{
    std::string MatchInfo::getEventName()const noexcept{
        return event_name;
    }

    void MatchInfo::setEventName(std::string name)noexcept{
        event_name = name;
    }

    std::string MatchInfo::getGameSpecificMessage()const noexcept{
        return game_specific_message;
    }

    void MatchInfo::setGameSpecificMessage(std::string message)noexcept{
        game_specific_message = message;
    }

    MatchType_t MatchInfo::getMatchType()const noexcept{
        return match_type;
    }

    void MatchInfo::setMatchType(MatchType_t type)noexcept{
        match_type = type;
    }

    uint16_t MatchInfo::getMatchNumber()const noexcept{
        return match_number;
    }

    void MatchInfo::setMatchNumber(uint16_t number)noexcept{
        match_number = number;
    }

    uint8_t MatchInfo::getReplayNumber()const noexcept{
        return replay_number;
    }

    void MatchInfo::setReplayNumber(uint8_t number)noexcept{
        replay_number = number;
    }

    AllianceStationID_t MatchInfo::getAllianceStationID()const noexcept{
        return alliance_station_id;
    }

    void MatchInfo::setAllianceStationID(AllianceStationID_t id)noexcept{
        alliance_station_id = id;
    }

    double MatchInfo::getMatchTime()const noexcept{
        return match_time;
    }

    void MatchInfo::setMatchTime(double time)noexcept{
        match_time = time;
    }

    std::string to_string(MatchType_t type){
        switch(type){
        case MatchType_t::kMatchType_none:
            return "NONE";
        case MatchType_t::kMatchType_practice:
            return "PRACTICE";
        case MatchType_t::kMatchType_elimination:
            return "ELIMINATION";
        case MatchType_t::kMatchType_qualification:
            return "QUALIFICATION";
        default:
            throw UnhandledEnumConstantException("MatchType_t");
        }
    }

    MatchType_t s_to_match_type(std::string s){
        switch(hasher(s.c_str())){
        case hasher("NONE"):
            return MatchType_t::kMatchType_none;
        case hasher("PRACTICE"):
            return MatchType_t::kMatchType_practice;
        case hasher("ELIMINATION"):
            return MatchType_t::kMatchType_elimination;
        case hasher("QUALIFICATION"):
            return MatchType_t::kMatchType_qualification;
        default:
            throw UnhandledCase();
        }
    }

    std::string to_string(AllianceStationID_t id){
        switch(id){
        case AllianceStationID_t::kAllianceStationID_blue1:
            return "BLUE1";
        case AllianceStationID_t::kAllianceStationID_blue2:
            return "BLUE2";
        case AllianceStationID_t::kAllianceStationID_blue3:
            return "BLUE3";
        case AllianceStationID_t::kAllianceStationID_red1:
            return "RED1";
        case AllianceStationID_t::kAllianceStationID_red2:
            return "RED2";
        case AllianceStationID_t::kAllianceStationID_red3:
            return "RED3";
        default:
            throw UnhandledEnumConstantException("MatchType_t");
        }
    }

    AllianceStationID_t s_to_alliance_station_id(std::string s){
        switch(hasher(s.c_str())){
        case hasher("BLUE1"):
            return AllianceStationID_t::kAllianceStationID_blue1;
        case hasher("BLUE2"):
            return AllianceStationID_t::kAllianceStationID_blue2;
        case hasher("BLUE3"):
            return AllianceStationID_t::kAllianceStationID_blue3;
        case hasher("RED1"):
            return AllianceStationID_t::kAllianceStationID_red1;
        case hasher("RED2"):
            return AllianceStationID_t::kAllianceStationID_red2;
        case hasher("RED3"):
            return AllianceStationID_t::kAllianceStationID_red3;
        default:
            throw UnhandledCase();
        }
    }

    std::string MatchInfo::toString()const{
        std::string s = "(";
        s += "event_name:" + event_name + ", ";
        s += "game_specific_message:" + game_specific_message + ", ";
        s += "match_type:" + asString(match_type) + ", ";
        s += "match_number:" + std::to_string(match_number) + ", ";
        s += "replay_number:" + std::to_string(replay_number) + ", ";
        s += "alliance_station_id:" + asString(alliance_station_id) + ", ";
        s += "match_time:" + std::to_string(match_time);
        s += ")";
        return s;
    }

    MatchInfo::MatchInfo()noexcept:event_name(""),game_specific_message(""),match_type(MatchType_t::kMatchType_none),match_number(0),replay_number(0),alliance_station_id(AllianceStationID_t::kAllianceStationID_red1),match_time(0){}
    MatchInfo::MatchInfo(const MatchInfo& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(event_name);
        COPY(game_specific_message);
        COPY(match_type);
        COPY(match_number);
        COPY(replay_number);
        COPY(alliance_station_id);
        COPY(match_time);
#undef COPY
    }
}

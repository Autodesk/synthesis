#include "sync_client.hpp"
#include "receive_data.hpp"

#include <unistd.h>
#include "util.hpp"
#include "global.hpp"
#include "json_util.hpp"

#define ETHERNET_MTU 1516

constexpr char DEFAULT_DESERIALIZATION_DATA[] = "{\"roborio\":{\"digital_hdrs\":[0,0,0,0,0,0,0,0,0,0],\"joysticks\":[{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0},{\"is_xbox\":0,\"type\":0,\"name\":\"\",\"buttons\":0,\"button_count\":0,\"axes\":[0,0,0,0,0,0,0,0,0,0,0,0],\"axis_count\":0,\"axis_types\":[0,0,0,0,0,0,0,0,0,0,0,0],\"povs\":[0,0,0,0,0,0,0,0,0,0,0,0],\"pov_count\":0,\"outputs\":0,\"left_rumble\":0,\"right_rumble\":0}],\"digital_mxp\":[{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0},{\"config\":\"DI\",\"value\":0.0}],\"match_info\":{\"event_name\":\"\",\"game_specific_message\":\"\",\"match_type\":\"NONE\",\"match_number\":0,\"replay_number\":0,\"alliance_station_id\":\"RED1\",\"match_time\":0.0},\"robot_mode\":{\"mode\":\"TELEOPERATED\",\"enabled\":0,\"emergency_stopped\":0,\"fms_attached\":0,\"ds_attached\":0},\"encoders\":[null,null,null,null,null,null,null,null]}}"; //TODO replace with shallow and deep versions

namespace hel {

    SyncClient::SyncClient(asio::io_service& io)   {
        endpoint = asio::ip::tcp::endpoint(asio::ip::tcp::v4(), RECEIVE_PORT);
        startSync(io);
    }

    std::string readJSONPacket(asio::ip::tcp::socket& socket, std::string& rest){
        std::array<char, ETHERNET_MTU + 1> data;
        std::fill(data.begin(), data.end(), '\0');
        int bytes_received = socket.receive(asio::buffer(data));
        std::string received_data = rest;
        received_data += std::string(data.begin(), data.begin()+bytes_received);
        rest = "";
        const std::string PREAMBLE = "{\"roborio";
        if(received_data.substr(0, PREAMBLE.length()) != PREAMBLE) {
            if (received_data.find(JSON_PACKET_SUFFIX) == std::string::npos) {
                rest = received_data;
            } else {
                rest = received_data.substr(received_data.find(JSON_PACKET_SUFFIX) + 1);
            }
            return readJSONPacket(socket,rest);
        }
        std::size_t i = received_data.find(JSON_PACKET_SUFFIX);
        if(i == std::string::npos) {
            rest = received_data;
            return readJSONPacket(socket,rest);
        }
        rest = received_data.substr(i + 1);
        return received_data.substr(0, i);
    }

    void SyncClient::startSync(asio::io_service& io) {
        while(1) {
            asio::ip::tcp::socket socket(io);
            asio::ip::tcp::acceptor acceptor(io, endpoint);
            acceptor.accept(socket);
            std::string rest = "";
            std::string json_string = "";
            try {
                while(1) {
                    json_string = readJSONPacket(socket,rest);
                    auto instance = ReceiveDataManager::getInstance();
                    instance.first->deserializeShallow(json_string);
                    instance.first->updateShallow();
                    instance.second.unlock();
                    usleep(30000);
                }
            } catch(std::system_error&) {
                warn("Receiver socket disconnected. User code will continue to run, but inputs will be set to default.");
                auto instance = ReceiveDataManager::getInstance();
                instance.first->deserializeDeep(std::string(DEFAULT_DESERIALIZATION_DATA));
                instance.first->updateDeep();
                instance.second.unlock();
            }
        }
    }
}

#include "sync_client.hpp"
#include "receive_data.hpp"

#include <unistd.h>
#include <iostream>
#include "util.hpp"
#include "global.hpp"

using asio::ip::tcp;

#define ETHERNET_MTU 1516

namespace hel {

    SyncClient::SyncClient(asio::io_service& io)   {
        endpoint = asio::ip::tcp::endpoint(asio::ip::tcp::v4(), 11000);
        startSync(io);
    }

    void SyncClient::startSync(asio::io_service& io) {
        while(1) {
            asio::ip::tcp::socket socket(io);
            asio::ip::tcp::acceptor acceptor(io, endpoint);
            acceptor.accept(socket);
            std::string rest = "";
            std::string json_string = "";
            bool finished_flag = false;
            try {
                while(1) {

                    while(!finished_flag) {

                        std::array<char, ETHERNET_MTU+1> data;
                        std::fill(data.begin(), data.end(), '\0');
                        int bytes_received = socket.receive(asio::buffer(data));
                        std::string received_data = rest;
                        received_data += std::string(data.begin(), data.begin()+bytes_received);
                        rest = "";
                        const std::string PREAMBLE = "{\"roborio";
                        if(received_data.substr(0, PREAMBLE.length()) != PREAMBLE) {
                            if (received_data.find("\x1B") == std::string::npos) {
                                rest = received_data;
                            } else {
                                rest = received_data.substr(received_data.find("\x1B")+1);
                            }
                            continue;
                        }
                        std::size_t i = received_data.find("\x1B");
                        if(i == std::string::npos) {
                            rest = received_data;
                            continue;
                        }
                        json_string = received_data.substr(0,i);
                        rest = received_data.substr(i+1);
                        finished_flag = true;
                    }
                    //std::cout << json_string << "\n";
                    auto instance = hel::ReceiveDataManager::getInstance();
                    instance.first->deserializeAndUpdate(json_string); //
                    instance.second.unlock(); //
                    finished_flag = false;
                    usleep(5000); //
                }
            }
            catch(...) {
                std::cout << std::flush << "Socket disconnected\n";
            }
        }
    }
}

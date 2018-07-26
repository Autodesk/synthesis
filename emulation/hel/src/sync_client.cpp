#include "sync_client.hpp"
#include "receive_data.hpp"

#include <unistd.h>
#include <iostream>
#include "util.hpp"
#include "json_util.hpp"

using asio::ip::tcp;

#define ETHERNET_MTU 1516

namespace hel {

    SyncClient::SyncClient(asio::io_service& io)   {
        endpoint = asio::ip::tcp::endpoint(asio::ip::tcp::v4(), 11000);
        startSync(io);
    }

    void SyncClient::startSync(asio::io_service& io) {
        asio::ip::tcp::socket socket(io);
        asio::ip::tcp::acceptor acceptor(io, endpoint);
        acceptor.accept(socket);
        std::string remaining_data = "";
        std::string json_string = "";
        while(1) {
            auto instance = hel::ReceiveDataManager::getInstance();

            while(1) {
                std::array<char, ETHERNET_MTU> data;
                socket.receive(asio::buffer(data));

                std::string received_data = remaining_data;
                received_data += data.data();//hel::to_string(data, (std::function<std::string(char)>)[](char a){return std::string(1,a);}, "", false);

                const std::string PREAMBLE = "{\"roborio"; //TODO use json packet suffix instead
                if(received_data.substr(0, PREAMBLE.length()) != PREAMBLE) {
                    unsigned i = 0; // EXPLAIN LATER

                    while(received_data[i] != hel::JSON_PACKET_SUFFIX) {
                        i++;
                        if (i >= received_data.length()) {
                            break;
                        }
                    }
                    received_data = ((i+1) >= received_data.length())? "" : received_data.substr(i+1);
                    remaining_data = received_data;
                    continue;
                }
                for (unsigned i = 0; i < received_data.length(); i++) {
                    if (received_data[i] == hel::JSON_PACKET_SUFFIX) {
                        json_string = received_data.substr(0,i);
                        remaining_data = received_data.substr(i+1);
                        goto end;
                    }
                }
            }
        end:
            instance.first->deserializeAndUpdate(json_string);
            instance.second.unlock();
            usleep(100000);
        }
    }
}

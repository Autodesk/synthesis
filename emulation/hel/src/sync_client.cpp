#include "sync_client.hpp"
#include "receive_data.hpp"

#include <unistd.h>
#include <iostream>
#include "util.hpp"

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
        std::string rest = "";
        std::string json_string = "";
        while(1) {

            auto instance = hel::ReceiveDataManager::getInstance();


            while(1) {
                std::array<char, ETHERNET_MTU> data;
                socket.receive(asio::buffer(data));
                std::string received_data = rest;
                received_data += hel::to_string(data, (std::function<std::string(char)>)[](char a){return std::string(1,a);}, "", false);
                rest = "";
                const std::string PREAMBLE = "{\"roborio";
                if(received_data.substr(0, PREAMBLE.length()) != PREAMBLE) {
                    unsigned i = 0; // EXPLAIN LATER
                    
                    while(received_data[i] != '\x1B') {
                        i++;
                        if (i >= received_data.length()) {
                            break;
                        }
                    }
                    received_data = ((i+1) >= received_data.length())? "" : received_data.substr(i+1);
                    rest = received_data;
                    continue;
                }
                for (unsigned i = 0; i < received_data.length(); i++) {
                    if (received_data[i] == '\x1B') {
                        json_string = received_data.substr(0,i);
                        rest = received_data.substr(i+1);
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

#include "sync_server.hpp"
#include "send_data.hpp"

#include <unistd.h>
#include <iostream>

using asio::ip::tcp;

namespace hel {

    void handle(const asio::error_code&, std::size_t){}

    SyncServer::SyncServer(asio::io_service& io)   {
        endpoint = asio::ip::tcp::endpoint(asio::ip::tcp::v4(), 11001);
        startSync(io);
    }

    void SyncServer::startSync(asio::io_service& io) {
        asio::ip::tcp::socket socket(io);
        asio::ip::tcp::acceptor acceptor(io, endpoint);
        acceptor.accept(socket);
        while(1) {

            auto instance = hel::SendDataManager::getInstance();

            auto data =  instance.first->serialize();
            instance.second.unlock();

            std::cout << data << "\n";
            asio::write(socket, asio::buffer(data), asio::transfer_all());

            usleep(30000);
        }
    }
}

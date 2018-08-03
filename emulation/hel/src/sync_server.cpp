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
        while(1) {
            asio::ip::tcp::socket socket(io);
            asio::ip::tcp::acceptor acceptor(io, endpoint);
            acceptor.accept(socket);
            while(1) {

                auto instance = hel::SendDataManager::getInstance();

                //if(instance.first->hasNewData()){//TODO
                auto data =  instance.first->serializeShallow();
                instance.second.unlock();
                try {
                    asio::write(socket, asio::buffer(data), asio::transfer_all());
                }
                catch(std::system_error){
                    std::cout << std::flush << "Sender Socket disconnected\n";
                    break;
                }
                //} else {
                    //instance.second.unlock();
                //}
                usleep(30000);
            }
        }
    }
}

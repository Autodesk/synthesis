#include "sync_server.hpp"
#include "roborio_manager.hpp"
#include "send_data.hpp"

#include <unistd.h>

namespace hel {

    SyncServer::SyncServer(asio::io_service& io)   {
        endpoint = asio::ip::tcp::endpoint(asio::ip::tcp::v4(), SEND_PORT);
        startSync(io);
    }

    void SyncServer::startSync(asio::io_service& io) {
        while(1) {
            asio::ip::tcp::socket socket(io);
            asio::ip::tcp::acceptor acceptor(io, endpoint);
            acceptor.accept(socket);
            std::string data = "";
            while(1) {
                auto instance = SendDataManager::getInstance();

                if(instance.first->hasNewData()){
                    data = instance.first->serializeShallow();
                    instance.second.unlock();
                    try {
                        asio::write(socket, asio::buffer(data), asio::transfer_all());
                    } catch(std::system_error&){
                        warn("Sender socket disconnected. User code will continue to run.");
                        break;
                    }
                } else {
                    instance.second.unlock();
                }
                usleep(30000);
            }
        }
    }
}

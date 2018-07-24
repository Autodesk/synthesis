#include "sync_server.hpp"
#include "send_data.hpp"

#include <unistd.h>
namespace hel {

    void handle(const asio::error_code&, std::size_t){}


    void SyncServer::startSync() {

        auto instance = hel::SendDataManager::getInstance();
        auto data =  instance.first->serialize();

        printf("%s\n", data.c_str());

        //asio::ip::udp::endpoint dest(asio::ip::address::from_string("127.0.0.1"), 11000);

        //socket.send_to(asio::buffer(data.c_str(), data.length()),
        //                     dest);
        usleep(30000);
        instance.second.unlock();
    }
}

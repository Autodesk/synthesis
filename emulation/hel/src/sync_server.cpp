#include "sync_server.h"
#include "send_data.h"

namespace hel {

    void SyncServer::startSync() {
        auto instance_lock = RoboRIOManager::swapBuffer(hel::RoboRIOManager::Buffer::Send);

        SendDataManager::getInstance()->update();
        auto data = SendDataManager::getInstance()->serialize();

        asio::ip::udp::endpoint dest(asio::ip::address::from_string("127.0.0.1"), 11000);

        socket.send_to(asio::buffer(data.c_str(), data.length()),
                       dest);
        instance_lock.unlock();
    }

}
